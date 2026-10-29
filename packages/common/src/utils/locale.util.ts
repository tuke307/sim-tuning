import { SUPPORTED_LOCALES, type SupportedLocale } from '../enums';

export const SOURCE_LOCALE: SupportedLocale = 'en';

export function isSupportedLocale(value: string): value is SupportedLocale {
  return (SUPPORTED_LOCALES as readonly string[]).includes(value);
}

/** Picks the first supported language from an Accept-Language header, else the source locale. */
export function resolveSupportedLocale(acceptLanguage?: string | null): SupportedLocale {
  for (const part of acceptLanguage?.split(',') ?? []) {
    const base = part.split(';')[0]?.trim().toLowerCase().split(/[-_]/)[0];
    if (base && isSupportedLocale(base)) return base;
  }
  return SOURCE_LOCALE;
}
