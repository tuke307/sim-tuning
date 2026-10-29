import { Injectable } from '@nestjs/common';
import { logger } from '@sim-tuning/logger';
import type { AnalyticsCollectDto } from '@sim-tuning/common/schemas';
import {
  NODE_ENV,
  UMAMI_ALLOWED_HOSTNAMES,
  UMAMI_URL,
  UMAMI_WEBSITE_ID,
} from '@/config/env.config';

export interface WebAnalyticsConfig {
  enabled: boolean;
  umamiUrl: string;
  websiteId: string;
  allowedHostnames: readonly string[];
  timeoutMs: number;
}

/** Request metadata needed to forward a pageview or event. Never logged. */
export interface PageviewRequestContext {
  hostname: string | undefined;
  clientIp: string | undefined;
  userAgent: string | undefined;
  acceptLanguage: string | undefined;
  /** `Sec-Fetch-Site` request header, when the browser sends it. */
  fetchSite: string | undefined;
}

export interface UmamiSendRequest {
  url: string;
  init: RequestInit;
}

const LANGUAGE_TAG_PATTERN = /^[a-z]{1,8}(?:-[a-z0-9]{1,8})*$/i;
const HOSTNAME_PATTERN = /^[a-z0-9.-]{1,253}$/;

export function firstLanguageTag(acceptLanguage: string | undefined): string | undefined {
  const tag = acceptLanguage?.split(',', 1)[0]?.split(';', 1)[0]?.trim();
  return tag && tag.length <= 35 && LANGUAGE_TAG_PATTERN.test(tag) ? tag : undefined;
}

export function normalizeHostname(raw: string | undefined): string | undefined {
  // X-Forwarded-Host may be a comma-separated list; the first entry is the client-facing host.
  const host = raw?.split(',', 1)[0]?.trim().toLowerCase().replace(/:\d+$/, '');
  return host && HOSTNAME_PATTERN.test(host) ? host : undefined;
}

@Injectable()
export class WebAnalyticsService {
  public config: WebAnalyticsConfig = {
    enabled: NODE_ENV === 'production' && UMAMI_WEBSITE_ID.length > 0,
    umamiUrl: UMAMI_URL,
    websiteId: UMAMI_WEBSITE_ID,
    allowedHostnames: UMAMI_ALLOWED_HOSTNAMES,
    timeoutMs: 3000,
  };

  public fetchImpl: typeof fetch = (input, init) => fetch(input, init);

  /**
   * Builds the Umami `/api/send` request, or returns null when the hit must be dropped
   * (forwarding disabled, unknown hostname, cross-site request, missing user agent).
   */
  buildUmamiRequest(
    hit: AnalyticsCollectDto,
    context: PageviewRequestContext,
  ): UmamiSendRequest | null {
    const { config } = this;
    if (!config.enabled) return null;

    // Browsers send Sec-Fetch-Site on fetch(); drop anything not originating from our own pages.
    if (context.fetchSite && context.fetchSite !== 'same-origin') return null;

    const hostname = normalizeHostname(context.hostname);
    if (!hostname || !config.allowedHostnames.includes(hostname)) return null;

    // Umami rejects requests without a User-Agent (bot detection), so skip early.
    const userAgent = context.userAgent?.trim();
    if (!userAgent) return null;

    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      'User-Agent': userAgent.slice(0, 512),
    };
    if (context.clientIp) {
      // Umami derives country + rotating session hash from this, then discards it.
      headers['x-umami-client-ip'] = context.clientIp;
    }

    const language = firstLanguageTag(context.acceptLanguage);

    return {
      url: `${config.umamiUrl}/api/send`,
      init: {
        method: 'POST',
        headers,
        body: JSON.stringify({
          type: 'event',
          payload: {
            website: config.websiteId,
            hostname,
            url: hit.url,
            ...(hit.type === 'pageview'
              ? { referrer: hit.referrer ?? '' }
              : { name: hit.name, ...(hit.data ? { data: hit.data } : {}) }),
            ...(language ? { language } : {}),
          },
        }),
        signal: AbortSignal.timeout(config.timeoutMs),
      },
    };
  }

  /** Forwards a pageview or event to Umami. Never throws; failures are logged without visitor data. */
  async forward(hit: AnalyticsCollectDto, context: PageviewRequestContext): Promise<void> {
    const request = this.buildUmamiRequest(hit, context);
    if (!request) return;

    try {
      const response = await this.fetchImpl(request.url, request.init);
      if (!response.ok) {
        logger.warn(`Umami forwarding failed with status ${response.status}`);
      }
    } catch (error) {
      const reason = error instanceof Error ? error.name : 'unknown error';
      logger.warn(`Umami forwarding failed: ${reason}`);
    }
  }
}
