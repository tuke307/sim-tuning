import { z } from 'zod';

export const searchSchema = z.object({
  searchWords: z
    .union([
      z.array(
        z
          .string()
          .min(1)
          .transform((val) => val.trim()),
      ),
      z
        .string()
        .min(1)
        .transform((val) => val.trim()),
    ])
    .optional(),
});
