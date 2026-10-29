import { roundTo } from '../utils/round.util';
import { dbscan, type Point2D } from './dbscan';
import { polyfit, polyval } from './polyfit';
import { computeSpectrogram, createSpectrogramSettings, type Spectrogram } from './spectrogram';

/**
 * Audio dyno pipeline from v2 `AudioLogic` + `DynoAudioViewModel`:
 * spectrogram → "hot" bins (magnitude·intensity ≥ 255) → (ms, U/min) points → DBSCAN clusters →
 * degree-5 polynomial over the chosen cluster, sampled every 5 ms.
 */

/** v2 UI defaults: 3000–12000 U/min, ε = 160, FFT 2^15, intensity 100 %. */
export const AUDIO_DYNO_DEFAULTS = {
  frequenzbeginn: 3000,
  frequenzende: 12000,
  epsilon: 160,
  fftSize: 32768,
  intensity: 100,
  minimumPointsPerCluster: 5,
  fitOrder: 5,
  fitStepMs: 5,
} as const;

/** FFT sizes offered by v2 (2^13 … 2^17). */
export const AUDIO_FFT_SIZES = [8192, 16384, 32768, 65536, 131072] as const;

export interface DrehzahlPoint {
  /** Time in ms. */
  x: number;
  /** Engine speed in U/min (single-cylinder: 1 Hz = 60 U/min). */
  y: number;
}

export interface SpectrogramOptions {
  fftSize?: number;
  /** Lower bound of the analysed speed range in U/min. */
  frequenzbeginn?: number;
  /** Upper bound of the analysed speed range in U/min. */
  frequenzende?: number;
}

/** v2 `AudioLogic.CalculateSpectrogram` (stepSize = samples / 1000, band = rpm / 60 Hz). */
export function calculateSpectrogram(
  samples: ArrayLike<number>,
  sampleRate: number,
  options: SpectrogramOptions = {},
): Spectrogram {
  const fftSize = options.fftSize ?? AUDIO_DYNO_DEFAULTS.fftSize;
  const minFreq = Math.trunc((options.frequenzbeginn ?? AUDIO_DYNO_DEFAULTS.frequenzbeginn) / 60);
  const maxFreq = Math.trunc((options.frequenzende ?? AUDIO_DYNO_DEFAULTS.frequenzende) / 60);
  const stepSize = Math.max(1, Math.trunc(samples.length / 1000));
  return computeSpectrogram(
    samples,
    createSpectrogramSettings(sampleRate, fftSize, stepSize, minFreq, maxFreq),
  );
}

/**
 * v2 `CalculateRotationalSpeed`: every bin whose magnitude × intensity saturates at 255 becomes a
 * point. The spectrogram only holds the band from `frequenzbeginn`, so the bin index is offset by
 * `fftIndex1`; v2 omitted that offset and reported every speed ≈ `frequenzbeginn` too low.
 */
export function calculateRotationalSpeed(
  spectrogram: Spectrogram,
  intensityPercent: number = AUDIO_DYNO_DEFAULTS.intensity,
): DrehzahlPoint[] {
  const intensity = intensityPercent / 100;
  const { stepLengthSec, hzPerPixel, fftIndex1 } = spectrogram.settings;
  const points: DrehzahlPoint[] = [];
  spectrogram.ffts.forEach((column, col) => {
    for (let row = 0; row < column.length; row++) {
      if (Math.min(column[row]! * intensity, 255) === 255) {
        points.push({
          x: roundTo(col * stepLengthSec * 1000, 4),
          y: roundTo((fftIndex1 + row) * hzPerPixel * 60, 4),
        });
      }
    }
  });
  return points;
}

/** v2 `CalculateClusters` (minimum 5 points per cluster). */
export function calculateClusters<T extends Point2D>(
  points: readonly T[],
  epsilon: number = AUDIO_DYNO_DEFAULTS.epsilon,
): T[][] {
  return dbscan(points, epsilon, AUDIO_DYNO_DEFAULTS.minimumPointsPerCluster);
}

/**
 * Track joining tolerances: time overlap/gap between clusters, relative rpm jump and the window
 * (ms) at each cluster end whose median speed is compared (short, because the engine accelerates).
 */
export const TRACK_DEFAULTS = { maxGapMs: 500, maxJumpRatio: 0.1, edgeMs: 200 } as const;

interface TrackEnds {
  start: number;
  end: number;
  rpmStart: number;
  rpmEnd: number;
}

const median = (values: number[]) => {
  const sorted = [...values].sort((a, b) => a - b);
  const mid = sorted.length >> 1;
  return sorted.length % 2 ? sorted[mid]! : (sorted[mid - 1]! + sorted[mid]!) / 2;
};

function trackEnds(points: readonly DrehzahlPoint[], edgeMs: number): TrackEnds {
  let start = Infinity;
  let end = -Infinity;
  for (const p of points) {
    start = Math.min(start, p.x);
    end = Math.max(end, p.x);
  }
  return {
    start,
    end,
    rpmStart: median(points.filter((p) => p.x <= start + edgeMs).map((p) => p.y)),
    rpmEnd: median(points.filter((p) => p.x >= end - edgeMs).map((p) => p.y)),
  };
}

