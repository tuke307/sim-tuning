export const ANALYTICS_OPT_OUT_KEY = 'analytics-opt-out';

const OPT_OUT_CHANGE_EVENT = 'analytics-opt-out-change';

/** True when this device has opted out of anonymous pageview statistics. */
export function isAnalyticsOptedOut(): boolean {
  if (typeof window === 'undefined') return false;
  try {
    return window.localStorage.getItem(ANALYTICS_OPT_OUT_KEY) === 'true';
  } catch {
    // Storage blocked (e.g. private mode): nothing persisted, so nothing to honour.
    return false;
  }
}

export function setAnalyticsOptOut(optedOut: boolean): void {
  if (typeof window === 'undefined') return;
  try {
    if (optedOut) {
      window.localStorage.setItem(ANALYTICS_OPT_OUT_KEY, 'true');
    } else {
      window.localStorage.removeItem(ANALYTICS_OPT_OUT_KEY);
    }
  } catch {
    // Ignore storage failures; the UI simply keeps its previous state.
  }
  window.dispatchEvent(new Event(OPT_OUT_CHANGE_EVENT));
}

/** Subscribe to opt-out changes (this tab and other tabs). Compatible with useSyncExternalStore. */
export function subscribeAnalyticsOptOut(onChange: () => void): () => void {
  const onStorage = (event: StorageEvent) => {
    if (event.key === null || event.key === ANALYTICS_OPT_OUT_KEY) onChange();
  };
  window.addEventListener(OPT_OUT_CHANGE_EVENT, onChange);
  window.addEventListener('storage', onStorage);
  return () => {
    window.removeEventListener(OPT_OUT_CHANGE_EVENT, onChange);
    window.removeEventListener('storage', onStorage);
  };
}
