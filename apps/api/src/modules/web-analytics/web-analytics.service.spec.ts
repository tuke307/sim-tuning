import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import { AnalyticsCollectSchema } from '@sim-tuning/common/schemas';
import {
  WebAnalyticsService,
  firstLanguageTag,
  normalizeHostname,
  type PageviewRequestContext,
} from './web-analytics.service';

function createService(enabled = true): WebAnalyticsService {
  const service = new WebAnalyticsService();
  service.config = {
    enabled,
    umamiUrl: 'https://umami.example.test',
    websiteId: 'FAKE-WEBSITE-ID-FOR-TESTING',
    allowedHostnames: ['app.example.test'],
    timeoutMs: 3000,
  };
  return service;
}

const context: PageviewRequestContext = {
  hostname: 'app.example.test',
  clientIp: '203.0.113.7',
  userAgent: 'Mozilla/5.0 (FakeAgent)',
  acceptLanguage: 'de-DE,de;q=0.9,en;q=0.8',
  fetchSite: 'same-origin',
};

describe('AnalyticsCollectSchema', () => {
  it('strips query strings and fragments from url and referrer', () => {
    const parsed = AnalyticsCollectSchema.parse({
      type: 'pageview',
      url: '/reset-password?token=FAKE-TOKEN#x',
      referrer: 'https://other.example/page?q=1',
    });
    assert.equal(parsed.type, 'pageview');
    assert.equal(parsed.url, '/reset-password');
    assert.equal(
      parsed.type === 'pageview' ? parsed.referrer : undefined,
      'https://other.example/page',
    );
  });

  it('accepts allow-listed events with low-cardinality data', () => {
    const parsed = AnalyticsCollectSchema.parse({
      type: 'event',
      name: 'data_export',
      url: '/?tab=history',
      data: { type: 'strength', duration_bucket: '30-60m' },
    });
    assert.equal(parsed.url, '/');
    assert.equal(parsed.type === 'event' ? parsed.name : undefined, 'data_export');
  });

  it('rejects non-path urls, protocol-relative urls, unknown keys and oversized input', () => {
    const invalid = [
      { type: 'pageview', url: 'https://evil.example/' },
      { type: 'pageview', url: '//evil.example/' },
      { type: 'event', url: '/' },
      { type: 'event', name: 'not_allowed', url: '/' },
      { type: 'event', name: 'login', url: '/', data: { 'Bad-Key': 'x' } },
      { type: 'event', name: 'login', url: '/', data: { status: 'x'.repeat(101) } },
      { type: 'event', name: 'login', url: '/', referrer: 'https://x.example/' },
      { type: 'pageview', url: '/', name: 'login' },
      { type: 'pageview', url: '/', screen: '1920x1080' },
      { type: 'pageview', url: `/${'a'.repeat(500)}` },
    ];
    for (const body of invalid) {
      assert.equal(AnalyticsCollectSchema.safeParse(body).success, false, JSON.stringify(body));
    }
  });
});

describe('WebAnalyticsService', () => {
  it('builds an Umami send request with visitor UA, client IP header and first language tag', () => {
    const request = createService().buildUmamiRequest(
      { type: 'pageview', url: '/', referrer: 'https://search.example/' },
      context,
    );
    assert.ok(request);
    assert.equal(request.url, 'https://umami.example.test/api/send');
    const headers = request.init.headers as Record<string, string>;
    assert.equal(headers['User-Agent'], 'Mozilla/5.0 (FakeAgent)');
    assert.equal(headers['x-umami-client-ip'], '203.0.113.7');
    assert.deepEqual(JSON.parse(String(request.init.body)), {
      type: 'event',
      payload: {
        website: 'FAKE-WEBSITE-ID-FOR-TESTING',
        hostname: 'app.example.test',
        url: '/',
        referrer: 'https://search.example/',
        language: 'de-DE',
      },
    });
  });

  it('forwards events with name and data but without referrer', () => {
    const request = createService().buildUmamiRequest(
      { type: 'event', name: 'login', url: '/login', data: { status: 'success' } },
      context,
    );
    assert.ok(request);
    assert.deepEqual(JSON.parse(String(request.init.body)).payload, {
      website: 'FAKE-WEBSITE-ID-FOR-TESTING',
      hostname: 'app.example.test',
      url: '/login',
      name: 'login',
      data: { status: 'success' },
      language: 'de-DE',
    });
  });

  it('drops pageviews when disabled, for foreign hosts, cross-site requests or missing UA', () => {
    const pageview = { type: 'pageview' as const, url: '/' };
    assert.equal(createService(false).buildUmamiRequest(pageview, context), null);
    const service = createService();
    assert.equal(service.buildUmamiRequest(pageview, { ...context, hostname: 'evil.test' }), null);
    assert.equal(
      service.buildUmamiRequest(pageview, { ...context, fetchSite: 'cross-site' }),
      null,
    );
    assert.equal(service.buildUmamiRequest(pageview, { ...context, userAgent: undefined }), null);
  });

  it('swallows fetch errors', async () => {
    const service = createService();
    service.fetchImpl = async () => {
      throw new Error('network down');
    };
    await assert.doesNotReject(service.forward({ type: 'pageview', url: '/' }, context));
  });

  it('normalizes hostnames and language tags', () => {
    assert.equal(normalizeHostname('App.Example.Test:443, proxy.internal'), 'app.example.test');
    assert.equal(normalizeHostname('bad host/'), undefined);
    assert.equal(firstLanguageTag('en-US;q=1'), 'en-US');
    assert.equal(firstLanguageTag('<script>'), undefined);
  });
});
