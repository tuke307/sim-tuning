import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import {
  analyzeDynoAudio,
  calculateSpectrogram,
  fitDrehzahlCurve,
  readWavMono,
} from '@sim-tuning/common/audio';
import {
  calculateSpectrogramChunked,
  DEFAULT_AUDIO_OPTIONS,
  downsample,
  downsampleClusters,
  handleWorkerMessage,
  isAnalyzeRequest,
  validateAnalysisInput,
  type WorkerResponse,
} from './audio-analysis';

const demoPath = fileURLToPath(
  new URL('../../../public/demo/dyno-acceleration.wav', import.meta.url),
);
const bytes = readFileSync(demoPath);
const demo = readWavMono(bytes.buffer.slice(bytes.byteOffset, bytes.byteOffset + bytes.byteLength));

/** Short synthetic engine sweep (mono, 16-bit range) for fast tests. */
function sweep(sampleRate: number, seconds: number): Float64Array {
  const out = new Float64Array(Math.round(sampleRate * seconds));
  let phase = 0;
  for (let i = 0; i < out.length; i++) {
    const hz = 60 + (120 * i) / out.length;
    phase += (2 * Math.PI * hz) / sampleRate;
    out[i] = Math.trunc(20000 * Math.sin(phase));
  }
  return out;
}

describe('audio analysis (worker logic)', () => {
  it('chunked spectrogram equals the common calculateSpectrogram', () => {
    const samples = sweep(22050, 2);
    const options = { fftSize: 8192, frequenzbeginn: 3000, frequenzende: 12000 };
    const reference = calculateSpectrogram(samples, 22050, options);
    const progress: number[] = [];
    const chunked = calculateSpectrogramChunked(
      samples,
      22050,
      options,
      (done) => {
        progress.push(done);
      },
      7,
    )!;
    assert.equal(chunked.ffts.length, reference.ffts.length);
    assert.deepEqual(chunked.settings.height, reference.settings.height);
    chunked.ffts.forEach((col, i) => assert.deepEqual(col, reference.ffts[i]));
    assert.equal(progress.at(-1), reference.ffts.length);
  });

  it('stops when the progress callback cancels', () => {
    const samples = sweep(22050, 2);
    const out = calculateSpectrogramChunked(
      samples,
      22050,
      { fftSize: 8192, frequenzbeginn: 3000, frequenzende: 12000 },
      () => false,
    );
    assert.equal(out, null);
  });

  it('validates input before running', () => {
    const ok = DEFAULT_AUDIO_OPTIONS;
    assert.equal(validateAnalysisInput(400000, 44100, ok), null);
    assert.equal(validateAnalysisInput(1000, 44100, ok), 'tooShort');
    assert.equal(validateAnalysisInput(400000, 100, ok), 'sampleRate');
    assert.equal(validateAnalysisInput(44100 * 200, 44100, ok), 'tooLong');
    assert.equal(validateAnalysisInput(400000, 44100, { ...ok, fftSize: 1000 }), 'fftSize');
    assert.equal(
      validateAnalysisInput(400000, 44100, { ...ok, frequenzbeginn: 9000, frequenzende: 3000 }),
      'frequencyRange',
    );
    assert.equal(validateAnalysisInput(400000, 44100, { ...ok, epsilon: 0 }), 'epsilon');
    assert.equal(validateAnalysisInput(400000, 44100, { ...ok, intensity: -1 }), 'intensity');
  });

  it('rejects malformed worker messages', () => {
    assert.equal(isAnalyzeRequest(null), false);
    assert.equal(
      isAnalyzeRequest({ type: 'analyze', id: 1, samples: [1, 2], sampleRate: 1 }),
      false,
    );
    const posted: WorkerResponse[] = [];
    handleWorkerMessage({ type: 'evil', id: 4 }, (m) => posted.push(m));
    assert.deepEqual(posted, [{ type: 'error', id: 4, error: 'failed' }]);
  });

  it('posts a validation error instead of running', () => {
    const posted: WorkerResponse[] = [];
    handleWorkerMessage(
      {
        type: 'analyze',
        id: 2,
        samples: new Float64Array(100),
        sampleRate: 44100,
        options: DEFAULT_AUDIO_OPTIONS,
      },
      (m) => posted.push(m),
    );
    assert.deepEqual(posted, [{ type: 'error', id: 2, error: 'tooShort' }]);
  });

  it('analyzes the demo recording (v2 point count, joined tracks) and progress', () => {
    const posted: WorkerResponse[] = [];
    handleWorkerMessage(
      {
        type: 'analyze',
        id: 7,
        samples: demo.samples,
        sampleRate: demo.sampleRate,
        options: DEFAULT_AUDIO_OPTIONS,
      },
      (m) => posted.push(m),
    );
    const result = posted.at(-1);
    assert.ok(result && result.type === 'result');
    const progress = posted.filter((m) => m.type === 'progress').map((m) => m.progress);
    assert.ok(progress.length > 10);
    assert.ok(progress.every((p, i) => i === 0 || p >= progress[i - 1]!));
    assert.equal(progress.at(-1), 1);

    const reference = analyzeDynoAudio(demo.samples, demo.sampleRate, DEFAULT_AUDIO_OPTIONS);
    assert.equal(result.result.points.length, reference.points.length);
    assert.equal(result.result.points.length, 8336, 'v2 golden rpmCount');
    assert.deepEqual(
      result.result.clusters.map((c) => c.length),
      reference.clusters.map((c) => c.length),
    );
    // v2's 15 DBSCAN clusters; the two halves of the firing-frequency track are joined.
    assert.equal(result.result.clusters.length, 12);
    assert.equal(result.result.fundamental, reference.fundamental);
    assert.equal(result.result.columns, 912);
    const track = result.result.clusters[result.result.fundamental]!;
    assert.ok(fitDrehzahlCurve(track).length > 1000);
  });
});

describe('downsampling for charts', () => {
  it('keeps first and last point and respects the limit', () => {
    const items = Array.from({ length: 1000 }, (_, i) => i);
    const out = downsample(items, 10);
    assert.equal(out.length, 10);
    assert.equal(out[0], 0);
    assert.equal(out.at(-1), 999);
    assert.deepEqual(downsample([1, 2, 3], 10), [1, 2, 3]);
  });

  it('downsamples clusters proportionally', () => {
    const clusters = [Array(900).fill(0), Array(100).fill(1)];
    const out = downsampleClusters(clusters, 100);
    assert.equal(out[0]!.length, 90);
    assert.equal(out[1]!.length, 10);
  });
});
