import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import {
  buildEventPayload,
  buildPageviewPayload,
  containsTokenLikeSegment,
  sanitizeReferrer,
} from './track';

const ORIGIN = 'https://app.example.test';

describe('analytics track helpers', () => {
  it('strips query strings so auth tokens never reach the collector', () => {
    assert.deepEqual(buildPageviewPayload('/reset-password?token=FAKE-TOKEN', undefined, ORIGIN), {
      type: 'pageview',
      url: '/reset-password',
    });
  });

  it('skips paths with token-like segments or invalid shapes', () => {
    assert.equal(containsTokenLikeSegment('/verify-email/abcdefghijklmnopqrstuvwxyz0123'), true);
    assert.equal(containsTokenLikeSegment('/change-password'), false);
    assert.equal(
      buildPageviewPayload('/x/abcdefghijklmnopqrstuvwxyz0123', undefined, ORIGIN),
      null,
    );
    assert.equal(buildPageviewPayload('//evil.example', undefined, ORIGIN), null);
  });

  it('keeps only cross-origin referrers without query', () => {
    assert.equal(
      sanitizeReferrer('https://search.example/q?s=secret', ORIGIN),
      'https://search.example/q',
    );
    assert.equal(sanitizeReferrer(`${ORIGIN}/login`, ORIGIN), undefined);
    assert.equal(sanitizeReferrer('not a url', ORIGIN), undefined);
    assert.equal(sanitizeReferrer('javascript:alert(1)', ORIGIN), undefined);
  });

  it('builds event payloads without undefined values and with a safe path', () => {
    assert.deepEqual(
      buildEventPayload('login', { status: 'success', method: undefined }, '/login'),
      {
        type: 'event',
        name: 'login',
        url: '/login',
        data: { status: 'success' },
      },
    );
    assert.deepEqual(buildEventPayload('pwa_installed', {}, '/x/abcdefghijklmnopqrstuvwxyz0123'), {
      type: 'event',
      name: 'pwa_installed',
      url: '/',
    });
  });
});
