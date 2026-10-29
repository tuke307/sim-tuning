/** Input level of the microphone while recording, derived from one analyser frame. */
export interface InputLevel {
  /** Root mean square of the frame, 0…1. */
  rms: number;
  /** Largest absolute sample of the frame, 0…1. */
  peak: number;
}

export type LevelState = 'low' | 'ok' | 'clipping';

/** Below this RMS (≈ −34 dBFS) the engine is too far away or the mic is covered. */
export const LOW_LEVEL_RMS = 0.02;
/** Samples this close to full scale mean the input is overdriven. */
export const CLIPPING_PEAK = 0.98;
/** A level below this RMS (≈ −46 dBFS) for {@link QUIET_WARNING_MS} suggests no engine sound. */
export const QUIET_RMS = 0.005;
export const QUIET_WARNING_MS = 3000;
/** Lower end of the meter scale in dBFS. */
const METER_FLOOR_DB = -60;

/** RMS and peak of a block of float samples (−1…1). */
export function measureLevel(samples: ArrayLike<number>): InputLevel {
  const n = samples.length;
  if (n === 0) return { rms: 0, peak: 0 };
  let sum = 0;
  let peak = 0;
  for (let i = 0; i < n; i++) {
    const v = samples[i] ?? 0;
    sum += v * v;
    const abs = Math.abs(v);
    if (abs > peak) peak = abs;
  }
  return { rms: Math.min(1, Math.sqrt(sum / n)), peak: Math.min(1, peak) };
}

export function classifyLevel(level: InputLevel): LevelState {
  if (level.peak >= CLIPPING_PEAK) return 'clipping';
  if (level.rms < LOW_LEVEL_RMS) return 'low';
  return 'ok';
}

/** Meter fill 0…1 on a logarithmic (dBFS) scale, so quiet input still moves the bar. */
export function meterFraction(rms: number): number {
  if (!(rms > 0)) return 0;
  const db = 20 * Math.log10(rms);
  return Math.min(1, Math.max(0, (db - METER_FLOOR_DB) / -METER_FLOOR_DB));
}

/** Elapsed recording time as `mm:ss`. */
export function formatClock(ms: number): string {
  const total = Math.max(0, Math.floor(ms / 1000));
  const pad = (v: number) => String(v).padStart(2, '0');
  return `${pad(Math.floor(total / 60))}:${pad(total % 60)}`;
}
