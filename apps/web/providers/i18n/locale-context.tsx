'use client';

import React, {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useSyncExternalStore,
  type ReactNode,
} from 'react';
import type { LanguagePreference, SupportedLocale } from '@sim-tuning/common/enums';
import {
  DEFAULT_LOCALE,
  detectSystemLocale,
  getDictionary,
  interpolate,
  LOCALE_REGISTRY,
  type TranslationDictionary,
} from '@/lib/i18n';
import { useAuth } from '../auth/auth-context';

const LANGUAGE_STORAGE_KEY = 'sim_tuning_language_preference';
const LOCALE_CHANGE_EVENT = 'sim-tuning-locale-preference-change';

interface LocaleContextValue {
  locale: SupportedLocale;
  bcp47: string;
  languagePreference: LanguagePreference;
  setLanguagePreference: (pref: LanguagePreference) => Promise<void>;
  t: TranslationDictionary;
  format: (template: string, params?: Record<string, string | number>) => string;
}

const LocaleContext = createContext<LocaleContextValue | null>(null);

function subscribeToLocaleChanges(onStoreChange: () => void): () => void {
  if (typeof window === 'undefined') return () => {};
  window.addEventListener('languagechange', onStoreChange);
  window.addEventListener('storage', onStoreChange);
  window.addEventListener(LOCALE_CHANGE_EVENT, onStoreChange);
  return () => {
    window.removeEventListener('languagechange', onStoreChange);
    window.removeEventListener('storage', onStoreChange);
    window.removeEventListener(LOCALE_CHANGE_EVENT, onStoreChange);
  };
}

function getClientPreferenceSnapshot(): LanguagePreference {
  if (typeof window === 'undefined') return 'system';
  const stored = window.localStorage.getItem(LANGUAGE_STORAGE_KEY);
  if (stored === 'en' || stored === 'de' || stored === 'system') {
    return stored;
  }
  return 'system';
}

function getServerPreferenceSnapshot(): LanguagePreference {
  return 'system';
}

function getClientSystemLocaleSnapshot(): SupportedLocale {
  return detectSystemLocale();
}

function getServerSystemLocaleSnapshot(): SupportedLocale {
  return DEFAULT_LOCALE;
}

export function LocaleProvider({ children }: { children: ReactNode }) {
  const { user, updatePreferences } = useAuth();

  const storedPref = useSyncExternalStore(
    subscribeToLocaleChanges,
    getClientPreferenceSnapshot,
    getServerPreferenceSnapshot,
  );

  const systemLocale = useSyncExternalStore(
    subscribeToLocaleChanges,
    getClientSystemLocaleSnapshot,
    getServerSystemLocaleSnapshot,
  );

  const userLangPref = user?.preferences?.language;
  const languagePreference: LanguagePreference =
    userLangPref === 'en' || userLangPref === 'de' || userLangPref === 'system'
      ? userLangPref
      : storedPref;

  useEffect(() => {
    if (userLangPref === 'en' || userLangPref === 'de' || userLangPref === 'system') {
      window.localStorage.setItem(LANGUAGE_STORAGE_KEY, userLangPref);
    }
  }, [userLangPref]);

  const locale: SupportedLocale =
    languagePreference === 'system' ? systemLocale : languagePreference;

  useEffect(() => {
    if (typeof document !== 'undefined') {
      document.documentElement.lang = locale;
    }
  }, [locale]);

  const setLanguagePreference = useCallback(
    async (pref: LanguagePreference) => {
      if (typeof window !== 'undefined') {
        window.localStorage.setItem(LANGUAGE_STORAGE_KEY, pref);
        window.dispatchEvent(new Event(LOCALE_CHANGE_EVENT));
      }
      await updatePreferences({ language: pref });
    },
    [updatePreferences],
  );

  const value = useMemo<LocaleContextValue>(() => {
    const t = getDictionary(locale);
    const bcp47 = LOCALE_REGISTRY[locale].bcp47;
    return {
      locale,
      bcp47,
      languagePreference,
      setLanguagePreference,
      t,
      format: interpolate,
    };
  }, [locale, languagePreference, setLanguagePreference]);

  return <LocaleContext.Provider value={value}>{children}</LocaleContext.Provider>;
}

export function useLocale(): LocaleContextValue {
  const ctx = useContext(LocaleContext);
  if (!ctx) {
    throw new Error('useLocale must be used within a LocaleProvider');
  }
  return ctx;
}
