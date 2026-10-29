import { format } from 'winston';

/**
 * Destructure necessary Winston format functions.
 */
const { combine, timestamp, printf, colorize, errors, json } = format;

/**
 * Timestamp format string (German locale): DD.MM.YYYY HH:mm:ss
 */
const timestampFormat = 'DD.MM.YYYY HH:mm:ss';

/**
 * Development format: colored output, timestamp, stack traces, and metadata.
 */
export const devFormat = combine(
  colorize({ all: true }),
  timestamp({ format: timestampFormat }),
  errors({ stack: true }),
  printf(({ timestamp, level, message, stack, ...metadata }) => {
    let logLine = `${timestamp} [${level}]: ${message}`;
    if (stack) {
      logLine += `\nStack: ${stack}`;
    }
    if (Object.keys(metadata).length) {
      logLine += ` ${JSON.stringify(metadata, null, 2)}`;
    }
    return logLine;
  }),
);

/**
 * Production format: JSON output with timestamp and stack traces (no colors).
 */
export const prodFormat = combine(
  timestamp({ format: timestampFormat }),
  errors({ stack: true }),
  json(),
);
