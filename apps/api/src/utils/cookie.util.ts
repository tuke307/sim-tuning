import {
  NODE_ENV,
  ACCESS_TOKEN_EXPIRY_MS,
  REFRESH_TOKEN_EXPIRY_MS,
  COOKIE_DOMAIN,
} from '@/config/env.config';
import type { CookieOptions, Response } from 'express';

const cookieDomain = COOKIE_DOMAIN;

// Common cookie options for access tokens
export const accessTokenCookieOptions: CookieOptions = {
  httpOnly: true, // HTTP-only cookie to prevent JavaScript access
  secure: NODE_ENV === 'production', // Secure cookies in production (requires HTTPS)
  sameSite: NODE_ENV === 'production' ? 'lax' : 'strict', // same-origin in production
  maxAge: ACCESS_TOKEN_EXPIRY_MS,
  domain: cookieDomain,
  path: '/',
};

// Common cookie options for refresh tokens
export const refreshTokenCookieOptions: CookieOptions = {
  httpOnly: true,
  secure: NODE_ENV === 'production',
  sameSite: NODE_ENV === 'production' ? 'lax' : 'strict',
  maxAge: REFRESH_TOKEN_EXPIRY_MS,
  domain: cookieDomain,
  path: '/',
};

// CSRF token cookie options (not httpOnly as it's accessible by JavaScript)
export const csrfCookieOptions: CookieOptions = {
  httpOnly: false, // CSRF token is accessible via JavaScript
  secure: NODE_ENV === 'production',
  sameSite: NODE_ENV === 'production' ? 'lax' : 'strict',
  maxAge: REFRESH_TOKEN_EXPIRY_MS, // Aligned with refresh token expiry so token refresh works after access token expires
  domain: cookieDomain,
  path: '/',
};

// Utility function to set or clear cookies
export function setCookie(
  res: Response,
  name: string,
  value: string | null,
  options: CookieOptions,
) {
  if (value === null) {
    // Clear the cookie by setting its value to null
    res.clearCookie(name, options);
  } else {
    // Set the cookie with the provided value and options
    res.cookie(name, value, options);
  }
}
