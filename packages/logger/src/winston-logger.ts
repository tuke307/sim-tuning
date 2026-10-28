import winston from 'winston';
import { transports } from 'winston';
import { LOG_LEVEL, NODE_ENV } from './config';
import { devFormat, prodFormat } from './formats';

/**
 * Create the core Winston logger instance.
 * It switches formats depending on NODE_ENV and always logs uncaught exceptions and rejections.
 */
export const winstonLogger = winston.createLogger({
  level: LOG_LEVEL,
  format: NODE_ENV === 'production' ? prodFormat : devFormat,
  transports: [
    new transports.Console({
      handleExceptions: true,
      handleRejections: true,
    }),
    // Uncomment and adjust file transports if persistent logs are required:
    // new transports.File({ filename: 'error.log', level: 'error' }),
    // new transports.File({ filename: 'combined.log' }),
  ],
  exitOnError: false, // Prevent exiting on handled exceptions
});