/**
 * Joins DBSCAN clusters that continue each other into one engine-speed track. DBSCAN uses one ε
 * for both axes (ms and U/min), so a single pull often breaks into several clusters where the
 * engine order fades or jumps slightly; a cluster is appended to the track whose end it starts
 * near in time (± `maxGapMs`) and speed (± `maxJumpRatio`). Tracks are returned by start time.
 */
export function mergeClusterTracks(
  clusters: readonly (readonly DrehzahlPoint[])[],
  options: Partial<typeof TRACK_DEFAULTS> = {},
): DrehzahlPoint[][] {
  const { maxGapMs, maxJumpRatio, edgeMs } = { ...TRACK_DEFAULTS, ...options };
  const ordered = clusters
    .filter((c) => c.length > 0)
    .map((points) => ({ points, ends: trackEnds(points, edgeMs) }))
    .sort((a, b) => a.ends.start - b.ends.start);
  const tracks: { points: DrehzahlPoint[]; ends: TrackEnds }[] = [];
  for (const cluster of ordered) {
    let best: (typeof tracks)[number] | undefined;
    let bestJump = Infinity;
    for (const track of tracks) {
      if (cluster.ends.start < track.ends.start) continue;
      if (Math.abs(cluster.ends.start - track.ends.end) > maxGapMs) continue;
      const jump = Math.abs(cluster.ends.rpmStart - track.ends.rpmEnd) / track.ends.rpmEnd;
      if (jump <= maxJumpRatio && jump < bestJump) {
        best = track;
        bestJump = jump;
      }
    }
    if (best && cluster.ends.end > best.ends.end) {
      best.points = [...best.points, ...cluster.points];
      best.ends = { ...best.ends, end: cluster.ends.end, rpmEnd: cluster.ends.rpmEnd };
    } else {
      tracks.push({ points: [...cluster.points], ends: cluster.ends });
    }
  }
  return tracks.map((t) => t.points);
}

/**
 * Index of the track most likely to be the engine's firing frequency: among the tracks lasting
 * at least half as long as the longest one, the one with the lowest median speed. Harmonics
 * (2×, 3× …) are often louder and therefore larger clusters, so "largest" picks them wrongly.
 * Returns -1 when there are no tracks.
 */
export function pickFundamentalTrack(tracks: readonly (readonly DrehzahlPoint[])[]): number {
  const spans = tracks.map((t) => {
    if (t.length < AUDIO_DYNO_DEFAULTS.minimumPointsPerCluster) return 0;
    const xs = t.map((p) => p.x);
    return Math.max(...xs) - Math.min(...xs);
  });
  const longest = Math.max(0, ...spans);
  let best = -1;
  let bestRpm = Infinity;
  tracks.forEach((t, i) => {
    if (longest === 0 || spans[i]! < longest / 2) return;
    const rpm = median(t.map((p) => p.y));
    if (rpm < bestRpm) {
      best = i;
      bestRpm = rpm;
    }
  });
  return best;
}

/**
 * v2 `LoadSpecificCluster`: fits a degree-5 polynomial through a cluster and samples it every
 * 5 ms from the first to the last point (exclusive), producing the stored `Drehzahl` curve.
 */
export function fitDrehzahlCurve(
  cluster: readonly DrehzahlPoint[],
  order: number = AUDIO_DYNO_DEFAULTS.fitOrder,
  stepMs: number = AUDIO_DYNO_DEFAULTS.fitStepMs,
): DrehzahlPoint[] {
  if (cluster.length <= order) return [];
  const coefficients = polyfit(
    cluster.map((p) => p.x),
    cluster.map((p) => p.y),
    order,
  );
  let xMin = Infinity;
  let xMax = -Infinity;
  for (const p of cluster) {
    xMin = Math.min(xMin, p.x);
    xMax = Math.max(xMax, p.x);
  }
  const curve: DrehzahlPoint[] = [];
  for (let x = xMin; x < xMax; x += stepMs) curve.push({ x, y: polyval(coefficients, x) });
  return curve;
}

export interface AudioDynoAnalysis {
  spectrogram: Spectrogram;
  points: DrehzahlPoint[];
  /** Engine-speed tracks: DBSCAN clusters joined by `mergeClusterTracks`. */
  clusters: DrehzahlPoint[][];
  /** Suggested track (`pickFundamentalTrack`), -1 if none. */
  fundamental: number;
}

/** DBSCAN clusters joined into tracks plus the suggested fundamental track. */
export function calculateTracks(
  points: readonly DrehzahlPoint[],
  epsilon: number = AUDIO_DYNO_DEFAULTS.epsilon,
): { clusters: DrehzahlPoint[][]; fundamental: number } {
  const clusters = mergeClusterTracks(calculateClusters(points, epsilon));
  return { clusters, fundamental: pickFundamentalTrack(clusters) };
}

/** Runs the whole pipeline in one go — meant for a Web Worker. */
export function analyzeDynoAudio(
  samples: ArrayLike<number>,
  sampleRate: number,
  options: SpectrogramOptions & { intensity?: number; epsilon?: number } = {},
): AudioDynoAnalysis {
  const spectrogram = calculateSpectrogram(samples, sampleRate, options);
  const points = calculateRotationalSpeed(spectrogram, options.intensity);
  return { spectrogram, points, ...calculateTracks(points, options.epsilon) };
}
