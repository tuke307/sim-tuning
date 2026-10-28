import type { LanguagePreference, SupportedLocale } from '@sim-tuning/common/enums';
import { en } from './locales/en';
import { de } from './locales/de';
import type { LocaleDefinition, TranslationDictionary } from './types';

export const LOCALE_REGISTRY: Record<SupportedLocale, LocaleDefinition> = {
  en: {
    code: 'en',
    label: 'English',
    nativeLabel: 'English',
    bcp47: 'en-US',
    dictionary: en,
  },
  de: {
    code: 'de',
    label: 'German',
    nativeLabel: 'Deutsch',
    bcp47: 'de-DE',
    dictionary: de,
  },
};

export const DEFAULT_LOCALE: SupportedLocale = 'en';

export function matchBrowserTagToLocale(tag: string | undefined | null): SupportedLocale | null {
  if (!tag) return null;
  const base = tag.trim().toLowerCase().split(/[-_]/)[0];
  if (base && base in LOCALE_REGISTRY) {
    return base as SupportedLocale;
  }
  return null;
}

export function detectSystemLocale(): SupportedLocale {
  if (typeof window === 'undefined' || typeof navigator === 'undefined') {
    return DEFAULT_LOCALE;
  }
  const candidates = [
    ...(Array.isArray(navigator.languages) ? navigator.languages : []),
    navigator.language,
  ];
  for (const candidate of candidates) {
    const matched = matchBrowserTagToLocale(candidate);
    if (matched) return matched;
  }
  return DEFAULT_LOCALE;
}

export function resolveLocale(preference: LanguagePreference | undefined | null): SupportedLocale {
  if (preference && preference !== 'system' && preference in LOCALE_REGISTRY) {
    return preference;
  }
  return detectSystemLocale();
}

export function interpolate(template: string, params?: Record<string, string | number>): string {
  if (!params) return template;
  return template.replace(/\{(\w+)\}/g, (_, key: string) =>
    params[key] !== undefined ? String(params[key]) : `{${key}}`,
  );
}

export function getDictionary(locale: SupportedLocale): TranslationDictionary {
  return LOCALE_REGISTRY[locale]?.dictionary ?? en;
}

export * from './types';
export { en, de };
