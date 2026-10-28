/**
 * Web Worker for the audio dyno: runs spectrogram → RPM points → DBSCAN off the main thread.
 * Audio never leaves the device; the worker only talks to the page that spawned it.
 * Cancelling is done by the page via `worker.terminate()`.
 */
import { handleWorkerMessage, type WorkerResponse } from './audio-analysis';

interface WorkerScope {
  onmessage: ((event: MessageEvent<unknown>) => void) | null;
  postMessage(message: WorkerResponse): void;
}

const scope = self as unknown as WorkerScope;

scope.onmessage = (event) => {
  handleWorkerMessage(event.data, (msg) => scope.postMessage(msg));
};
