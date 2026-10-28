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
 * point. v2 quirk kept: the row index is not offset by the band start, so speeds are relative to
 * `frequenzbeginn`.
 */
export function calculateRotationalSpeed(
  spectrogram: Spectrogram,
  intensityPercent: number = AUDIO_DYNO_DEFAULTS.intensity,
): DrehzahlPoint[] {
  const intensity = intensityPercent / 100;
  const { stepLengthSec, hzPerPixel } = spectrogram.settings;
  const points: DrehzahlPoint[] = [];
  spectrogram.ffts.forEach((column, col) => {
    for (let row = 0; row < column.length; row++) {
      if (Math.min(column[row]! * intensity, 255) === 255) {
        points.push({
          x: roundTo(col * stepLengthSec * 1000, 4),
          y: roundTo(row * hzPerPixel * 60, 4),
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
  clusters: DrehzahlPoint[][];
}

/** Runs the whole pipeline in one go — meant for a Web Worker. */
export function analyzeDynoAudio(
  samples: ArrayLike<number>,
  sampleRate: number,
  options: SpectrogramOptions & { intensity?: number; epsilon?: number } = {},
): AudioDynoAnalysis {
  const spectrogram = calculateSpectrogram(samples, sampleRate, options);
  const points = calculateRotationalSpeed(spectrogram, options.intensity);
  const clusters = calculateClusters(points, options.epsilon);
  return { spectrogram, points, clusters };
}
