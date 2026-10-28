// Next.js instrumentation hook — runs once on server startup (Node.js runtime)
// before any request is handled.
//
// Routes every server-side console.* call — including Next.js framework
// errors (⨯ InvariantError, "Failed to find Server Action", route logs, which
// Next emits via console) — through the shared `@sim-tuning/logger` Winston logger.
// That is the same JSON formatter the api/email-worker services use, so web
// log lines are byte-identical and Loki's `| json` parser consumes them
// without JSONParserErr.
//
// Recursion-safe: Winston's Console transport writes to the raw
// `console._stderr`/`console._stdout` streams (not the console methods), so
// delegating console.error -> logger.error -> transport does not loop.
//
// See: https://nextjs.org/docs/app/building-your-application/optimizing/instrumentation
import { logger } from '@sim-tuning/logger';
import { SERVER_API_URL, NODE_ENV } from '@/lib/config/env.config';

const isNodeRuntime = process.env.NEXT_RUNTIME === 'nodejs';
let installed = false;

// The high-level logger API expects a string|Error as the first argument; a
// bare console.log(undefined) / console.log(123) would otherwise produce a
// meaningless line, so coerce non-string/Error leads into text.
function asMessage(arg: unknown): string | Error {
  if (typeof arg === 'string' || arg instanceof Error) return arg;
  if (arg === undefined) return 'undefined';
  if (arg === null) return 'null';
  return String(arg);
}

export async function register(): Promise<void> {
  if (!isNodeRuntime || installed) return;
  installed = true;

  const redirect =
    (level: 'info' | 'debug' | 'warn' | 'error') =>
    (...args: unknown[]): void => {
      logger[level](asMessage(args[0]), ...args.slice(1));
    };

  console.log = redirect('info') as typeof console.log;
  console.info = redirect('info') as typeof console.info;
  console.debug = redirect('debug') as typeof console.debug;
  console.warn = redirect('warn') as typeof console.warn;
  console.error = redirect('error') as typeof console.error;

  // Log the resolved SSR API base at boot, then probe it so startup logs show
  // whether server-side fetch can reach the api. The api (Nest) bootstraps
  // slower than this Next.js server, so retry for a few seconds instead of
  // failing the probe on the first attempt while the api's port is still
  // unbound.
  logger.info('Server-side fetch initialized', { SERVER_API_URL, NODE_ENV });
  void probeApi(`${SERVER_API_URL}/health`);
}

// Retries the api health endpoint until it responds or attempts run out. The
// api normally binds within ~2s; this absorbs that startup race so we don't
// log a spurious "unreachable" warning on every boot.
async function probeApi(url: string, attempts = 12, delayMs = 500): Promise<void> {
  for (let i = 0; i < attempts; i++) {
    try {
      const res = await fetch(url, { signal: AbortSignal.timeout(1500) });
      logger.info('Server-side fetch reachable', { status: res.status });
      return;
    } catch (err) {
      if (i === attempts - 1) {
        const cause = (err as { cause?: unknown }).cause;
        logger.warn('Server-side fetch unreachable', {
          error: err instanceof Error ? err.message : String(err),
          cause: cause instanceof Error ? cause.message : (cause ?? undefined),
        });
      }
      await new Promise((resolve) => setTimeout(resolve, delayMs));
    }
  }
}
