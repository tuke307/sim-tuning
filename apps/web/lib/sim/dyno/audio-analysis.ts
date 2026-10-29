import {
  AUDIO_DYNO_DEFAULTS,
  AUDIO_FFT_SIZES,
  calculateRotationalSpeed,
  calculateTracks,
  computeSpectrogram,
  createSpectrogramSettings,
  type DrehzahlPoint,
  type Spectrogram,
} from '@sim-tuning/common/audio';

/**
 * Audio dyno analysis as it runs inside the Web Worker (`audio.worker.ts`). Pure and
 * framework-free so it can be unit-tested in Node. The DSP itself comes from
 * `@sim-tuning/common/audio` (golden-tested v2 ports); this module only adds chunking (for
 * progress/cancel), input validation and the message protocol.
 */

export interface AudioAnalysisOptions {
  /** Lower bound of the analysed speed range in U/min. */
  frequenzbeginn: number;
  /** Upper bound of the analysed speed range in U/min. */
  frequenzende: number;
  /** DBSCAN neighbourhood radius (v2 "Epsilon"). */
  epsilon: number;
  /** FFT window length (power of two, one of `AUDIO_FFT_SIZES`). */
  fftSize: number;
  /** Intensity in percent (v2 divides by 100). */
  intensity: number;
}

export const DEFAULT_AUDIO_OPTIONS: AudioAnalysisOptions = {
  frequenzbeginn: AUDIO_DYNO_DEFAULTS.frequenzbeginn,
  frequenzende: AUDIO_DYNO_DEFAULTS.frequenzende,
  epsilon: AUDIO_DYNO_DEFAULTS.epsilon,
  fftSize: AUDIO_DYNO_DEFAULTS.fftSize,
  intensity: AUDIO_DYNO_DEFAULTS.intensity,
};

/** Upper bounds that keep a single analysis within a few seconds on a phone. */
export const MAX_AUDIO_SECONDS = 120;
export const MIN_SAMPLE_RATE = 8000;
export const MAX_SAMPLE_RATE = 192000;

export type AnalysisValidationError =
  'sampleRate' | 'tooShort' | 'tooLong' | 'frequencyRange' | 'epsilon' | 'fftSize' | 'intensity';

/** Validates options + audio before the (expensive) analysis starts. Returns `null` if valid. */
export function validateAnalysisInput(
  sampleCount: number,
  sampleRate: number,
  options: AudioAnalysisOptions,
): AnalysisValidationError | null {
  if (!Number.isFinite(sampleRate) || sampleRate < MIN_SAMPLE_RATE || sampleRate > MAX_SAMPLE_RATE)
    return 'sampleRate';
  if (!(AUDIO_FFT_SIZES as readonly number[]).includes(options.fftSize)) return 'fftSize';
  if (sampleCount <= options.fftSize) return 'tooShort';
  if (sampleCount > sampleRate * MAX_AUDIO_SECONDS) return 'tooLong';
  const { frequenzbeginn: from, frequenzende: to } = options;
  if (!Number.isFinite(from) || !Number.isFinite(to) || from < 60 || to <= from) {
    return 'frequencyRange';
  }
  if (!Number.isFinite(options.epsilon) || options.epsilon <= 0 || options.epsilon > 100000) {
    return 'epsilon';
  }
  if (!Number.isFinite(options.intensity) || options.intensity <= 0 || options.intensity > 10000) {
    return 'intensity';
  }
  return null;
}

/**
 * Same result as `calculateSpectrogram` from `@sim-tuning/common/audio`, but computed in
 * column chunks so the worker can report progress and stop early. Uses the identical v2 settings
 * (stepSize = samples / 1000, band = U/min / 60) — `audio-analysis.spec.ts` asserts parity.
 */
export function calculateSpectrogramChunked(
  samples: Float64Array,
  sampleRate: number,
  options: Pick<AudioAnalysisOptions, 'fftSize' | 'frequenzbeginn' | 'frequenzende'>,
  onProgress?: (done: number, total: number) => boolean | void,
  chunkColumns = 32,
): Spectrogram | null {
  const minFreq = Math.trunc(options.frequenzbeginn / 60);
  const maxFreq = Math.trunc(options.frequenzende / 60);
  const stepSize = Math.max(1, Math.trunc(samples.length / 1000));
  const settings = createSpectrogramSettings(
    sampleRate,
    options.fftSize,
    stepSize,
    minFreq,
    maxFreq,
  );
  const total = Math.max(0, Math.trunc((samples.length - options.fftSize) / stepSize));
  const ffts: Float64Array[] = [];
  for (let start = 0; start < total; start += chunkColumns) {
    const columns = Math.min(chunkColumns, total - start);
    const from = start * stepSize;
    // `columns * stepSize + fftSize` samples yield exactly `columns` windows.
    const slice = samples.subarray(from, from + columns * stepSize + options.fftSize);
    const part = computeSpectrogram(slice, settings);
    for (let i = 0; i < columns; i++) ffts.push(part.ffts[i]!);
    if (onProgress?.(start + columns, total) === false) return null;
  }
  return { settings, ffts };
}

