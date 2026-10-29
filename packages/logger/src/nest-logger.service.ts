import { winstonLogger } from './winston-logger';

/**
 * Matches the multi-line stack traces Node produces (`"\n    at ..."`),
 * used to tell a stack string apart from a short Nest `context` string.
 */
const STACK_PATTERN = /\n\s*at /;

/**
 * Structural mirror of Nest's `LoggerService` (from `@nestjs/common`).
 *
 * The logger package deliberately stays free of a web-framework dependency.
 * Compatibility with the real interface is enforced where it matters — at the
 * `NestFactory.create({ logger })` call site in each app's `main.ts`, where
 * `@nestjs/common` is installed.
 */
export interface NestLoggerLike {
  log(message: unknown, ...optional: unknown[]): void;
  error(message: unknown, ...optional: unknown[]): void;
  warn(message: unknown, ...optional: unknown[]): void;
  debug(message: unknown, ...optional: unknown[]): void;
  verbose(message: unknown, ...optional: unknown[]): void;
  fatal(message: unknown, ...optional: unknown[]): void;
}

/**
 * Turns Nest's variadic optional params into Winston metadata.
 *
 * Nest calls its logger with a few conventions:
 *   log(message, context?)           -> context is a trailing string
 *   error(message, stack?, context?) -> stack contains "\n    at "
 *   error(message, error)            -> the error carries its own stack
 *
 * `context` and `stack` are surfaced as structured fields so they survive the
 * JSON pipeline and stay queryable in Loki.
 */
function toMetadata(optional: unknown[]): Record<string, unknown> {
  const meta: Record<string, unknown> = {};

  const errorArg = optional.find((p): p is Error => p instanceof Error);
  if (errorArg?.stack) {
    meta.stack = errorArg.stack;
  }

  const stackString = optional.find((p) => typeof p === 'string' && STACK_PATTERN.test(p));
  if (stackString && !meta.stack) {
    meta.stack = stackString;
  }

  const context = optional.find((p) => typeof p === 'string' && !STACK_PATTERN.test(p));
  if (context) {
    meta.context = context;
  }

  return meta;
}

/**
 * NestJS `LoggerService` backed by the shared Winston logger.
 *
 * Pass it to `NestFactory.create(AppModule, { logger: new NestLoggerService() })`
 * so Nest's own framework logs (NestFactory, InstanceLoader, RoutesResolver,
 * RouterExplorer, ...) flow through Winston instead of the built-in Console
 * logger. In production every line is then a JSON object, which keeps Loki's
 * `| json` parser from emitting JSONParserErr on bootstrap/routing output such
 * as `Mapped {/api/media, GET} route`.
 */
export class NestLoggerService implements NestLoggerLike {
  log(message: unknown, ...optional: unknown[]): void {
    winstonLogger.info(message as string, toMetadata(optional));
  }

  error(message: unknown, ...optional: unknown[]): void {
    winstonLogger.error(message as string, toMetadata(optional));
  }

  warn(message: unknown, ...optional: unknown[]): void {
    winstonLogger.warn(message as string, toMetadata(optional));
  }

  debug(message: unknown, ...optional: unknown[]): void {
    winstonLogger.debug(message as string, toMetadata(optional));
  }

  verbose(message: unknown, ...optional: unknown[]): void {
    winstonLogger.verbose(message as string, toMetadata(optional));
  }

  fatal(message: unknown, ...optional: unknown[]): void {
    winstonLogger.error(message as string, toMetadata(optional));
  }
}
