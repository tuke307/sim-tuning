import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import {
  calculateClusters,
  calculateRotationalSpeed,
  calculateSpectrogram,
  encodeWavMono,
  fitDrehzahlCurve,
  polyfit,
  polyval,
  readWavMono,
} from './index';

const fixture = (name: string) =>
  fileURLToPath(new URL(`../__fixtures__/${name}`, import.meta.url));
const golden = JSON.parse(readFileSync(fixture('v2-golden.json'), 'utf8')) as Record<string, never>;
const wavBytes = readFileSync(fixture('dyno-acceleration.wav'));
const audio = readWavMono(
  wavBytes.buffer.slice(wavBytes.byteOffset, wavBytes.byteOffset + wavBytes.byteLength),
);

const relClose = (actual: number, expected: number, tol: number, label = '') =>
  assert.ok(
    Math.abs(actual - expected) <= Math.abs(expected) * tol + 1e-9,
    `${label} ${actual} ≉ ${expected}`,
  );

describe('v2 parity — audio dyno on the bundled v2 demo recording', () => {
  const spectrogram = calculateSpectrogram(audio.samples, audio.sampleRate, {
    fftSize: 32768,
    frequenzbeginn: 3000,
    frequenzende: 12000,
  });

  it('reads the 16-bit PCM WAV and builds the same spectrogram grid', () => {
    assert.equal(audio.sampleRate, 44100);
    assert.equal(spectrogram.ffts.length, golden.specCols);
    assert.equal(spectrogram.ffts[0]!.length, golden.specRows);
    assert.equal(spectrogram.settings.hzPerPixel, golden.specHzPerPx);
    assert.equal(spectrogram.settings.stepLengthSec, golden.specSecPerPx);
    const [a, b, c, d] = golden.specSample as unknown as number[];
    const last = spectrogram.ffts[spectrogram.ffts.length - 1]!;
    relClose(spectrogram.ffts[0]![0]!, a!, 1e-9);
    relClose(spectrogram.ffts[0]![10]!, b!, 1e-9);
    relClose(spectrogram.ffts[100]![50]!, c!, 1e-9);
    relClose(last[last.length - 1]!, d!, 1e-9);
  });

  const points = calculateRotationalSpeed(spectrogram, 100);

  it('finds the same RPM points', () => {
    assert.equal(points.length, golden.rpmCount);
    assert.deepEqual(
      points.slice(0, 10).map((p) => [p.x, p.y]),
      golden.rpmHead,
    );
    assert.deepEqual(
      points.slice(-5).map((p) => [p.x, p.y]),
      golden.rpmTail,
    );
  });

  const clusters = calculateClusters(points, 160);

  it('produces the same DBSCAN clusters in the same order', () => {
    assert.deepEqual(
      clusters.map((c) => c.length),
      golden.clusterSizes,
    );
    assert.deepEqual(
      clusters.map((c) => [c[0]!.x, c[0]!.y]),
      golden.clusterFirst,
    );
  });

  it('fits the same degree-5 RPM curve through the largest cluster', () => {
    const largest = [...clusters].sort((a, b) => b.length - a.length)[0]!;
    const coefficients = polyfit(
      largest.map((p) => p.x),
      largest.map((p) => p.y),
      5,
    );
    const [x0, y0, x1, y1, x2, y2] = golden.fitAt as unknown as number[];
    relClose(polyval(coefficients, x0!), y0!, 1e-6, 'start');
    relClose(polyval(coefficients, x1!), y1!, 1e-6, 'middle');
    relClose(polyval(coefficients, x2!), y2!, 1e-6, 'end');
    const curve = fitDrehzahlCurve(largest);
    assert.equal(curve[0]!.x, x0);
    assert.ok(curve.every((p, i) => i === 0 || p.x - curve[i - 1]!.x === 5));
  });

  it('round-trips WAV encoding', () => {
    const encoded = readWavMono(encodeWavMono(audio.samples.subarray(0, 1000), audio.sampleRate));
    assert.deepEqual(Array.from(encoded.samples), Array.from(audio.samples.subarray(0, 1000)));
  });
});
