'use client';

import { useCallback, useEffect, useReducer, useState } from 'react';
import { toGpsSample } from '@/lib/sim/dyno/gps';
import { initialRunState, isTracking, runReducer } from '@/lib/sim/dyno/runtime';

export type GpsError = 'denied' | 'unavailable' | 'timeout' | 'unsupported' | 'insecure';

interface WakeLockLike {
  release(): Promise<void>;
}

/**
 * GPS run state (v2 `DynoRuntimeViewModel`): watches `navigator.geolocation` with high accuracy
 * while a run is active, drives countdown/stopwatch ticks and keeps the screen awake where the
 * Screen Wake Lock API exists (browsers stop delivering positions in the background).
 */
export function useGpsRun() {
  const [state, dispatch] = useReducer(runReducer, initialRunState);
  const [gpsError, setGpsError] = useState<GpsError | null>(null);
  const tracking = isTracking(state.phase);

  useEffect(() => {
    if (!tracking) return;
    if (typeof navigator === 'undefined' || !('geolocation' in navigator)) {
      queueMicrotask(() => setGpsError('unsupported'));
      return;
    }
    const watchId = navigator.geolocation.watchPosition(
      (position) => {
        setGpsError(null);
        const sample = toGpsSample(position);
        if (sample) dispatch({ type: 'sample', sample });
      },
      (error) => {
        setGpsError(
          error.code === error.PERMISSION_DENIED
            ? window.isSecureContext
              ? 'denied'
              : 'insecure'
            : error.code === error.TIMEOUT
              ? 'timeout'
              : 'unavailable',
        );
      },
      { enableHighAccuracy: true, maximumAge: 0, timeout: 15_000 },
    );
    return () => navigator.geolocation.clearWatch(watchId);
  }, [tracking]);

  useEffect(() => {
    if (!tracking) return;
    const id = window.setInterval(() => dispatch({ type: 'tick', now: Date.now() }), 50);
    return () => window.clearInterval(id);
  }, [tracking]);

  useEffect(() => {
    if (!tracking) return;
    const wakeLock = (
      navigator as Navigator & {
        wakeLock?: { request(type: 'screen'): Promise<WakeLockLike> };
      }
    ).wakeLock;
    if (!wakeLock) return;
    let lock: WakeLockLike | null = null;
    let released = false;
    wakeLock
      .request('screen')
      .then((acquired) => {
        if (released) void acquired.release();
        else lock = acquired;
      })
      .catch(() => {
        // Wake lock is best effort (e.g. denied on low battery).
      });
    return () => {
      released = true;
      void lock?.release().catch(() => {});
    };
  }, [tracking]);

  const start = useCallback(() => {
    setGpsError(null);
    dispatch({ type: 'start', now: Date.now() });
  }, []);
  const stop = useCallback(() => dispatch({ type: 'stop', now: Date.now() }), []);
  const reset = useCallback(() => dispatch({ type: 'reset' }), []);

  return { state, gpsError, start, stop, reset };
}
