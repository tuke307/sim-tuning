import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import {
  calculateClusters,
  calculateRotationalSpeed,
  calculateSpectrogram,
  calculateTracks,
  encodeWavMono,
  fitDrehzahlCurve,
  polyfit,
  polyval,
  readWavMono,
} from './index';
import { getLeistungskurve, wattToPs } from '../logic/dyno';

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
  // v2 reported speeds relative to the band start; v3 adds it back (audit D1).
  const bandStartRpm = spectrogram.settings.fftIndex1 * spectrogram.settings.hzPerPixel * 60;

  it('finds the same RPM points, shifted by the band start', () => {
    assert.equal(points.length, golden.rpmCount);
    const v2 = (p: { x: number; y: number }) => [p.x, Math.round((p.y - bandStartRpm) * 1e3) / 1e3];
    const golden3 = (rows: unknown) =>
      (rows as [number, number][]).map(([x, y]) => [x, Math.round(y * 1e3) / 1e3]);
    assert.deepEqual(points.slice(0, 10).map(v2), golden3(golden.rpmHead));
    assert.deepEqual(points.slice(-5).map(v2), golden3(golden.rpmTail));
  });

  const clusters = calculateClusters(points, 160);

  it('produces the same DBSCAN clusters in the same order', () => {
    assert.deepEqual(
      clusters.map((c) => c.length),
      golden.clusterSizes,
    );
    assert.deepEqual(
      clusters.map((c) => [c[0]!.x, Math.round((c[0]!.y - bandStartRpm) * 1e3) / 1e3]),
      (golden.clusterFirst as unknown as [number, number][]).map(([x, y]) => [
        x,
        Math.round(y * 1e3) / 1e3,
      ]),
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
    relClose(polyval(coefficients, x0!) - bandStartRpm, y0!, 1e-6, 'start');
    relClose(polyval(coefficients, x1!) - bandStartRpm, y1!, 1e-6, 'middle');
    relClose(polyval(coefficients, x2!) - bandStartRpm, y2!, 1e-6, 'end');
    const curve = fitDrehzahlCurve(largest);
    assert.equal(curve[0]!.x, x0);
    assert.ok(curve.every((p, i) => i === 0 || p.x - curve[i - 1]!.x === 5));
  });

  it('round-trips WAV encoding', () => {
    const encoded = readWavMono(encodeWavMono(audio.samples.subarray(0, 1000), audio.sampleRate));
    assert.deepEqual(Array.from(encoded.samples), Array.from(audio.samples.subarray(0, 1000)));
  });
});

/**
 * The bundled demo recording is the sample run of the SIP HorsePower Dyno app, which ships its own
 * result for it (`dyno-acceleration.sip-result.txt`: 220 kg, c_w 0.5, A 0.75 m², 5.5 engine
 * revolutions per metre, 20 °C, 1 010 hPa). Independent reference for the v3 pipeline.
 */
describe('SIP reference — demo recording', () => {
  const spectrogram = calculateSpectrogram(audio.samples, audio.sampleRate);
  const points = calculateRotationalSpeed(spectrogram);
  const { clusters, fundamental } = calculateTracks(points);
  const sip = readFileSync(fixture('dyno-acceleration.sip-result.txt'), 'utf8')
    .split('\n')
    .filter((line) => /^\d+;/.test(line))
    .map((line) => line.split(';').map(Number) as [number, number]);
  const sipRpm = sip.map(([rpm]) => rpm);
  const sipPeak = sip.reduce((a, b) => (b[1] > a[1] ? b : a));

  it('joins the split firing-frequency clusters and picks them over the louder harmonic', () => {
    const track = clusters[fundamental]!;
    const ys = fitDrehzahlCurve(track).map((p) => p.y);
    relClose(Math.min(...ys), Math.min(...sipRpm), 0.1, 'start rpm');
    relClose(Math.max(...ys), Math.max(...sipRpm), 0.05, 'end rpm');
  });

  it('reproduces the SIP peak power position and magnitude', () => {
    const curve = fitDrehzahlCurve(clusters[fundamental]!);
    const ratio = 2 * Math.PI * 0.277 * 5.5; // 5.5 engine rev/m with a 0.277 m wheel
    const power = getLeistungskurve(
      curve.filter((_, i) => i % 20 === 0).map((p) => ({ zeit: p.x, drehzahl: p.y })),
      ratio,
      0.277,
      { masse: 220, luftdichte: 1.2, cw: 0.5, frontA: 0.75, rollwiderstand: 0.015, steigung: 0 },
    );
    const peak = power.reduce((a, b) => (b.watt > a.watt ? b : a));
    relClose(peak.drehzahl, sipPeak[0], 0.05, 'peak rpm');
    relClose(wattToPs(peak.watt), sipPeak[1], 0.2, 'peak PS');
  });
});
