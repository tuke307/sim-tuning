import {
  Injectable,
  CanActivate,
  ExecutionContext,
  UnauthorizedException,
  HttpException,
} from '@nestjs/common';
import type { Request } from 'express';
import { verifyAccessToken } from '@/utils/jwt.util';
import { getRedis } from '@/utils/redis.util';
import { PrismaService } from '@/modules/database/prisma.service';
import { logger } from '@sim-tuning/logger';
import type { AuthenticatedUser } from '@/decorators/current-user.decorator';

export type AuthTransport = 'bearer' | 'cookie';

export function extractBearerToken(request: Request): string | null {
  const authHeader = request.headers.authorization;
  if (!authHeader) return null;
  const raw = Array.isArray(authHeader) ? authHeader[0] : authHeader;
  if (!raw) return null;
  const match = /^Bearer\s+(.+)$/i.exec(raw.trim());
  return match?.[1]?.trim() || null;
}

@Injectable()
export class AuthGuard implements CanActivate {
  public checkDenylist?: (jti: string) => Promise<boolean>;

  constructor(private readonly prisma: PrismaService) {}

  async canActivate(context: ExecutionContext): Promise<boolean> {
    const request = context
      .switchToHttp()
      .getRequest<Request & { user?: AuthenticatedUser; authTransport?: AuthTransport }>();
    const bearerToken = extractBearerToken(request);
    const cookieToken = request.cookies?.accessToken as string | undefined;
    const accessToken = bearerToken ?? cookieToken;

    if (!accessToken) {
      throw new UnauthorizedException('Access token missing');
    }

    const authTransport: AuthTransport = bearerToken ? 'bearer' : 'cookie';

    try {
      const decoded = verifyAccessToken(accessToken);

      if (decoded.jti) {
        try {
          const denied = this.checkDenylist
            ? await this.checkDenylist(decoded.jti)
            : Boolean(await getRedis().get(`access_denylist:${decoded.jti}`));
          if (denied) {
            throw new UnauthorizedException('Session revoked');
          }
        } catch (err) {
          if (err instanceof HttpException) throw err;
          logger.debug(`Access-token denylist check skipped: ${err}`);
        }
      }

      if (!decoded || typeof decoded.id !== 'string' || !decoded.id.trim()) {
        throw new UnauthorizedException('Invalid access token payload');
      }

      const user = await this.prisma.user.findUnique({
        where: { id: decoded.id },
        select: { id: true, email: true },
      });

      if (!user) {
        throw new UnauthorizedException('User account not found');
      }

      request.user = {
        id: user.id,
        email: user.email,
      };
      request.authTransport = authTransport;

      return true;
    } catch (err) {
      if (err instanceof HttpException) throw err;
      logger.debug('Auth guard error:', err);
      throw new UnauthorizedException('Invalid or expired access token');
    }
  }
}
