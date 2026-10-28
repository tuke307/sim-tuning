import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import { isSupportedLocale, resolveSupportedLocale, SOURCE_LOCALE } from './locale.util';

describe('resolveSupportedLocale', () => {
  it('picks the first supported base language', () => {
    assert.equal(resolveSupportedLocale('fr-FR,en-US;q=0.8,de;q=0.5'), 'en');
    assert.equal(resolveSupportedLocale('de_AT'), 'de');
  });

  it('falls back to the source locale', () => {
    assert.equal(resolveSupportedLocale(undefined), SOURCE_LOCALE);
    assert.equal(resolveSupportedLocale('fr, it'), SOURCE_LOCALE);
  });

  it('narrows supported locales', () => {
    assert.equal(isSupportedLocale('en'), true);
    assert.equal(isSupportedLocale('xx'), false);
  });
});
