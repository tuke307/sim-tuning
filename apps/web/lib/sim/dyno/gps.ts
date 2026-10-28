import { MAX_DYNO_POINTS, type GpsSampleDto } from '@sim-tuning/common/schemas';

/** Structural subset of `GeolocationPosition` (so helpers are testable in Node). */
export interface PositionLike {
  timestamp: number;
  coords: {
    latitude: number;
    longitude: number;
    accuracy: number | null;
    altitude: number | null;
    altitudeAccuracy: number | null;
    heading: number | null;
    speed: number | null;
  };
}

const finiteOrNull = (value: number | null | undefined): number | null =>
  typeof value === 'number' && Number.isFinite(value) ? value : null;

/**
 * Maps a browser position to the stored `GpsSampleDto` (v2 `GeschwindigkeitModel` /
 * `AusrollenModel`: `speed` in m/s, `zeitpunkt` ISO timestamp). Returns `null` for positions that
 * would fail schema validation (out-of-range coordinates, invalid time).
 */
export function toGpsSample(position: PositionLike): GpsSampleDto | null {
  const { coords } = position;
  const { latitude, longitude } = coords;
  if (!Number.isFinite(latitude) || latitude < -90 || latitude > 90) return null;
  if (!Number.isFinite(longitude) || longitude < -180 || longitude > 180) return null;
  const time = new Date(position.timestamp);
  if (Number.isNaN(time.getTime())) return null;
  const speed = finiteOrNull(coords.speed);
  return {
    latitude,
    longitude,
    speed: speed === null ? null : Math.max(0, speed),
    accuracy: finiteOrNull(coords.accuracy),
    altitude: finiteOrNull(coords.altitude),
    altitudeAccuracy: finiteOrNull(coords.altitudeAccuracy),
    heading: finiteOrNull(coords.heading),
    headingAccuracy: null,
    zeitpunkt: time.toISOString(),
  };
}

/** Appends a sample unless the series is full or the sample duplicates the previous timestamp. */
export function appendSample(series: GpsSampleDto[], sample: GpsSampleDto): GpsSampleDto[] {
  if (series.length >= MAX_DYNO_POINTS) return series;
  const last = series[series.length - 1];
  if (last && last.zeitpunkt && last.zeitpunkt === sample.zeitpunkt) return series;
  return [...series, sample];
}

const EARTH_RADIUS_M = 6371008.8;
const toRad = (deg: number) => (deg * Math.PI) / 180;

/** Great-circle distance in metres. */
export function haversineMeters(
  a: { latitude: number; longitude: number },
  b: { latitude: number; longitude: number },
): number {
  const dLat = toRad(b.latitude - a.latitude);
  const dLon = toRad(b.longitude - a.longitude);
  const h =
    Math.sin(dLat / 2) ** 2 +
    Math.cos(toRad(a.latitude)) * Math.cos(toRad(b.latitude)) * Math.sin(dLon / 2) ** 2;
  return 2 * EARTH_RADIUS_M * Math.asin(Math.min(1, Math.sqrt(h)));
}

export interface SpeedPoint {
  /** Seconds since the first sample. */
  t: number;
  /** Speed in m/s. */
  speed: number;
}

/** Stored samples (`GpsSampleResponseDto`) may carry `null` for any field. */
export interface SampleLike {
  latitude?: number | null;
  longitude?: number | null;
  speed?: number | null;
  zeitpunkt?: string | null;
}

const hasCoords = (s: SampleLike): s is SampleLike & { latitude: number; longitude: number } =>
  typeof s.latitude === 'number' && typeof s.longitude === 'number';

/**
 * Speed over time of a run. Uses the device-reported speed and falls back to the distance between
 * consecutive fixes when the browser does not report one (desktop / some Android browsers).
 */
export function speedSeries(samples: readonly SampleLike[]): SpeedPoint[] {
  const timed = samples
    .map((s) => ({ s, ms: s.zeitpunkt ? Date.parse(s.zeitpunkt) : Number.NaN }))
    .filter((x) => Number.isFinite(x.ms));
  const first = timed[0];
  if (!first) return [];
  return timed.map(({ s, ms }, i) => {
    let speed = typeof s.speed === 'number' && Number.isFinite(s.speed) ? s.speed : null;
    const prev = timed[i - 1];
    if (speed === null && prev && ms > prev.ms && hasCoords(prev.s) && hasCoords(s)) {
      speed = haversineMeters(prev.s, s) / ((ms - prev.ms) / 1000);
    }
    return { t: (ms - first.ms) / 1000, speed: speed ?? 0 };
  });
}

export interface CoastDownFit {
  /** Mean deceleration in m/s² (positive while slowing down). */
  deceleration: number;
  /** Speed at t = 0 of the fitted line, m/s. */
  intercept: number;
  /** Coefficient of determination of the linear fit. */
  r2: number;
  /** Resistance force F = m · a in N (only when a mass is known). */
  resistanceN: number | null;
}

/**
 * Minimal coast-down ("Ausrollen") evaluation. v2 left `DynoLogic.GetAusrollGraphFitted`
 * commented out, so v3 fits a straight line v(t) = v₀ − a·t by least squares and reports the mean
 * deceleration `a` and the driving resistance `F = m·a` it implies. Needs at least 3 points.
 */
export function fitCoastDown(
  points: readonly SpeedPoint[],
  massKg?: number | null,
): CoastDownFit | null {
  const n = points.length;
  if (n < 3) return null;
  let sx = 0;
  let sy = 0;
  for (const p of points) {
    sx += p.t;
    sy += p.speed;
  }
  const mx = sx / n;
  const my = sy / n;
  let sxx = 0;
  let sxy = 0;
  let syy = 0;
  for (const p of points) {
    sxx += (p.t - mx) ** 2;
    sxy += (p.t - mx) * (p.speed - my);
    syy += (p.speed - my) ** 2;
  }
  if (sxx === 0) return null;
  const slope = sxy / sxx;
  const intercept = my - slope * mx;
  const r2 = syy === 0 ? 1 : (sxy * sxy) / (sxx * syy);
  const deceleration = -slope;
  return {
    deceleration,
    intercept,
    r2,
    resistanceN: massKg && massKg > 0 ? massKg * deceleration : null,
  };
}

/** Max speed in m/s of a run (0 for empty runs). */
export function maxSpeed(points: readonly SpeedPoint[]): number {
  return points.reduce((max, p) => Math.max(max, p.speed), 0);
}

export const MS_TO_KMH = 3.6;
