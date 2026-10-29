import { DIRECT_API_URL, NODE_ENV, SERVER_API_URL } from '@/lib/config/env.config';
import type { ApiResponse } from '@sim-tuning/common/api/responses/api.response';

const isServer = typeof window === 'undefined';
const API_URL = '/api';

let refreshInFlight: Promise<boolean> | null = null;

async function tryRefreshSession(): Promise<boolean> {
  if (refreshInFlight) {
    return refreshInFlight;
  }
  refreshInFlight = (async () => {
    try {
      const refreshed = await fetchClient('/auth/refresh-token', {
        method: 'POST',
        retry: false,
      });
      return Boolean(refreshed.success);
    } catch {
      return false;
    } finally {
      refreshInFlight = null;
    }
  })();
  return refreshInFlight;
}

export class ApiClientError extends Error {
  status: number;
  isAuthError: boolean;
  isNetworkError: boolean;

  constructor(
    message: string,
    opts: { status?: number; isAuthError?: boolean; isNetworkError?: boolean } = {},
  ) {
    super(message);
    this.name = 'ApiClientError';
    this.status = opts.status ?? 0;
    this.isAuthError = opts.isAuthError ?? false;
    this.isNetworkError = opts.isNetworkError ?? false;
  }
}

export async function fetchClient<T = null>(
  endpoint: string,
  options: RequestInit & { retry?: boolean } = {},
): Promise<ApiResponse<T>> {
  const { retry = true, ...fetchOptions } = options;
  const isFormData = fetchOptions.body instanceof FormData;

  try {
    const Cookies = (await import('js-cookie')).default;
    const csrfToken = Cookies.get('csrfToken');

    const headers = new Headers({
      'X-CSRF-Token': csrfToken || '',
      ...(fetchOptions.headers || {}),
    });

    if (!isFormData && !headers.has('Content-Type')) {
      headers.set('Content-Type', 'application/json');
    }
    // The app's resolved UI language (kept on <html lang> by LocaleProvider) selects the language of
    // catalog content returned by the API.
    if (typeof document !== 'undefined' && document.documentElement.lang) {
      headers.set('Accept-Language', document.documentElement.lang);
    }

    let response: Response;
    try {
      response = await fetch(`${API_URL}${endpoint}`, {
        ...fetchOptions,
        headers,
        credentials: 'include',
        body: fetchOptions.body,
      });
    } catch (netErr) {
      throw new ApiClientError(
        netErr instanceof Error ? netErr.message : 'Network connection unavailable',
        { isNetworkError: true },
      );
    }

    if (
      response.status === 401 &&
      retry &&
      endpoint !== '/auth/login' &&
      endpoint !== '/auth/register' &&
      endpoint !== '/auth/refresh-token'
    ) {
      const refreshedOk = await tryRefreshSession();
      if (refreshedOk) {
        return fetchClient<T>(endpoint, { ...options, retry: false });
      }

      throw new ApiClientError('Authentication expired', {
        status: 401,
        isAuthError: true,
      });
    }

    const text = await response.text();
    const responseData = text
      ? (JSON.parse(text) as ApiResponse<T>)
      : ({ success: true, data: null, error: null } as ApiResponse<T>);

    if (!response.ok) {
      throw new ApiClientError(responseData.error || `Request failed (${response.status})`, {
        status: response.status,
        isAuthError: response.status === 401,
      });
    }

    return responseData;
  } catch (error) {
    if (error instanceof ApiClientError) {
      throw error;
    }
    throw new ApiClientError(error instanceof Error ? error.message : 'Request failed', {
      isNetworkError: true,
    });
  }
}

export async function fetchServer<T = null>(endpoint: string): Promise<ApiResponse<T>> {
  try {
    const response = await fetch(`${SERVER_API_URL}${endpoint}`, {
      method: 'GET',
      cache: 'no-store',
      headers: {
        'Content-Type': 'application/json',
      },
      signal: AbortSignal.timeout(5000),
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const data = await response.json();
    return data as ApiResponse<T>;
  } catch (error) {
    return {
      success: false,
      data: null,
      error: error instanceof Error ? error.message : 'Unknown error',
    } as ApiResponse<T>;
  }
}

export async function apiFetch<T = null>(
  endpoint: string,
  options: RequestInit & { retry?: boolean } = {},
): Promise<ApiResponse<T>> {
  if (isServer) {
    return fetchServer<T>(endpoint);
  }
  return fetchClient<T>(endpoint, options);
}

export { API_URL, NODE_ENV, DIRECT_API_URL, isServer };
