import { PipeTransform, Injectable, BadRequestException } from '@nestjs/common';
import type { ZodSchema } from 'zod';
import { ZodError } from 'zod';

@Injectable()
export class ZodValidationPipe implements PipeTransform {
  constructor(private schema?: ZodSchema) {}

  transform(value: unknown) {
    // If no schema is provided, return the value as-is
    if (!this.schema) {
      return value;
    }

    try {
      const validatedValue = this.schema.parse(value);
      return validatedValue;
    } catch (error) {
      if (error instanceof ZodError) {
        const errorDetails = error.issues.map((issue) => {
          const path = issue.path.join('.') || 'unknown';
          return { field: path, message: issue.message };
        });
        throw new BadRequestException({
          message: 'Validation failed',
          errors: errorDetails,
        });
      }
      throw new BadRequestException('Validation failed');
    }
  }
}
