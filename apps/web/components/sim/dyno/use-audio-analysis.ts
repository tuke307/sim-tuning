'use client';

import { useCallback, useEffect, useRef, useState } from 'react';
import type { AnalysisResult, AudioAnalysisOptions } from '@/lib/sim/dyno/audio-analysis';
import {
  AudioAnalysisError,
  startAudioAnalysis,
  type RunningAnalysis,
} from '@/lib/sim/dyno/audio-worker-client';
import type { DynoAudio } from '@/lib/sim/dyno/audio-input';

export type AnalysisErrorCode = AudioAnalysisError['code'] | 'cancelled';

export interface AudioAnalysisState {
  status: 'idle' | 'running' | 'done' | 'error';
  progress: number;
  result: AnalysisResult | null;
  /** Options the current `result` was computed with. */
  options: AudioAnalysisOptions | null;
  error: AnalysisErrorCode | null;
}

const IDLE: AudioAnalysisState = {
  status: 'idle',
  progress: 0,
  result: null,
  options: null,
  error: null,
};

/** Runs the audio dyno pipeline in a Web Worker with progress and cancel. */
export function useAudioAnalysis() {
  const [state, setState] = useState<AudioAnalysisState>(IDLE);
  const running = useRef<RunningAnalysis | null>(null);

  useEffect(() => () => running.current?.cancel(), []);

  const run = useCallback((audio: DynoAudio, options: AudioAnalysisOptions) => {
    running.current?.cancel();
    setState((prev) => ({ ...prev, status: 'running', progress: 0, error: null }));
    const job = startAudioAnalysis(audio.samples, audio.sampleRate, options, (progress) =>
      setState((prev) => (prev.status === 'running' ? { ...prev, progress } : prev)),
    );
    running.current = job;
    job.promise.then(
      (result) => {
        if (running.current !== job) return;
        running.current = null;
        setState({ status: 'done', progress: 1, result, options, error: null });
      },
      (err: unknown) => {
        if (running.current !== job) return;
        running.current = null;
        const code: AnalysisErrorCode =
          err instanceof AudioAnalysisError
            ? err.code
            : err instanceof DOMException && err.name === 'AbortError'
              ? 'cancelled'
              : 'failed';
        setState((prev) => ({ ...prev, status: 'error', progress: 0, error: code }));
      },
    );
  }, []);

  const cancel = useCallback(() => {
    const job = running.current;
    if (!job) return;
    running.current = null;
    job.cancel();
    setState((prev) => ({ ...prev, status: 'error', progress: 0, error: 'cancelled' }));
  }, []);

  const reset = useCallback(() => {
    running.current?.cancel();
    running.current = null;
    setState(IDLE);
  }, []);

  return { state, run, cancel, reset };
}
