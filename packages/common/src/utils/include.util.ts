import { z } from 'zod';

export const createIncludeSchema = (validIncludes: string[]) => {
  return z.object({
    include: z
      .string()
      .optional()
      .refine(
        (value) => {
          if (!value) return true;

          const includes = value.split(',').map((inc) => inc.trim());
          const invalid = includes.filter((inc) => !validIncludes.includes(inc));
          return invalid.length === 0;
        },
        {
          message: `"include" contains invalid values. Valid options are: ${validIncludes.join(', ')}`,
        },
      )
      .optional(),
  });
};