export interface AnalysisResult {
  points: DrehzahlPoint[];
  /** Engine-speed tracks (DBSCAN clusters joined by `mergeClusterTracks`). */
  clusters: DrehzahlPoint[][];
  /** Suggested track (lowest-order engine track), -1 if none. */
  fundamental: number;
  /** Spectrogram grid size, shown as info. */
  columns: number;
  rows: number;
  durationMs: number;
}

/** Full pipeline: spectrogram → RPM points → DBSCAN tracks. `null` when cancelled. */
export function runAudioAnalysis(
  samples: Float64Array,
  sampleRate: number,
  options: AudioAnalysisOptions,
  onProgress?: (fraction: number) => boolean | void,
): AnalysisResult | null {
  // Spectrogram dominates the runtime (≈ 90 %); points + clustering get the rest.
  const spectrogram = calculateSpectrogramChunked(samples, sampleRate, options, (done, total) =>
    onProgress?.(total === 0 ? 0.9 : (done / total) * 0.9),
  );
  if (!spectrogram) return null;
  const points = calculateRotationalSpeed(spectrogram, options.intensity);
  if (onProgress?.(0.93) === false) return null;
  const { clusters, fundamental } = calculateTracks(points, options.epsilon);
  onProgress?.(1);
  return {
    points,
    clusters,
    fundamental,
    columns: spectrogram.ffts.length,
    rows: spectrogram.settings.height,
    durationMs: (samples.length / sampleRate) * 1000,
  };
}

// ─────────────────────────────────────────────────────────────────────────────
// Worker message protocol
// ─────────────────────────────────────────────────────────────────────────────

export interface AnalyzeRequest {
  type: 'analyze';
  id: number;
  /** 16-bit sample values (−32768…32767) as produced by `readWavMono` / `floatToPcm16`. */
  samples: Float64Array;
  sampleRate: number;
  options: AudioAnalysisOptions;
}

export type WorkerResponse =
  | { type: 'progress'; id: number; progress: number }
  | { type: 'result'; id: number; result: AnalysisResult }
  | { type: 'error'; id: number; error: AnalysisValidationError | 'failed' };

/** Type guard for incoming worker messages (the worker treats every message as untrusted). */
export function isAnalyzeRequest(data: unknown): data is AnalyzeRequest {
  if (!data || typeof data !== 'object') return false;
  const msg = data as Record<string, unknown>;
  const options = msg.options as Record<string, unknown> | undefined;
  return (
    msg.type === 'analyze' &&
    typeof msg.id === 'number' &&
    msg.samples instanceof Float64Array &&
    typeof msg.sampleRate === 'number' &&
    !!options &&
    ['frequenzbeginn', 'frequenzende', 'epsilon', 'fftSize', 'intensity'].every(
      (key) => typeof options[key] === 'number',
    )
  );
}

/**
 * Handles one worker message and posts progress/result/error back. Progress is throttled to
 * whole percent steps to keep the message channel quiet.
 */
export function handleWorkerMessage(data: unknown, post: (msg: WorkerResponse) => void): void {
  if (!isAnalyzeRequest(data)) {
    const id =
      typeof (data as { id?: unknown })?.id === 'number' ? (data as { id: number }).id : -1;
    post({ type: 'error', id, error: 'failed' });
    return;
  }
  const { id, samples, sampleRate, options } = data;
  const invalid = validateAnalysisInput(samples.length, sampleRate, options);
  if (invalid) {
    post({ type: 'error', id, error: invalid });
    return;
  }
  try {
    let lastPercent = -1;
    const result = runAudioAnalysis(samples, sampleRate, options, (fraction) => {
      const percent = Math.floor(fraction * 100);
      if (percent !== lastPercent) {
        lastPercent = percent;
        post({ type: 'progress', id, progress: percent / 100 });
      }
    });
    if (result) post({ type: 'result', id, result });
  } catch {
    post({ type: 'error', id, error: 'failed' });
  }
}

// ─────────────────────────────────────────────────────────────────────────────
// Display helpers
// ─────────────────────────────────────────────────────────────────────────────

/** Deterministic stride sampling so SVG charts stay fast; keeps first and last point. */
export function downsample<T>(items: readonly T[], max: number): T[] {
  if (items.length <= max || max < 2) return items.slice();
  const out: T[] = [];
  const step = (items.length - 1) / (max - 1);
  for (let i = 0; i < max; i++) out.push(items[Math.round(i * step)]!);
  return out;
}

/** Downsamples clusters proportionally so the total stays below `max` points. */
export function downsampleClusters<T>(clusters: readonly (readonly T[])[], max: number): T[][] {
  const total = clusters.reduce((sum, c) => sum + c.length, 0);
  if (total <= max) return clusters.map((c) => c.slice());
  return clusters.map((c) => downsample(c, Math.max(2, Math.floor((c.length / total) * max))));
}
