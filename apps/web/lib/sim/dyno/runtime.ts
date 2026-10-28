import type { GpsSampleDto } from '@sim-tuning/common/schemas';
import { appendSample } from './gps';

/**
 * Run flow of v2 `DynoRuntimeViewModel` as a pure reducer:
 * "Anfahren" (pre) → start → 10 s countdown → "Vollgas" (acceleration, stopwatch, GPS samples →
 * `geschwindigkeit`) → stop → 5 s countdown → "Ausrollen" (rollout, GPS samples → `ausrollen`) →
 * stop → "Fertig" (done). Reset returns to "Anfahren" and drops the recorded samples.
 */
export type RunPhase =
  'pre' | 'countdownAcceleration' | 'acceleration' | 'countdownRollout' | 'rollout' | 'done';

export const ACCELERATION_COUNTDOWN_MS = 10_000;
export const ROLLOUT_COUNTDOWN_MS = 5_000;

export interface RunState {
  phase: RunPhase;
  /** Timestamp (ms) the current countdown or stopwatch started. */
  phaseStartedAt: number | null;
  /** Milliseconds left in a countdown / elapsed in a measuring phase. */
  countdownMs: number;
  elapsedMs: number;
  geschwindigkeit: GpsSampleDto[];
  ausrollen: GpsSampleDto[];
  /** Latest speed in m/s (also shown outside measuring phases). */
  currentSpeed: number | null;
}

export const initialRunState: RunState = {
  phase: 'pre',
  phaseStartedAt: null,
  countdownMs: 0,
  elapsedMs: 0,
  geschwindigkeit: [],
  ausrollen: [],
  currentSpeed: null,
};

export type RunEvent =
  | { type: 'start'; now: number }
  | { type: 'stop'; now: number }
  | { type: 'tick'; now: number }
  | { type: 'sample'; sample: GpsSampleDto }
  | { type: 'reset' };

export function runReducer(state: RunState, event: RunEvent): RunState {
  switch (event.type) {
    case 'reset':
      return initialRunState;
    case 'start':
      if (state.phase !== 'pre' && state.phase !== 'done') return state;
      return {
        ...initialRunState,
        currentSpeed: state.currentSpeed,
        phase: 'countdownAcceleration',
        phaseStartedAt: event.now,
        countdownMs: ACCELERATION_COUNTDOWN_MS,
      };
    case 'stop':
      if (state.phase === 'acceleration' || state.phase === 'countdownAcceleration') {
        return {
          ...state,
          phase: 'countdownRollout',
          phaseStartedAt: event.now,
          countdownMs: ROLLOUT_COUNTDOWN_MS,
          elapsedMs: 0,
        };
      }
      if (state.phase === 'rollout' || state.phase === 'countdownRollout') {
        return { ...state, phase: 'done', phaseStartedAt: null, countdownMs: 0 };
      }
      return state;
    case 'tick': {
      if (state.phaseStartedAt === null) return state;
      const passed = Math.max(0, event.now - state.phaseStartedAt);
      if (state.phase === 'countdownAcceleration' || state.phase === 'countdownRollout') {
        const total =
          state.phase === 'countdownAcceleration'
            ? ACCELERATION_COUNTDOWN_MS
            : ROLLOUT_COUNTDOWN_MS;
        if (passed >= total) {
          return {
            ...state,
            phase: state.phase === 'countdownAcceleration' ? 'acceleration' : 'rollout',
            phaseStartedAt: state.phaseStartedAt + total,
            countdownMs: 0,
            elapsedMs: passed - total,
          };
        }
        return { ...state, countdownMs: total - passed };
      }
      if (state.phase === 'acceleration' || state.phase === 'rollout') {
        return { ...state, elapsedMs: passed };
      }
      return state;
    }
    case 'sample': {
      const currentSpeed = event.sample.speed ?? state.currentSpeed;
      if (state.phase === 'acceleration') {
        return {
          ...state,
          currentSpeed,
          geschwindigkeit: appendSample(state.geschwindigkeit, event.sample),
        };
      }
      if (state.phase === 'rollout') {
        return { ...state, currentSpeed, ausrollen: appendSample(state.ausrollen, event.sample) };
      }
      return { ...state, currentSpeed };
    }
  }
}

/** Whether GPS needs to be watched in this phase (also during countdowns to get a fix early). */
export const isTracking = (phase: RunPhase) => phase !== 'pre' && phase !== 'done';

/** `mm:ss.cc` like the v2 stopwatch. */
export function formatStopwatch(ms: number): string {
  const total = Math.max(0, Math.floor(ms / 10));
  const cs = total % 100;
  const s = Math.floor(total / 100) % 60;
  const m = Math.floor(total / 6000);
  const pad = (v: number) => String(v).padStart(2, '0');
  return `${pad(m)}:${pad(s)}.${pad(cs)}`;
}
