import { fftInPlace, isPowerOfTwo } from './fft';

/**
 * Port of the vendored v2 `Spectrogram` library (originally swharden/Spectrogram, MIT License,
 * Copyright (c) 2020 Scott W Harden). Hann window, magnitude / fftSize, frequency band limited to
 * [minFreq, maxFreq].
 */
export interface SpectrogramSettings {
  sampleRate: number;
  fftSize: number;
  stepSize: number;
  fftIndex1: number;
  fftIndex2: number;
  height: number;
  hzPerPixel: number;
  stepLengthSec: number;
  window: Float64Array;
}

export function createSpectrogramSettings(
  sampleRate: number,
  fftSize: number,
  stepSize: number,
  minFreq = 0,
  maxFreq = Number.POSITIVE_INFINITY,
): SpectrogramSettings {
  if (!isPowerOfTwo(fftSize)) throw new Error('FFT size must be a power of 2');
  if (!Number.isInteger(stepSize) || stepSize < 1) throw new Error('Step size must be >= 1');
  const min = Math.max(minFreq, 0);
  const nyquist = Math.trunc(sampleRate / 2);
  const hzPerPixel = sampleRate / fftSize;
  const fftIndex1 = min === 0 ? 0 : Math.trunc(min / hzPerPixel);
  const fftIndex2 = maxFreq >= nyquist ? fftSize / 2 : Math.trunc(maxFreq / hzPerPixel);
  const window = new Float64Array(fftSize);
  for (let n = 0; n < fftSize; n++) {
    window[n] = 0.5 - 0.5 * Math.cos((2 * Math.PI * n) / (fftSize - 1));
  }
  return {
    sampleRate,
    fftSize,
    stepSize,
    fftIndex1,
    fftIndex2,
    height: fftIndex2 - fftIndex1,
    hzPerPixel,
    stepLengthSec: stepSize / sampleRate,
    window,
  };
}

export interface Spectrogram {
  settings: SpectrogramSettings;
  /** One magnitude column per time step, `settings.height` rows each. */
  ffts: Float64Array[];
}

/** Computes every complete FFT window of `audio` (v2 `Spectrogram.Add(audio, process: true)`). */
export function computeSpectrogram(
  audio: ArrayLike<number>,
  settings: SpectrogramSettings,
): Spectrogram {
  const { fftSize, stepSize, window, height, fftIndex1 } = settings;
  const count = Math.trunc((audio.length - fftSize) / stepSize);
  const ffts: Float64Array[] = [];
  const re = new Float64Array(fftSize);
  const im = new Float64Array(fftSize);
  for (let col = 0; col < count; col++) {
    const offset = col * stepSize;
    for (let i = 0; i < fftSize; i++) {
      re[i] = (audio[offset + i] ?? 0) * window[i]!;
      im[i] = 0;
    }
    fftInPlace(re, im);
    const column = new Float64Array(height);
    for (let i = 0; i < height; i++) {
      const k = fftIndex1 + i;
      column[i] = Math.hypot(re[k]!, im[k]!) / fftSize;
    }
    ffts.push(column);
  }
  return { settings, ffts };
}
