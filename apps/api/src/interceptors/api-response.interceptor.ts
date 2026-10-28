import {
  Injectable,
  NestInterceptor,
  ExecutionContext,
  CallHandler,
  StreamableFile,
} from '@nestjs/common';
import type { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import type {
  ApiResponse,
  PageNumberPaginationMeta,
} from '@sim-tuning/common/api/responses/api.response';

type PaginationEnvelope<T> = {
  data?: T | null;
  pagination?: PageNumberPaginationMeta;
};

const isApiResponse = <T>(value: unknown): value is ApiResponse<T> => {
  if (!value || typeof value !== 'object') {
    return false;
  }

  return 'success' in value && 'data' in value && 'error' in value;
};

const isPaginationEnvelope = <T>(value: unknown): value is PaginationEnvelope<T> => {
  if (!value || typeof value !== 'object') {
    return false;
  }

  return 'data' in value && 'pagination' in value;
};

@Injectable()
export class ApiResponseInterceptor<T> implements NestInterceptor<
  T,
  ApiResponse<T> | StreamableFile
> {
  intercept(
    _context: ExecutionContext,
    next: CallHandler<T>,
  ): Observable<ApiResponse<T> | StreamableFile> {
    return next.handle().pipe(
      map((data) => {
        // Binary file streams (downloads) must bypass the JSON response envelope
        if (data instanceof StreamableFile) {
          return data;
        }

        if (isApiResponse<T>(data)) {
          return data;
        }

        if (isPaginationEnvelope<T>(data)) {
          return {
            success: true,
            data: (data.data ?? null) as T,
            error: null,
            pagination: data.pagination,
          };
        }

        return {
          success: true,
          data: (data ?? null) as T,
          error: null,
        };
      }),
    );
  }
}
