'use client';

import { useCallback } from 'react';
import { usePathname, useRouter, useSearchParams } from 'next/navigation';
import { confirmLeave } from './leave-guard';

/** Query parameter holding the top-level view (`home`, a module or `profile`). */
export const NAV_VIEW_PARAM = 'm';
/** Query parameter holding the sub-tab of the current module. */
export const NAV_SUBTAB_PARAM = 't';

/**
 * Sub-tab of the current module, kept in the URL (`?t=`) so reload, back and shared links restore
 * it. Unknown values fall back to `fallback`. Switching waits for the unsaved-changes guard and
 * replaces the history entry (module switches push, sub-tabs do not).
 */
export function useSubTab<K extends string>(
  allowed: readonly K[],
  fallback: K,
): [K, (next: K) => void] {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const raw = searchParams.get(NAV_SUBTAB_PARAM);
  const current = raw && (allowed as readonly string[]).includes(raw) ? (raw as K) : fallback;

  const setTab = useCallback(
    (next: K) => {
      if (next === current) return;
      void confirmLeave().then((ok) => {
        if (!ok) return;
        const params = new URLSearchParams(window.location.search);
        params.set(NAV_SUBTAB_PARAM, next);
        router.replace(`${pathname}?${params.toString()}`, { scroll: false });
      });
    },
    [current, pathname, router],
  );

  return [current, setTab];
}
