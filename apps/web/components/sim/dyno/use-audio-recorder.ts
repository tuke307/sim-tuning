'use client';

import { useCallback, useEffect, useRef, useState } from 'react';
import {
  decodeToPcm,
  pickRecorderMimeType,
  RECORDING_CONSTRAINTS,
  type DynoAudio,
} from '@/lib/sim/dyno/audio-input';

export type RecorderStatus = 'idle' | 'requesting' | 'recording' | 'processing';
export type RecorderError = 'denied' | 'unsupported' | 'failed';

interface Session {
  recorder: MediaRecorder;
  stream: MediaStream;
  chunks: Blob[];
  startedAt: number;
}

const stopTracks = (stream: MediaStream) => stream.getTracks().forEach((track) => track.stop());

/**
 * Microphone recording via `MediaRecorder` (webm/opus on Chrome/Firefox, mp4/aac on Safari).
 * The recording is decoded locally into 16-bit samples; nothing is uploaded.
 */
export function useAudioRecorder() {
  const [status, setStatus] = useState<RecorderStatus>('idle');
  const [error, setError] = useState<RecorderError | null>(null);
  const [startedAt, setStartedAt] = useState<number | null>(null);
  const session = useRef<Session | null>(null);

  useEffect(
    () => () => {
      const current = session.current;
      if (current) {
        if (current.recorder.state !== 'inactive') current.recorder.stop();
        stopTracks(current.stream);
      }
    },
    [],
  );

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
    try {
      const mimeType = pickRecorderMimeType((type) => MediaRecorder.isTypeSupported(type));
      const recorder = new MediaRecorder(stream, mimeType ? { mimeType } : undefined);
      const chunks: Blob[] = [];
      recorder.ondataavailable = (event) => {
        if (event.data.size > 0) chunks.push(event.data);
      };
      const now = Date.now();
      session.current = { recorder, stream, chunks, startedAt: now };
      // Timeslice so Safari flushes data periodically.
      recorder.start(1000);
      setStartedAt(now);
      setStatus('recording');
      return true;
    } catch {
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
    setStatus('processing');
    setStartedAt(null);
    try {
      const { recorder, stream, chunks } = current;
      if (recorder.state !== 'inactive') {
        await new Promise<void>((resolve) => {
          recorder.onstop = () => resolve();
          recorder.stop();
        });
      }
      stopTracks(stream);
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

  return { status, error, startedAt, start, stop };
}
