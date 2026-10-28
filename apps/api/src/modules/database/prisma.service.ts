import { Injectable, OnModuleInit, OnModuleDestroy } from '@nestjs/common';
import prisma from '@sim-tuning/database';
import { logger } from '@sim-tuning/logger';

@Injectable()
export class PrismaService implements OnModuleInit, OnModuleDestroy {
  get client(): typeof prisma {
    return prisma;
  }

  async onModuleInit() {
    try {
      await prisma.$connect();
      logger.info('Prisma client connected successfully');
    } catch (error) {
      logger.warn('Database not reachable at startup (continuing in resilient mode):', error);
    }
  }

  async onModuleDestroy() {
    try {
      await prisma.$disconnect();
      logger.info('Prisma client disconnected successfully');
    } catch (error) {
      logger.error('Error disconnecting from database:', error);
    }
  }

  get $transaction(): typeof prisma.$transaction {
    return prisma.$transaction.bind(prisma);
  }

  get user(): typeof prisma.user {
    return prisma.user;
  }

  get userPreferences(): typeof prisma.userPreferences {
    return prisma.userPreferences;
  }

  get refreshToken(): typeof prisma.refreshToken {
    return prisma.refreshToken;
  }

  get emailTemplate(): typeof prisma.emailTemplate {
    return prisma.emailTemplate;
  }

  get email(): typeof prisma.email {
    return prisma.email;
  }
}
