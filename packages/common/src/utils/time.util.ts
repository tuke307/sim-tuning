import { format, addDays, isBefore, differenceInMinutes } from 'date-fns';
import { de } from 'date-fns/locale';
import { toZonedTime } from 'date-fns-tz';

// German timezone constant
const GERMAN_TIMEZONE = 'Europe/Berlin';

// German month names for formatting
const GERMAN_MONTH_NAMES = [
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

export const getStartOfMonthUTC = (year: number, month: number): Date => {
  // Use UTC components directly: date-fns startOfMonth reads the host-local
  // month of the instant, which shifts the boundary under a non-UTC TZ.
  return new Date(Date.UTC(year, month - 1, 1, 0, 0, 0, 0));
};

export const getEndOfMonthUTC = (year: number, month: number): Date => {
  // Last millisecond of the month in UTC (day 0 of next month = last day).
  return new Date(Date.UTC(year, month, 0, 23, 59, 59, 999));
};

export const getDateRangeUTC = (startDate: Date, endDate: Date): Date[] => {
  const dateRange: Date[] = [];
  let currentDay = startDate;
  while (!isBefore(endDate, currentDay)) {
    dateRange.push(currentDay);
    currentDay = addDays(currentDay, 1);
  }
  return dateRange;
};

// English weekday names indexed by Date.getUTCDay() (0 = Sunday). Uses UTC so
// the weekday matches the UTC-stored wall-clock day regardless of host TZ.
const UTC_DAY_OF_WEEK = [
  'SUNDAY',
  'MONDAY',
  'TUESDAY',
  'WEDNESDAY',
  'THURSDAY',
  'FRIDAY',
  'SATURDAY',
] as const;

export const getDayOfWeekUTC = (date: Date): string => {
  // getUTCDay() is always 0–6; narrowing to that union keeps the tuple lookup
  // total (no undefined) under noUncheckedIndexedAccess.
  return UTC_DAY_OF_WEEK[date.getUTCDay() as 0 | 1 | 2 | 3 | 4 | 5 | 6];
};

export const formatUTCDate = (date: Date): string => {
  return format(date, "yyyy-MM-dd'T'HH:mm:ss.SSSX");
};

export const isBeforeToday = (date: Date): boolean => {
  return isBefore(date, new Date());
};

export const getDifferenceInMinutes = (date1: Date, date2: Date): number => {
  return differenceInMinutes(date2, date1);
};

/**
 * Formats a UTC date to a German-readable string with timezone conversion (e.g., "10. Januar 2025")
 * Converts UTC date to German timezone before formatting
 * @param date UTC Date object from database
 * @param dateFormat Format string for date-fns
 * @returns Formatted date string in German timezone
 */
export const formatGermanDateWithTZ = (
  date: Date,
  dateFormat: string = 'dd. MMMM yyyy',
): string => {
  const germanDate = toZonedTime(date, GERMAN_TIMEZONE);
  return format(germanDate, dateFormat, { locale: de });
};

/**
 * Formats time to German-readable format with timezone conversion (e.g., "14:30")
 * Converts UTC date to German timezone before formatting
 * @param date UTC Date object from database
 * @param timeFormat Format string for date-fns
 * @returns Formatted time string in German timezone
 */
export const formatGermanTimeWithTZ = (date: Date, timeFormat: string = 'HH:mm'): string => {
  const germanDate = toZonedTime(date, GERMAN_TIMEZONE);
  return format(germanDate, timeFormat, { locale: de });
};

/**
 * Get the number of days in a specific month
 * @param month Month number (1-12)
 * @param year Year (defaults to current year)
 * @returns Number of days in the month
 */
export const getDaysInMonth = (
  month: number,
  year: number = new Date().getUTCFullYear(),
): number => {
  // Day 0 of next month = last day of this month, computed in UTC so it is
  // independent of the host/browser timezone.
  return new Date(Date.UTC(year, month, 0)).getUTCDate();
};

export const convertTimeStringToDate = (time?: string): Date | null => {
  return time ? new Date(`1970-01-01T${time}Z`) : null;
};

/**
 * Get the German timezone constant
 * @returns The timezone string for Germany (Europe/Berlin)
 */
export const getGermanTimezone = (): string => {
  return GERMAN_TIMEZONE;
};

/**
 * Get German month names array
 * @returns Array of German month names
 */
export const getGermanMonthNames = (): string[] => {
  return [...GERMAN_MONTH_NAMES];
};

/**
 * Coerce unknown form/API values into a Date, or undefined if invalid/empty.
 * Accepts Date, ISO string, timestamp number; returns undefined for null/empty/garbage.
 */
export const normalizeDateValue = (value: unknown): Date | undefined => {
  if (!value) {
    return undefined;
  }

  if (value instanceof Date) {
    return value;
  }

  if (typeof value === 'string' || typeof value === 'number') {
    const parsed = new Date(value);
    return Number.isNaN(parsed.getTime()) ? undefined : parsed;
  }

  return undefined;
};

/**
 * Encode the picked calendar day as UTC midnight. Needed for date-only columns
 * (Prisma `@db.Date`): the calendar returns local midnight, which serialized to
 * JSON becomes the previous UTC day for any timezone east of UTC (e.g. CET/CEST).
 * Postgres then truncates that UTC timestamp to its date, storing the wrong day.
 * Encoding the picked calendar day as UTC midnight keeps the stored date
 * identical to what the user clicked.
 */
export const toUtcMidnight = (date: Date): Date =>
  new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()));
