import jwt, { JwtPayload } from 'jsonwebtoken';
import {
  ACCESS_TOKEN_EXPIRY_MS,
  getJwtRefreshSecret,
  getJwtSecret,
  REFRESH_TOKEN_EXPIRY_MS,
} from '@/config/env.config';
import crypto from 'crypto';
import type { UserPayloadDto } from '@sim-tuning/common/schemas';

export const JWT_ALGORITHM = 'HS512';

export const generateAccessToken = (user: { id: string; email: string }): string => {
  const payload = {
    id: user.id,
    email: user.email,
    jti: crypto.randomBytes(16).toString('hex'),
  };

  return jwt.sign(payload, getJwtSecret(), {
    expiresIn: Math.floor(ACCESS_TOKEN_EXPIRY_MS / 1000),
    algorithm: JWT_ALGORITHM,
  });
};

export const generateRefreshToken = (user: { id: string; email?: string }): string => {
  const payload = {
    id: user.id,
    jti: crypto.randomBytes(16).toString('hex'),
  };

  return jwt.sign(payload, getJwtRefreshSecret(), {
    expiresIn: Math.floor(REFRESH_TOKEN_EXPIRY_MS / 1000),
    algorithm: JWT_ALGORITHM,
  });
};

export const verifyAccessToken = (token: string): UserPayloadDto & JwtPayload => {
  return jwt.verify(token, getJwtSecret(), {
    algorithms: [JWT_ALGORITHM],
  }) as UserPayloadDto & JwtPayload;
};

export const verifyRefreshToken = (token: string): UserPayloadDto & JwtPayload => {
  return jwt.verify(token, getJwtRefreshSecret(), {
    algorithms: [JWT_ALGORITHM],
  }) as UserPayloadDto & JwtPayload;
};

export const generateCsrfToken = (): string => {
  return crypto.randomBytes(24).toString('hex');
};
