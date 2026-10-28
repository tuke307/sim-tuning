import { logger } from '@sim-tuning/logger';

export const API_PORT = process.env.API_PORT || 3001;
export const NODE_ENV = process.env.NODE_ENV || 'development';

const isProduction = NODE_ENV === 'production';

const INSECURE_SECRET_VALUES = new Set([
  'your-jwt-secret-key',
  'your-jwt-refresh-secret-key',
  'secret',
  '',
]);

function resolveJwtSecret(envValue: string | undefined, name: string): string {
  const value = (envValue ?? '').trim();
  if (value.length >= 32 && !INSECURE_SECRET_VALUES.has(value)) {
    return value;
  }
  if (isProduction) {
    throw new Error(
      `${name} must be set to a high-entropy secret of at least 32 characters in production. Refusing to start.`,
    );
  }
  logger.warn(
    `${name} is missing or insecure — falling back to a dev-only default. NEVER use this in production.`,
  );
  return `dev-only-insecure-${name.toLowerCase()}-do-not-use-in-production`;
}

export const BCRYPT_SALT_ROUNDS = Number(process.env.BCRYPT_SALT_ROUNDS) || 10;
export const ACCESS_TOKEN_EXPIRY_MS = Number(process.env.ACCESS_TOKEN_EXPIRY_MS) || 900000;

let _jwtSecret: string | undefined;
export function getJwtSecret(): string {
  if (_jwtSecret === undefined) _jwtSecret = resolveJwtSecret(process.env.JWT_SECRET, 'JWT_SECRET');
  return _jwtSecret;
}

let _jwtRefreshSecret: string | undefined;
export function getJwtRefreshSecret(): string {
  if (_jwtRefreshSecret === undefined) {
    _jwtRefreshSecret = resolveJwtSecret(process.env.JWT_REFRESH_SECRET, 'JWT_REFRESH_SECRET');
  }
  return _jwtRefreshSecret;
}

export const REFRESH_TOKEN_EXPIRY_MS = Number(process.env.REFRESH_TOKEN_EXPIRY_MS) || 604800000;

export const DATABASE_URL =
  process.env.DATABASE_URL || `postgresql://postgres:postgres@localhost:5432/sim_tuning`;

export const FRONTEND_URL = process.env.FRONTEND_URL || 'http://localhost:3000';
export const COOKIE_DOMAIN = process.env.COOKIE_DOMAIN || undefined;
export const REDIS_URL = process.env.REDIS_URL || 'redis://localhost:6379';

// Email & BullMQ Configuration
export const SMTP_HOST = process.env.SMTP_HOST || 'localhost';
export const SMTP_PORT = Number(process.env.SMTP_PORT) || 1025;
export const SMTP_USER = process.env.SMTP_USER || '';
export const SMTP_PASS = process.env.SMTP_PASS || '';
export const SMTP_REJECT_UNAUTHORIZED = process.env.SMTP_REJECT_UNAUTHORIZED !== 'false';
export const EMAIL_FROM = process.env.EMAIL_FROM || 'noreply@simtuning.app';
export const EMAIL_MAX_ATTEMPTS = Number(process.env.EMAIL_MAX_ATTEMPTS) || 3;
export const EMAIL_RETRY_DELAY_MS = Number(process.env.EMAIL_RETRY_DELAY_MS) || 5000;

export const EMAIL_VERIFICATION_EXPIRY_MS =
  Number(process.env.EMAIL_VERIFICATION_EXPIRY_MS) || 24 * 60 * 60 * 1000;
export const PASSWORD_RESET_EXPIRY_MS =
  Number(process.env.PASSWORD_RESET_EXPIRY_MS) || 60 * 60 * 1000;
export const ACCOUNT_DELETION_EXPIRY_MS =
  Number(process.env.ACCOUNT_DELETION_EXPIRY_MS) || 60 * 60 * 1000;

// Web analytics (self-hosted Umami, pageviews only). Forwarding is disabled unless
// NODE_ENV=production and UMAMI_WEBSITE_ID is set.
const DEFAULT_UMAMI_URL = 'https://umami.tony-luke.de';

function resolveUmamiUrl(envValue: string | undefined): string {
  const raw = (envValue ?? '').trim();
  if (!raw) return DEFAULT_UMAMI_URL;
  try {
    const parsed = new URL(raw);
    // Operator-configured target only; still require HTTPS so pageview data
    // (incl. the visitor IP header) is never sent in clear text.
    if (parsed.protocol !== 'https:' && isProduction) {
      logger.warn('UMAMI_URL must use https — falling back to the default Umami URL.');
      return DEFAULT_UMAMI_URL;
    }
    return parsed.origin + parsed.pathname.replace(/\/+$/, '');
  } catch {
    logger.warn('UMAMI_URL is not a valid URL — falling back to the default Umami URL.');
    return DEFAULT_UMAMI_URL;
  }
}

export const UMAMI_URL = resolveUmamiUrl(process.env.UMAMI_URL);
export const UMAMI_WEBSITE_ID = (process.env.UMAMI_WEBSITE_ID ?? '').trim();
export const UMAMI_ALLOWED_HOSTNAMES = (
  process.env.UMAMI_ALLOWED_HOSTNAMES || 'simtuning.tony-luke.de'
)
  .split(',')
  .map((host) => host.trim().toLowerCase())
  .filter(Boolean);
