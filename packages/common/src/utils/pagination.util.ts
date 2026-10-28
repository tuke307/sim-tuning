import { z } from 'zod';

export const paginationSchema = z.object({
  limit: z.coerce.number().int().min(1).default(10).optional(),
  page: z.coerce.number().int().min(1).default(1).optional(),
});
