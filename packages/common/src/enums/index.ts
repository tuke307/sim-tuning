export const SUPPORTED_LOCALES = ['en', 'de'] as const;
export type SupportedLocale = (typeof SUPPORTED_LOCALES)[number];
export const LANGUAGE_PREFERENCES = ['system', 'en', 'de'] as const;
export type LanguagePreference = (typeof LANGUAGE_PREFERENCES)[number];

export const THEME_PREFERENCES = ['system', 'light', 'dark'] as const;
export type ThemePreference = (typeof THEME_PREFERENCES)[number];

export enum EmailTemplateType {
  ACCOUNT_CONFIRMATION = 'ACCOUNT_CONFIRMATION',
  PASSWORD_RESET = 'PASSWORD_RESET',
  ACCOUNT_DELETION = 'ACCOUNT_DELETION',
}

export enum EmailStatus {
  PROCESSING = 'PROCESSING',
  PENDING = 'PENDING',
  SENT = 'SENT',
  FAILED = 'FAILED',
}
