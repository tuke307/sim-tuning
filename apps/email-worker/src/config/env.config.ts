export const EMAIL_WORKER_PORT = Number(process.env.EMAIL_WORKER_PORT) || 3002;
export const REDIS_URL = process.env.REDIS_URL || 'redis://localhost:6379';
export const SMTP_HOST = process.env.SMTP_HOST || 'localhost';
export const SMTP_PORT = Number(process.env.SMTP_PORT) || 1025;
export const SMTP_USER = process.env.SMTP_USER;
export const SMTP_PASS = process.env.SMTP_PASS;
export const EMAIL_FROM = process.env.EMAIL_FROM || 'noreply@simtuning.app';
