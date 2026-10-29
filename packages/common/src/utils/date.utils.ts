import { format } from 'date-fns';
import { de } from 'date-fns/locale';

function toDate(dateInput: Date | string | number): Date {
  return dateInput instanceof Date ? dateInput : new Date(dateInput);
}

/**
 * Formats a date to `dd.MM.yyyy` (e.g., `17.10.2026`)
 */
export function formatDate(dateInput: Date | string | number): string {
  const d = toDate(dateInput);
  if (isNaN(d.getTime())) return '';
  const day = String(d.getDate()).padStart(2, '0');
  const month = String(d.getMonth() + 1).padStart(2, '0');
  const year = d.getFullYear();
  return `${day}.${month}.${year}`;
}

/**
 * Formats a time to 24h `HH:mm` without seconds (e.g., `16:10`)
 */
export function formatTime(dateInput: Date | string | number): string {
  const d = toDate(dateInput);
  if (isNaN(d.getTime())) return '';
  const hours = String(d.getHours()).padStart(2, '0');
  const minutes = String(d.getMinutes()).padStart(2, '0');
  return `${hours}:${minutes}`;
}

/**
 * Formats a date and time to `dd.MM.yyyy HH:mm` without seconds (e.g., `17.10.2026 16:10`)
 */
export function formatDateTime(dateInput: Date | string | number): string {
  const d = toDate(dateInput);
  if (isNaN(d.getTime())) return '';
  return `${formatDate(d)} ${formatTime(d)}`;
}

/**
 * Formats a date with full month name (e.g., `17. October 2026` or `17. Oktober 2026`)
 */
export function formatLongDate(
  dateInput: Date | string | number,
  locale: 'en' | 'de' = 'en',
): string {
  const d = toDate(dateInput);
  if (isNaN(d.getTime())) return '';
  const day = d.getDate();
  const monthName = d.toLocaleDateString(locale === 'de' ? 'de-DE' : 'en-US', { month: 'long' });
  const year = d.getFullYear();
  return `${day}. ${monthName} ${year}`;
}

/**
 * Formats a date with full month name and 24h time (e.g., `17. October 2026, 16:10`)
 */
export function formatLongDateTime(
  dateInput: Date | string | number,
  locale: 'en' | 'de' = 'en',
): string {
  const d = toDate(dateInput);
  if (isNaN(d.getTime())) return '';
  return `${formatLongDate(d, locale)}, ${formatTime(d)}`;
}

/**
 * Formats a compact day.month label for chart axes (e.g., `17.10.`)
 */
export function formatShortDayMonth(dateInput: Date | string | number): string {
  const d = toDate(dateInput);
  if (isNaN(d.getTime())) return '';
  const day = String(d.getDate()).padStart(2, '0');
  const month = String(d.getMonth() + 1).padStart(2, '0');
  return `${day}.${month}.`;
}

/**
 * Converts a month number (1-12) to its German name
 * @param monthNumber The month number (1 = January, 12 = December)
 * @returns The German month name
 */
export function getGermanMonth(monthNumber: number): string {
  const months = [
    'Januar',
    'Februar',
    'März',
    'April',
    'Mai',
    'Juni',
    'Juli',
    'August',
    'September',
    'Oktober',
    'November',
    'Dezember',
  ];

  // Validate month is in range 1-12
  if (monthNumber < 1 || monthNumber > 12) {
    return 'Ungültiger Monat';
  }

  return months[monthNumber - 1] as string; // Adjust for zero-based array
}

/**
 * Formats a day and month in German style (e.g., "1. Mai")
 * Alias for formatGermanDateFromNumbers for backward compatibility
 * @param day The day of the month
 * @param month The month number (1-12)
 * @returns Formatted date string in German style with month name
 */
export function formatGermanDate(day: number | undefined, month: number | undefined): string {
  if (day === undefined || month === undefined) {
    return 'Unbekannt';
  }

  return `${day}. ${getGermanMonth(month)}`;
}

/**
 * Formats a date to ISO format for input fields (yyyy-MM-dd)
 * @param date The date to format
 * @returns The date in ISO format (yyyy-MM-dd)
 */
export function formatISODate(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

/**
 * Formats a date for a calendar's display
 * @param date The date to format
 * @returns The date in German format (dd.MM.yyyy)
 */
export function formatCalendarDate(date: Date): string {
  return formatDate(date);
}

/**
 * Formats a date to German long format with day name (e.g., "Montag, 1. Januar 2024")
 * @param date The date to format
 * @returns Formatted date string with German day and month names
 */
export function formatGermanLongDate(date: Date): string {
  return format(date, 'EEEE, d. MMMM yyyy', { locale: de });
}

/**
 * Extracts time from ISO string in HH:mm format
 * @param isoString The ISO datetime string to extract time from
 * @returns Time string in HH:mm format, or empty string if invalid
 */
export function extractTime(isoString?: string | null | undefined): string {
  if (!isoString) return '';
  try {
    const date = new Date(isoString);
    return format(date, 'HH:mm');
  } catch {
    return '';
  }
}

/**
 * Returns local midnight of the Monday that starts the calendar week containing `dateInput`
 * (ISO 8601 weeks run Monday–Sunday).
 */
export function startOfIsoWeek(dateInput: Date | string | number): Date {
  const d = toDate(dateInput);
  const monday = new Date(d.getFullYear(), d.getMonth(), d.getDate());
  monday.setDate(monday.getDate() - ((monday.getDay() + 6) % 7));
  return monday;
}

/** Returns the seven local-midnight dates (Monday → Sunday) of the week containing `dateInput`. */
export function isoWeekDays(dateInput: Date | string | number): Date[] {
  const monday = startOfIsoWeek(dateInput);
  return Array.from(
    { length: 7 },
    (_, i) => new Date(monday.getFullYear(), monday.getMonth(), monday.getDate() + i),
  );
}
