'use client';

import { useCallback, useEffect, useRef, useState } from 'react';
import {
  decodeToPcm,
  pickRecorderMimeType,
  RECORDING_CONSTRAINTS,
  type DynoAudio,
} from '@/lib/sim/dyno/audio-input';
import {
  measureLevel,
  QUIET_RMS,
  QUIET_WARNING_MS,
  type InputLevel,
} from '@/lib/sim/dyno/recording-level';
import { useWakeLock } from './use-wake-lock';

export type RecorderStatus = 'idle' | 'requesting' | 'recording' | 'processing';
export type RecorderError = 'denied' | 'unsupported' | 'failed';

interface Session {
  recorder: MediaRecorder;
  stream: MediaStream;
  chunks: Blob[];
  startedAt: number;
  meter: LevelMeter | null;
}

interface LevelMeter {
  context: AudioContext;
  analyser: AnalyserNode;
  buffer: Float32Array<ArrayBuffer>;
}

export interface AudioRecorderOptions {
  /**
   * Called when the page went to the background during a recording. The recording is finished
   * at that point so what was captured so far is kept (mobile browsers mute the microphone of
   * hidden pages). Without this callback recording continues in the background.
   */
  onInterrupted?: (audio: DynoAudio | null) => void;
}

/** How often the elapsed time and input level are refreshed while recording. */
const METER_INTERVAL_MS = 100;

const stopTracks = (stream: MediaStream) => stream.getTracks().forEach((track) => track.stop());

function createLevelMeter(stream: MediaStream): LevelMeter | null {
  try {
    const Ctor =
      window.AudioContext ??
      (window as Window & { webkitAudioContext?: typeof AudioContext }).webkitAudioContext;
    if (!Ctor) return null;
    const context = new Ctor();
    const analyser = context.createAnalyser();
    analyser.fftSize = 2048;
    context.createMediaStreamSource(stream).connect(analyser);
    return { context, analyser, buffer: new Float32Array(analyser.fftSize) };
  } catch {
    // The level meter is optional; recording works without it.
    return null;
  }
}

function closeMeter(meter: LevelMeter | null) {
  if (meter) void meter.context.close().catch(() => {});
}

/**
 * Microphone recording via `MediaRecorder` (webm/opus on Chrome/Firefox, mp4/aac on Safari).
 * The recording is decoded locally into 16-bit samples; nothing is uploaded. While recording the
 * screen is kept awake and the elapsed time plus the input level (AnalyserNode) are reported.
 */
export function useAudioRecorder({ onInterrupted }: AudioRecorderOptions = {}) {
  const [status, setStatus] = useState<RecorderStatus>('idle');
  const [error, setError] = useState<RecorderError | null>(null);
  const [startedAt, setStartedAt] = useState<number | null>(null);
  const [elapsedMs, setElapsedMs] = useState(0);
  const [level, setLevel] = useState<InputLevel | null>(null);
  const [quiet, setQuiet] = useState(false);
  const session = useRef<Session | null>(null);
  const interruptedRef = useRef(onInterrupted);

  useEffect(() => {
    interruptedRef.current = onInterrupted;
  }, [onInterrupted]);

  useWakeLock(status === 'recording');

  useEffect(
    () => () => {
      const current = session.current;
      if (current) {
        if (current.recorder.state !== 'inactive') current.recorder.stop();
        stopTracks(current.stream);
        closeMeter(current.meter);
      }
    },
    [],
  );

  useEffect(() => {
    if (status !== 'recording') return;
    let quietSince: number | null = null;
    const id = window.setInterval(() => {
      const current = session.current;
      if (!current) return;
      const now = Date.now();
      setElapsedMs(now - current.startedAt);
      if (!current.meter) return;
      current.meter.analyser.getFloatTimeDomainData(current.meter.buffer);
      const next = measureLevel(current.meter.buffer);
      setLevel(next);
      if (next.rms < QUIET_RMS) {
        quietSince ??= now;
        setQuiet(now - quietSince >= QUIET_WARNING_MS);
      } else {
        quietSince = null;
        setQuiet(false);
      }
    }, METER_INTERVAL_MS);
    return () => window.clearInterval(id);
  }, [status]);

  /** Finishes a session and decodes what it captured. */
  const finish = useCallback(async (current: Session): Promise<DynoAudio | null> => {
    setStatus('processing');
    setStartedAt(null);
    setLevel(null);
    setQuiet(false);
    try {
      const { recorder, stream, chunks } = current;
      if (recorder.state !== 'inactive') {
        await new Promise<void>((resolve) => {
          recorder.onstop = () => resolve();
          recorder.stop();
        });
      }
      stopTracks(stream);
      closeMeter(current.meter);
      if (chunks.length === 0) return null;
      const blob = new Blob(chunks, { type: recorder.mimeType || chunks[0]?.type || 'audio/webm' });
      const pcm = await decodeToPcm(await blob.arrayBuffer());
      return {
        ...pcm,
        source: 'recording',
        label: new Date(current.startedAt).toLocaleString(),
      };
    } catch {
      setError('failed');
      return null;
    } finally {
      setStatus('idle');
    }
  }, []);

  useEffect(() => {
    if (status !== 'recording') return;
    const onVisibility = () => {
      const current = session.current;
      if (document.visibilityState !== 'hidden' || !current || !interruptedRef.current) return;
      session.current = null;
      void finish(current).then((audio) => interruptedRef.current?.(audio));
    };
    document.addEventListener('visibilitychange', onVisibility);
    return () => document.removeEventListener('visibilitychange', onVisibility);
  }, [status, finish]);

  const start = useCallback(async (): Promise<boolean> => {
    if (session.current) return true;
    setError(null);
    if (
      typeof navigator === 'undefined' ||
      !navigator.mediaDevices?.getUserMedia ||
      typeof MediaRecorder === 'undefined'
    ) {
      setError('unsupported');
      return false;
    }
    setStatus('requesting');
    let stream: MediaStream;
    try {
      stream = await navigator.mediaDevices.getUserMedia(RECORDING_CONSTRAINTS);
    } catch (err) {
      const name = err instanceof DOMException ? err.name : '';
      setError(name === 'NotAllowedError' || name === 'SecurityError' ? 'denied' : 'unsupported');
      setStatus('idle');
      return false;
    }
    let meter: LevelMeter | null = null;
    try {
      const mimeType = pickRecorderMimeType((type) => MediaRecorder.isTypeSupported(type));
      const recorder = new MediaRecorder(stream, mimeType ? { mimeType } : undefined);
      const chunks: Blob[] = [];
      recorder.ondataavailable = (event) => {
        if (event.data.size > 0) chunks.push(event.data);
      };
      const now = Date.now();
      meter = createLevelMeter(stream);
      session.current = { recorder, stream, chunks, startedAt: now, meter };
      // Timeslice so Safari flushes data periodically.
      recorder.start(1000);
      setStartedAt(now);
      setElapsedMs(0);
      setLevel(null);
      setQuiet(false);
      setStatus('recording');
      return true;
    } catch {
      session.current = null;
      closeMeter(meter);
      stopTracks(stream);
      setError('unsupported');
      setStatus('idle');
      return false;
    }
  }, []);

  /** Stops the recording and decodes it. Resolves `null` when nothing was recorded. */
  const stop = useCallback(async (): Promise<DynoAudio | null> => {
    const current = session.current;
    if (!current) return null;
    session.current = null;
    return finish(current);
  }, [finish]);

  return { status, error, startedAt, elapsedMs, level, quiet, start, stop };
}
