export const LOCALES = ['en', 'es', 'fr', 'ar', 'ur', 'bn', 'id', 'tr', 'fa'] as const;
export type Locale = typeof LOCALES[number];
export const DEFAULT_LOCALE: Locale = 'en';

export const LOCALE_NAMES: Record<Locale, string> = {
  en: 'English',
  es: 'Español',
  fr: 'Français',
  ar: 'العربية',
  ur: 'اردو',
  bn: 'বাংলা',
  id: 'Indonesia',
  tr: 'Türkçe',
  fa: 'فارسی',
};

export const RTL_LOCALES = new Set<Locale>(['ar', 'ur', 'fa']);

export function isRTL(lang: string): boolean {
  return RTL_LOCALES.has(lang as Locale);
}

export function isValidLocale(lang: string): lang is Locale {
  return (LOCALES as readonly string[]).includes(lang);
}
