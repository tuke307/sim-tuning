import { ExceptionFilter, Catch, ArgumentsHost, HttpException, HttpStatus } from '@nestjs/common';
import { Response } from 'express';
import { logger } from '@sim-tuning/logger';
import { NODE_ENV } from '@/config/env.config';
import type { ApiResponse } from '@sim-tuning/common/api/responses/api.response';

interface ErrorDetails {
  message?: string;
  errors?: Array<{ field: string; message: string }>;
  stack?: string;
  code?: string;
}

@Catch()
export class HttpExceptionFilter implements ExceptionFilter {
  catch(exception: unknown, host: ArgumentsHost) {
    const ctx = host.switchToHttp();
    const response = ctx.getResponse<Response>();

    let status = HttpStatus.INTERNAL_SERVER_ERROR;
    let message = 'Internal Server Error';
    let errorDetails: ErrorDetails | undefined = undefined;

    if (exception instanceof HttpException) {
      status = exception.getStatus();
      const exceptionResponse = exception.getResponse();

      if (typeof exceptionResponse === 'string') {
        message = exceptionResponse;
      } else if (typeof exceptionResponse === 'object' && exceptionResponse !== null) {
        const responseObj = exceptionResponse as {
          message?: string;
          errors?: ErrorDetails['errors'];
          code?: string;
        };
        message = responseObj.message || message;

        // Handle validation errors
        if (responseObj.errors) {
          errorDetails = { errors: responseObj.errors };
        }
        // Forward a machine-readable code (e.g. MUST_CHANGE_PASSWORD).
        if (responseObj.code) {
          errorDetails = { ...(errorDetails || {}), code: responseObj.code };
        }
      }
    } else if (exception instanceof Error) {
      // Handle known error patterns
      if (exception.name === 'ValidationError') {
        status = HttpStatus.BAD_REQUEST;
        message = 'Validation Error';
      } else if (exception.name === 'UnauthorizedError') {
        status = HttpStatus.UNAUTHORIZED;
        message = 'Invalid or Missing Authentication Token';
      } else if (exception.name === 'ForbiddenError') {
        status = HttpStatus.FORBIDDEN;
        message = 'Forbidden: You do not have access to this resource';
      } else if (prismaErrorCode(exception) === 'P2002') {
        // Prisma unique constraint violation
        status = HttpStatus.BAD_REQUEST;
        message = 'Duplicate Entry';
      } else if (prismaErrorCode(exception) === 'P2025') {
        // Prisma foreign key constraint violation
        status = HttpStatus.BAD_REQUEST;
        message = 'Foreign Key Constraint Violation';
      }

      // In development, include detailed error information
      if (NODE_ENV !== 'production') {
        errorDetails = {
          ...(errorDetails || {}),
          message: exception.message,
          stack: exception.stack,
        };
      }
    }

    // Log at the appropriate level. 4xx responses are routine client errors
    // (expired/missing auth, missing CSRF, validation failures, unknown routes
    // and bot probes such as /api/client_secret.json). Logging them at `error`
    // drowns out real server faults, so they go to `warn` while 5xx stay at
    // `error`. Passing (summary, exception) preserves stack extraction at any
    // level; the request method+url adds the context these logs were missing.
    const request = ctx.getRequest<{ method?: string; url?: string }>();
    const requestLine = request?.method && request?.url ? ` ${request.method} ${request.url}` : '';
    const isClientError = status >= 400 && status < 500;
    const summary = `${status}${requestLine}`;

    // "No credentials present" outcomes from the auth/CSRF guards are routine
    // for any unauthenticated request (e.g. the login-page session probe) and
    // not actionable. Log at `debug` so they don't flood `warn`; at the default
    // `info` level they're suppressed entirely and recoverable via
    // LOG_LEVEL=debug. Expired/invalid tokens, permission denials,
    // MUST_CHANGE_PASSWORD and validation errors stay at `warn`.
    const ROUTINE_MISSING_CREDENTIALS = new Set<string>([
      'Access token missing', // AuthGuard: no accessToken cookie
      'CSRF token missing', // CsrfGuard: no csrfToken cookie/header
    ]);
    const isRoutineAuthProbe =
      (status === HttpStatus.UNAUTHORIZED || status === HttpStatus.FORBIDDEN) &&
      typeof message === 'string' &&
      ROUTINE_MISSING_CREDENTIALS.has(message);

    const log = isRoutineAuthProbe ? logger.debug : isClientError ? logger.warn : logger.error;

    if (exception instanceof Error) {
      log(summary, exception);
    } else {
      log(`${summary}: ${String(exception)}`);
    }

    const details = {
      statusCode: status,
      timestamp: new Date().toISOString(),
      ...(errorDetails || {}),
    };

    const productionDetails = {
      statusCode: status,
      timestamp: details.timestamp,
      ...(errorDetails?.errors ? { errors: errorDetails.errors } : {}),
      ...(errorDetails?.code ? { code: errorDetails.code } : {}),
    };

    const errorResponse: ApiResponse<null> = {
      success: false,
      data: null,
      error: message,
      details: NODE_ENV === 'production' ? productionDetails : details,
    };

    response.status(status).json(errorResponse);
  }
}

function prismaErrorCode(error: Error): string | undefined {
  const code = (error as Error & { code?: unknown }).code;
  return typeof code === 'string' ? code : undefined;
}
