import { Injectable } from '@nestjs/common';
import { createHash } from 'crypto';
import type { RefreshToken } from '@sim-tuning/database';
import { PrismaService } from '../database/prisma.service';

@Injectable()
export class RefreshTokenService {
  constructor(private readonly prisma: PrismaService) {}

  private hashToken(token: string): string {
    return createHash('sha256').update(token).digest('hex');
  }

  async create(data: { token: string; userId: string; expiresAt: Date }): Promise<RefreshToken> {
    return this.prisma.refreshToken.create({
      data: {
        token: this.hashToken(data.token),
        userId: data.userId,
        expiresAt: data.expiresAt,
      },
    });
  }

  async claimToken(token: string): Promise<boolean> {
    const result = await this.prisma.refreshToken.deleteMany({
      where: {
        token: this.hashToken(token),
        expiresAt: { gt: new Date() },
      },
    });
    return result.count > 0;
  }

  async deleteByUserId(userId: string): Promise<void> {
    await this.prisma.refreshToken.deleteMany({
      where: { userId },
    });
  }
}
