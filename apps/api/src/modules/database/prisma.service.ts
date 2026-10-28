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

  get environment(): typeof prisma.environment {
    return prisma.environment;
  }

  get vehicle(): typeof prisma.vehicle {
    return prisma.vehicle;
  }

  get motor(): typeof prisma.motor {
    return prisma.motor;
  }

  get einlass(): typeof prisma.einlass {
    return prisma.einlass;
  }

  get vergaser(): typeof prisma.vergaser {
    return prisma.vergaser;
  }

  get auslass(): typeof prisma.auslass {
    return prisma.auslass;
  }

  get auspuff(): typeof prisma.auspuff {
    return prisma.auspuff;
  }

  get ueberstroemer(): typeof prisma.ueberstroemer {
    return prisma.ueberstroemer;
  }

  get dyno(): typeof prisma.dyno {
    return prisma.dyno;
  }

  get drehzahl(): typeof prisma.drehzahl {
    return prisma.drehzahl;
  }

  get dynoPs(): typeof prisma.dynoPs {
    return prisma.dynoPs;
  }

  get ausrollen(): typeof prisma.ausrollen {
    return prisma.ausrollen;
  }

  get geschwindigkeit(): typeof prisma.geschwindigkeit {
    return prisma.geschwindigkeit;
  }
}

/** Interactive transaction client passed to `prisma.$transaction(async (tx) => …)`. */
export type PrismaTx = Parameters<Parameters<PrismaService['$transaction']>[0]>[0];
