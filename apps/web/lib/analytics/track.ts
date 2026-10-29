import {
  ANALYTICS_MAX_URL_LENGTH,
  stripUrlQuery,
  type AnalyticsEventName,
} from '@sim-tuning/common/schemas';
import { isAnalyticsOptedOut } from './opt-out';

/**
 * Same-origin first-party collector (NestJS, see apps/api web-analytics module). The browser
 * never contacts Umami directly and never reads screen size, navigator.language, document.title
 * or other device information (§25 TDDDG); the server only forwards path, referrer and event data.
 */
export const ANALYTICS_COLLECT_PATH = '/api/analytics/collect';

// Defence in depth: tokens are passed via query strings (always stripped), but never send a path
// that contains a long opaque segment that could be a token, id or secret.
const TOKEN_LIKE_SEGMENT = /^[A-Za-z0-9_-]{24,}$/;

export interface PageviewPayload {
  type: 'pageview';
  url: string;
  referrer?: string;
}

/** Event data: low-cardinality values only — never PII, weights, heart rate or body data. */
export type AnalyticsEventData = Record<string, string | number | boolean | undefined>;

export interface EventPayload {
  type: 'event';
  name: AnalyticsEventName;
  url: string;
  data?: Record<string, string | number | boolean>;
}

export function containsTokenLikeSegment(path: string): boolean {
  return path.split('/').some((segment) => TOKEN_LIKE_SEGMENT.test(segment));
}

/** Returns the cross-origin referrer without query/fragment, or undefined. */
export function sanitizeReferrer(referrer: string, currentOrigin: string): string | undefined {
  if (!referrer) return undefined;
  try {
    const parsed = new URL(referrer);
    if (parsed.protocol !== 'https:' && parsed.protocol !== 'http:') return undefined;
    if (parsed.origin === currentOrigin) return undefined;
    const value = stripUrlQuery(`${parsed.origin}${parsed.pathname}`);
    return value.length <= ANALYTICS_MAX_URL_LENGTH ? value : parsed.origin;
  } catch {
    return undefined;
  }
}

/** Builds the collector payload, or null if the path must not be tracked. */
export function buildPageviewPayload(
  path: string,
  referrer: string | undefined,
  currentOrigin: string,
): PageviewPayload | null {
  const url = stripUrlQuery(path);
  if (!url.startsWith('/') || url.startsWith('//')) return null;
  if (url.length > ANALYTICS_MAX_URL_LENGTH || containsTokenLikeSegment(url)) return null;

  const safeReferrer = referrer ? sanitizeReferrer(referrer, currentOrigin) : undefined;
  return safeReferrer
    ? { type: 'pageview', url, referrer: safeReferrer }
    : { type: 'pageview', url };
}

/** Builds the collector payload for a custom event (current path, undefined values dropped). */
export function buildEventPayload(
  name: AnalyticsEventName,
  data: AnalyticsEventData,
  path: string,
): EventPayload {
  const url = stripUrlQuery(path);
  const safeUrl =
    url.startsWith('/') && !url.startsWith('//') && !containsTokenLikeSegment(url) ? url : '/';
  const entries = Object.entries(data)
    .filter((entry): entry is [string, string | number | boolean] => entry[1] !== undefined)
    .slice(0, 10)
    .map(([key, value]): [string, string | number | boolean] => [
      key,
      typeof value === 'string' ? value.slice(0, 100) : value,
    ]);
  return entries.length > 0
    ? {
        type: 'event',
        name,
        url: safeUrl.slice(0, ANALYTICS_MAX_URL_LENGTH),
        data: Object.fromEntries(entries),
      }
    : { type: 'event', name, url: safeUrl.slice(0, ANALYTICS_MAX_URL_LENGTH) };
}

function isTrackingEnabled(): boolean {
  if (typeof window === 'undefined') return false;
  if (process.env.NODE_ENV !== 'production') return false;
  return !isAnalyticsOptedOut();
}

function send(payload: PageviewPayload | EventPayload): void {
  try {
    void fetch(ANALYTICS_COLLECT_PATH, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
      keepalive: true,
      // No cookies: the collector never needs to know who the visitor is.
      credentials: 'omit',
    }).catch(() => undefined);
  } catch {
    // Offline or fetch unavailable: analytics are best-effort and never queued.
  }
}

let hasTrackedFirstPageview = false;

/** Sends an anonymous pageview. No-op on the server, outside production and when opted out. */
export function trackPageview(path: string): void {
  if (!isTrackingEnabled()) return;

  // External referrer is only meaningful for the landing pageview of this page load.
  const referrer = hasTrackedFirstPageview ? undefined : document.referrer;
  hasTrackedFirstPageview = true;

  const payload = buildPageviewPayload(path, referrer, window.location.origin);
  if (payload) send(payload);
}

/**
 * Sends an anonymous custom event (name must be in ANALYTICS_EVENT_NAMES). No-op on the server,
 * outside production and when opted out. Never pass PII or health/body data.
 */
export function trackEvent(name: AnalyticsEventName, data: AnalyticsEventData = {}): void {
  if (!isTrackingEnabled()) return;
  send(buildEventPayload(name, data, window.location.pathname));
}

/** Coarse duration bucket so exact training times never leave the device. */
export function durationBucket(seconds: number): string {
  const minutes = seconds / 60;
  if (minutes < 15) return '<15m';
  if (minutes < 30) return '15-30m';
  if (minutes < 60) return '30-60m';
  if (minutes < 90) return '60-90m';
  return '90m+';
}

/** Coarse count bucket (e.g. completed sets). */
export function countBucket(count: number): string {
  if (count <= 0) return '0';
  if (count <= 5) return '1-5';
  if (count <= 15) return '6-15';
  if (count <= 30) return '16-30';
  return '31+';
}
