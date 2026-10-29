export const NODE_ENV = process.env.NODE_ENV || 'development';
export const SITE_URL = process.env.NEXT_PUBLIC_SITE_URL || 'http://localhost:3000';

export const SERVER_API_URL = (
  process.env.API_URL ||
  process.env.NEXT_PUBLIC_DIRECT_API_URL ||
  `http://localhost:${process.env.API_PORT || 3001}/api`
).replace(/\/+$/, '');

export const DIRECT_API_URL = process.env.NEXT_PUBLIC_DIRECT_API_URL;
export const JWT_SECRET = process.env.JWT_SECRET;
