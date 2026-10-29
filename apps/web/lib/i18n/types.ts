import type { LanguagePreference, SupportedLocale } from '@sim-tuning/common/enums';
import type { en } from './locales/en';

type WidenStrings<T> = T extends string
  ? string
  : T extends readonly string[]
    ? readonly string[]
    : T extends Record<string, unknown>
      ? { [K in keyof T]: WidenStrings<T[K]> }
      : T;

export type TranslationDictionary = WidenStrings<typeof en>;

export interface LocaleDefinition {
  code: SupportedLocale;
  label: string;
  nativeLabel: string;
  bcp47: string;
  dictionary: TranslationDictionary;
}

export type { LanguagePreference, SupportedLocale };
