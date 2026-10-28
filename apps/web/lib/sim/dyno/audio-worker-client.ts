import type {
  AnalysisResult,
  AnalysisValidationError,
  AudioAnalysisOptions,
  WorkerResponse,
} from './audio-analysis';

export class AudioAnalysisError extends Error {
  constructor(public readonly code: AnalysisValidationError | 'failed' | 'unsupported') {
    super(`Audio analysis failed: ${code}`);
    this.name = 'AudioAnalysisError';
  }
}

export interface RunningAnalysis {
  promise: Promise<AnalysisResult>;
  /** Terminates the worker; the promise then rejects with an `AbortError`. */
  cancel: () => void;
}

let nextId = 1;

/**
 * Starts the audio analysis in a dedicated Web Worker (one worker per run, terminated afterwards
 * so cancel is immediate even in the middle of an FFT). The samples are copied once and the copy
 * is transferred, so the caller keeps its buffer for re-runs with other parameters.
 */
export function startAudioAnalysis(
  samples: Float64Array,
  sampleRate: number,
  options: AudioAnalysisOptions,
  onProgress: (fraction: number) => void,
): RunningAnalysis {
  if (typeof Worker === 'undefined') {
    return { promise: Promise.reject(new AudioAnalysisError('unsupported')), cancel: () => {} };
  }
  const worker = new Worker(new URL('./audio.worker.ts', import.meta.url), { type: 'module' });
  const id = nextId++;
  let settle: { reject: (err: unknown) => void } | null = null;

  const promise = new Promise<AnalysisResult>((resolve, reject) => {
    settle = { reject };
    worker.onmessage = (event: MessageEvent<WorkerResponse>) => {
      const msg = event.data;
      if (!msg || msg.id !== id) return;
      if (msg.type === 'progress') onProgress(msg.progress);
      else if (msg.type === 'result') {
        worker.terminate();
        resolve(msg.result);
      } else {
        worker.terminate();
        reject(new AudioAnalysisError(msg.error));
      }
    };
    worker.onerror = () => {
      worker.terminate();
      reject(new AudioAnalysisError('failed'));
    };
  });

  const copy = new Float64Array(samples);
  worker.postMessage({ type: 'analyze', id, samples: copy, sampleRate, options }, [copy.buffer]);

  return {
    promise,
    cancel: () => {
      worker.terminate();
      settle?.reject(new DOMException('Analysis cancelled', 'AbortError'));
    },
  };
}
