'use client';

import { useEffect } from 'react';

interface WakeLockLike {
  release(): Promise<void>;
}

type WakeLockNavigator = Navigator & {
  wakeLock?: { request(type: 'screen'): Promise<WakeLockLike> };
};

/**
 * Keeps the screen on while `active` where the Screen Wake Lock API exists. Browsers drop the lock
 * when the page is hidden, so it is requested again when the page becomes visible. Best effort:
 * a denied request (e.g. low battery) is ignored.
 */
export function useWakeLock(active: boolean) {
  useEffect(() => {
    if (!active || typeof navigator === 'undefined') return;
    const wakeLock = (navigator as WakeLockNavigator).wakeLock;
    if (!wakeLock) return;
    let lock: WakeLockLike | null = null;
    let disposed = false;

    const acquire = () => {
      if (lock || document.visibilityState !== 'visible') return;
      wakeLock
        .request('screen')
        .then((acquired) => {
          if (disposed) void acquired.release().catch(() => {});
          else lock = acquired;
        })
        .catch(() => {});
    };
    const onVisibility = () => {
      if (document.visibilityState === 'visible') {
        // The previous lock was released by the browser when the page was hidden.
        lock = null;
        acquire();
      }
    };

    acquire();
    document.addEventListener('visibilitychange', onVisibility);
    return () => {
      disposed = true;
      document.removeEventListener('visibilitychange', onVisibility);
      void lock?.release().catch(() => {});
      lock = null;
    };
  }, [active]);
}
