import { createParamDecorator, ExecutionContext } from '@nestjs/common';
import type { Request } from 'express';
import { resolveSupportedLocale } from '@sim-tuning/common/utils';
import type { SupportedLocale } from '@sim-tuning/common/enums';

/** Locale for localized catalog content, taken from the `Accept-Language` header. */
export const RequestLocale = createParamDecorator(
  (_data: unknown, ctx: ExecutionContext): SupportedLocale =>
    resolveSupportedLocale(ctx.switchToHttp().getRequest<Request>().headers['accept-language']),
);
