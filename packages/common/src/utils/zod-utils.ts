import { z } from 'zod';

export const IsoOrUndefined = z
  .preprocess(
    (val) => (val === '' || val == null ? undefined : val),
    z.string().datetime('Ungültige Uhrzeit'),
  )
  .optional(); // Output: string | undefined, Input: string | undefined | null | ''
