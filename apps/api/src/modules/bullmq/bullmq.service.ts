import { Injectable } from '@nestjs/common';
import type { ConnectionOptions } from 'bullmq';
import { KeepJobs, Queue, Worker } from 'bullmq';
import IORedis from 'ioredis';
import { logger } from '@sim-tuning/logger';
import { EMAIL_MAX_ATTEMPTS, EMAIL_RETRY_DELAY_MS, REDIS_URL } from '../../config/env.config';

export interface EmailJobData {
  emailId: number;
}

const EMAIL_QUEUE_NAME = 'email';

@Injectable()
export class BullMQService {
  private static instance: BullMQService;
  private connection?: IORedis;
  private emailQueue?: Queue<EmailJobData>;
  private emailWorker?: Worker<EmailJobData>;

  private ensureQueue(): Queue<EmailJobData> {
    if (!this.connection) {
      this.connection = new IORedis(REDIS_URL, {
        maxRetriesPerRequest: null,
        lazyConnect: true,
      });
      this.connection.on('error', (err) => {
        logger.debug(`BullMQ Redis connection error: ${err.message}`);
      });
    }
    if (!this.emailQueue) {
      this.emailQueue = new Queue<EmailJobData>(EMAIL_QUEUE_NAME, {
        connection: this.getBullConnection(),
      });
    }
    return this.emailQueue;
  }

  static getInstance(): BullMQService {
    if (!BullMQService.instance) {
      BullMQService.instance = new BullMQService();
    }
    return BullMQService.instance;
  }

  async addEmailJob(emailId: number, delayMs: number = 0): Promise<void> {
    const queue = this.ensureQueue();
    await queue.add(
      'send-email',
      { emailId },
      {
        delay: delayMs,
        attempts: EMAIL_MAX_ATTEMPTS,
        backoff: { type: 'exponential', delay: EMAIL_RETRY_DELAY_MS },
      },
    );
    logger.info(`Email job added: ${emailId}`);
  }

  startEmailWorker(processor: (emailId: number) => Promise<void>): void {
    this.ensureQueue();
    this.emailWorker = new Worker<EmailJobData>(
      EMAIL_QUEUE_NAME,
      async (job) => {
        await processor(job.data.emailId);
      },
      {
        connection: this.getBullConnection(),
        concurrency: 5,
        removeOnComplete: { count: 10 } as KeepJobs,
        removeOnFail: { count: 5 } as KeepJobs,
      },
    );

    this.emailWorker.on('completed', (job) => {
      logger.info(`Email job completed: ${job.data.emailId}`);
    });

    this.emailWorker.on('failed', (job, err) => {
      logger.error(`Email job failed: ${job?.data?.emailId}`, err);
    });

    logger.info('Email worker started');
  }

  async getQueueStats() {
    const queue = this.ensureQueue();
    return queue.getJobCounts();
  }

  async close(): Promise<void> {
    if (this.emailWorker) {
      await this.emailWorker.close();
    }
    if (this.emailQueue) {
      await this.emailQueue.close();
    }
    if (this.connection) {
      await this.connection.quit();
    }
    logger.info('BullMQ service closed');
  }

  private getBullConnection(): ConnectionOptions {
    return this.connection as unknown as ConnectionOptions;
  }
}
