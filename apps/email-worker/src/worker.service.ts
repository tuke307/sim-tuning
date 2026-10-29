import { Injectable, OnModuleDestroy } from '@nestjs/common';
import { logger } from '@sim-tuning/logger';
import { EmailService } from '@sim-tuning/api/services/email.service';
import { BullMQService } from '@sim-tuning/api/services/bullmq.service';
import { PrismaService } from '@sim-tuning/api/services/prisma.service';

@Injectable()
export class WorkerService implements OnModuleDestroy {
  private readonly bullmqService: BullMQService;
  private readonly emailService: EmailService;

  constructor(private readonly prismaService: PrismaService) {
    this.bullmqService = BullMQService.getInstance();
    this.emailService = new EmailService(this.prismaService, this.bullmqService);
  }

  async startWorker(): Promise<void> {
    logger.info('Starting email worker...');

    this.bullmqService.startEmailWorker(async (emailId) => {
      await this.processEmailJob(emailId);
    });

    logger.info('Email worker started successfully');
  }

  private async processEmailJob(emailId: number): Promise<void> {
    try {
      const email = await this.emailService.findEmailById(emailId);

      if (!email) {
        logger.error(`Email ${emailId} not found`);
        throw new Error(`Email ${emailId} not found`);
      }

      await this.emailService.sendEmail(email);
    } catch (error) {
      logger.error(`Error processing email job ${emailId}:`, error);
      throw error;
    }
  }

  async getQueueStats() {
    return this.bullmqService.getQueueStats();
  }

  async onModuleDestroy() {
    logger.info('Shutting down email worker...');
    await this.bullmqService.close();
    logger.info('Email worker shut down successfully');
  }
}
