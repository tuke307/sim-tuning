import { timingSafeEqual } from 'node:crypto';

/**
 * Constant-time comparison of two strings.
 *
 * Use this for secrets and capability tokens (CSRF tokens, cancellation /
 * process tokens, shared secrets) instead of `===` / `!==`, which
 * short-circuit on the first differing byte and can leak the value over a
 * network timing side channel. `timingSafeEqual` requires equal-length
 * buffers, so unequal lengths fall through to a non-secret-dependent `false`.
 *
 * Server-only: depends on `node:crypto`, so this is exported via the
 * `@sim-tuning/common/utils/crypto` subpath (NOT the `./utils` barrel) to keep it
 * out of browser bundles.
 */
export function constantTimeEqual(a: string, b: string): boolean {
  const aBuf = Buffer.from(a, 'utf8');
  const bBuf = Buffer.from(b, 'utf8');
  if (aBuf.length !== bBuf.length) {
    return false;
  }
  return timingSafeEqual(aBuf, bBuf);
}
