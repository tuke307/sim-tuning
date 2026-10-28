import { Injectable, CanActivate, ExecutionContext, ForbiddenException } from '@nestjs/common';
import type { Request } from 'express';
import { constantTimeEqual } from '@sim-tuning/common/utils/crypto';
import type { AuthTransport } from './auth.guard';

@Injectable()
export class CsrfGuard implements CanActivate {
  canActivate(context: ExecutionContext): boolean {
    const request = context
      .switchToHttp()
      .getRequest<Request & { authTransport?: AuthTransport }>();

    // Bearer-authenticated requests (e.g., native iOS clients) do not
    // rely on ambient browser cookies, so CSRF protection is not applicable.
    if (request.authTransport === 'bearer') {
      return true;
    }

    const csrfTokenFromCookie: string = request.cookies?.csrfToken ?? '';
    const headerValue = request.headers['x-csrf-token'];
    const csrfTokenFromHeader: string = Array.isArray(headerValue)
      ? (headerValue[0] ?? '')
      : (headerValue ?? '');

    if (!csrfTokenFromCookie || !csrfTokenFromHeader) {
      throw new ForbiddenException('CSRF token missing');
    }

    // Constant-time compare: a plain !== short-circuits on the first differing
    // byte, leaking the token over a timing side channel.
    if (!constantTimeEqual(csrfTokenFromCookie, csrfTokenFromHeader)) {
      throw new ForbiddenException('Invalid CSRF token');
    }

    return true;
  }
}
