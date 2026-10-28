import { Injectable, NestInterceptor, ExecutionContext, CallHandler } from '@nestjs/common';
import { Observable } from 'rxjs';
import { Request } from 'express';
import { logger } from '@sim-tuning/logger';

@Injectable()
export class LoggingInterceptor implements NestInterceptor {
  intercept(context: ExecutionContext, next: CallHandler): Observable<unknown> {
    const request = context.switchToHttp().getRequest<Request>();

    // Method and path only: headers carry cookies/bearer tokens and bodies carry passwords,
    // so neither may be logged — not even in development.
    logger.info(`${request.method} ${request.path}`);

    return next.handle();
  }
}
