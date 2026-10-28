import { winstonLogger } from './winston-logger';

/**
 * Define allowed log levels to match Winston's built‐in levels.
 */
type LogLevel = 'error' | 'warn' | 'info' | 'http' | 'verbose' | 'debug' | 'silly';

/**
 * Define primitive types and complex types that may be passed to the logger.
 */
type LogPrimitive = string | number | boolean;
type LogValue = LogPrimitive | Error | Record<string, unknown> | unknown[] | unknown;
type LogArgument = LogValue | LogValue[];

/**
 * Logger wrapper around Winston that provides handling for objects and variables.
 *
 * Examples:
 *   logger.info("A message");                              // Simple message
 *   logger.info("User data", user);                        // Message with an object (formatted as JSON)
 *   logger.info("Value:", 123);                            // Message with primitive value (appended to message)
 *   logger.info("Multiple values:", "a", "b", "c");        // Multiple primitive values (all appended to message)
 *   logger.info("Mixed values:", "text", { key: "value" }); // Mixed types (formatted as data object)
 *   logger.error(err);                                     // Log an error directly
 *   logger.error("Operation failed", err);                 // Log an error with a message
 */
export const logger = (Object.keys(winstonLogger.levels) as LogLevel[]).reduce(
  (acc, level) => {
    acc[level] = (messageOrError: string | Error, ...args: LogArgument[]) => {
      // If logging an Error directly at 'error' level:
      if (level === 'error' && messageOrError instanceof Error) {
        const { message, stack, name, ...errorProps } = messageOrError;
        const errorDetails = {
          error: {
            message,
            name,
            stack,
            ...errorProps,
          },
        };
        winstonLogger.error(message, errorDetails);
        return;
      }

      const message = messageOrError as string;

      if (args.length === 0) {
        // Simple message
        winstonLogger[level](message);
      } else if (args.length === 1) {
        const first = args[0];

        // Single Error argument
        if (first instanceof Error) {
          const { message: errMsg, stack, name, ...errorProps } = first;
          winstonLogger[level](message, {
            error: {
              message: errMsg,
              name,
              stack,
              ...errorProps,
            },
          });
        }
        // Single primitive argument
        else if (
          typeof first === 'string' ||
          typeof first === 'number' ||
          typeof first === 'boolean'
        ) {
          winstonLogger[level](`${message} ${first}`);
        }
        // Single object/array argument
        else {
          winstonLogger[level](message, { data: first });
        }
      } else {
        // Multiple arguments: check if all primitives
        const allSimple = args.every(
          (arg): arg is LogPrimitive =>
            typeof arg === 'string' || typeof arg === 'number' || typeof arg === 'boolean',
        );

        if (allSimple) {
          winstonLogger[level](`${message} ${args.join(' ')}`);
        } else {
          winstonLogger[level](message, { data: args });
        }
      }
    };
    return acc;
  },
  {} as Record<LogLevel, (messageOrError: string | Error, ...args: LogArgument[]) => void>,
);

/**
 * A writable stream interface that can be used, for example, with Morgan or other middleware.
 * Every written line is trimmed and sent to the logger at 'info' level.
 */
export const stream = {
  write: (message: string) => {
    winstonLogger.info(message.trim());
  },
};

/**
 * Re‐exporting the underlying Winston instance in case raw access is needed.
 */
export { winstonLogger };

export { NestLoggerService } from './nest-logger.service';
export default logger;
