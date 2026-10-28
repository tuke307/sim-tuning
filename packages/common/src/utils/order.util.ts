import { z } from 'zod';

export const orderSchema = z.object({
  orderBy: z.string().optional(),
  orderDirection: z.enum(['ASC', 'DESC']).optional(),
});

/**
 * Return a sort field that is safe to use as a Prisma `orderBy` key.
 *
 * `orderSchema.orderBy` accepts any string, so a client-supplied (or garbage)
 * value must never be spread into the Prisma query object directly — that
 * throws `PrismaClientValidationError` (HTTP 500, and a stack trace in dev).
 * Only a field present in `allowed` is honored; anything else falls back to
 * `fallback` (default `id`).
 */
export function pickSortField(
  field: string | undefined,
  allowed: readonly string[],
  fallback = 'id',
): string {
  return field && allowed.includes(field) ? field : fallback;
}
