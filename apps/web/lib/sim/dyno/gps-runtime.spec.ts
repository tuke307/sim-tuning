import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import { GpsSampleSchema, type GpsSampleDto } from '@sim-tuning/common/schemas';
import {
  appendSample,
  fitCoastDown,
  haversineMeters,
  speedSeries,
  toGpsSample,
  type PositionLike,
} from './gps';
import {
  ACCELERATION_COUNTDOWN_MS,
  formatStopwatch,
  initialRunState,
  isTracking,
  ROLLOUT_COUNTDOWN_MS,
  runReducer,
  type RunState,
} from './runtime';

const position = (
  overrides: Partial<PositionLike['coords']> = {},
  timestamp = 0,
): PositionLike => ({
  timestamp: Date.UTC(2025, 5, 1, 12, 0, 0) + timestamp,
  coords: {
    latitude: 51.05,
    longitude: 13.74,
    accuracy: 4,
    altitude: null,
    altitudeAccuracy: null,
    heading: Number.NaN,
    speed: 12.5,
    ...overrides,
  },
});

describe('GPS sample mapping', () => {
  it('maps a position to a schema-valid sample with ISO zeitpunkt', () => {
    const sample = toGpsSample(position())!;
    assert.equal(sample.speed, 12.5);
    assert.equal(sample.heading, null, 'NaN heading becomes null');
    assert.equal(sample.zeitpunkt, '2025-06-01T12:00:00.000Z');
    assert.ok(GpsSampleSchema.safeParse(sample).success);
  });

  it('rejects invalid coordinates and clamps negative speeds', () => {
    assert.equal(toGpsSample(position({ latitude: 91 })), null);
    assert.equal(toGpsSample(position({ longitude: Number.NaN })), null);
    assert.equal(toGpsSample(position({ speed: -1 }))!.speed, 0);
  });

  it('skips duplicate timestamps', () => {
    const a = toGpsSample(position())!;
    const series = appendSample(appendSample([], a), a);
    assert.equal(series.length, 1);
  });

  it('derives speed from distance when the device reports none', () => {
    const d = haversineMeters({ latitude: 0, longitude: 0 }, { latitude: 0, longitude: 0.001 });
    assert.ok(Math.abs(d - 111.2) < 0.5);
    const samples = [
      toGpsSample(position({ speed: null, longitude: 0, latitude: 0 }, 0))!,
      toGpsSample(position({ speed: null, longitude: 0.001, latitude: 0 }, 10_000))!,
    ];
    const series = speedSeries(samples);
    assert.equal(series[1]!.t, 10);
    assert.ok(Math.abs(series[1]!.speed - d / 10) < 1e-9);
  });

  it('fits a coast-down line and derives the driving resistance', () => {
    const points = [0, 1, 2, 3, 4].map((t) => ({ t, speed: 20 - 0.5 * t }));
    const fit = fitCoastDown(points, 200)!;
    assert.ok(Math.abs(fit.deceleration - 0.5) < 1e-12);
    assert.ok(Math.abs(fit.intercept - 20) < 1e-12);
    assert.ok(Math.abs(fit.r2 - 1) < 1e-12);
    assert.ok(Math.abs(fit.resistanceN! - 100) < 1e-9);
    assert.equal(fitCoastDown(points.slice(0, 2)), null);
    assert.equal(fitCoastDown(points)!.resistanceN, null);
  });
});

describe('GPS run state machine (v2 DynoRuntimeViewModel)', () => {
  const sample = (ms: number, speed: number): GpsSampleDto => toGpsSample(position({ speed }, ms))!;

  it('walks pull-away → countdown → full throttle → coast-down → done', () => {
    let s: RunState = runReducer(initialRunState, { type: 'start', now: 0 });
    assert.equal(s.phase, 'countdownAcceleration');
    assert.ok(isTracking(s.phase));
    s = runReducer(s, { type: 'sample', sample: sample(1, 1) });
    assert.equal(s.geschwindigkeit.length, 0, 'no samples during the countdown');
    s = runReducer(s, { type: 'tick', now: 4000 });
    assert.equal(s.countdownMs, ACCELERATION_COUNTDOWN_MS - 4000);
    s = runReducer(s, { type: 'tick', now: ACCELERATION_COUNTDOWN_MS + 250 });
    assert.equal(s.phase, 'acceleration');
    assert.equal(s.elapsedMs, 250);
    s = runReducer(s, { type: 'sample', sample: sample(11_000, 5) });
    s = runReducer(s, { type: 'sample', sample: sample(12_000, 9) });
    assert.equal(s.geschwindigkeit.length, 2);
    assert.equal(s.currentSpeed, 9);

    s = runReducer(s, { type: 'stop', now: 20_000 });
    assert.equal(s.phase, 'countdownRollout');
    s = runReducer(s, { type: 'tick', now: 20_000 + ROLLOUT_COUNTDOWN_MS });
    assert.equal(s.phase, 'rollout');
    s = runReducer(s, { type: 'sample', sample: sample(26_000, 8) });
    assert.equal(s.ausrollen.length, 1);
    s = runReducer(s, { type: 'stop', now: 30_000 });
    assert.equal(s.phase, 'done');
    assert.equal(isTracking(s.phase), false);
    assert.equal(s.geschwindigkeit.length, 2, 'data kept until reset');

    s = runReducer(s, { type: 'reset' });
    assert.deepEqual(s, initialRunState);
  });

  it('ignores start while running and stop before starting', () => {
    const running = runReducer(initialRunState, { type: 'start', now: 0 });
    assert.equal(runReducer(running, { type: 'start', now: 5 }), running);
    assert.equal(runReducer(initialRunState, { type: 'stop', now: 5 }), initialRunState);
  });

  it('formats the stopwatch like v2', () => {
    assert.equal(formatStopwatch(0), '00:00.00');
    assert.equal(formatStopwatch(61_234), '01:01.23');
  });
});
