import { NestFactory } from '@nestjs/core';
import { NestExpressApplication } from '@nestjs/platform-express';
import { AppModule } from './app.module';
import { EMAIL_WORKER_PORT } from './config/env.config';
import { logger, NestLoggerService } from '@sim-tuning/logger';
import { WorkerService } from './worker.service';

async function bootstrap() {
  const app = await NestFactory.create<NestExpressApplication>(AppModule, {
    logger: new NestLoggerService(),
  });

  app.enableShutdownHooks();

  const workerService = app.get(WorkerService);
  await workerService.startWorker();

  await app.listen(EMAIL_WORKER_PORT);
  logger.info(`Email Worker is running on port ${EMAIL_WORKER_PORT}`);
}

process.on('SIGTERM', () => {
  logger.info('SIGTERM signal received: closing HTTP server gracefully');
});

process.on('SIGINT', () => {
  logger.info('SIGINT signal received: closing HTTP server gracefully');
});

process.on('uncaughtException', (error: Error) => {
  logger.error('Uncaught Exception:', error);
  process.exit(1);
});

process.on('unhandledRejection', (reason: unknown, promise: Promise<unknown>) => {
  logger.error('Unhandled Rejection at:', promise, 'reason:', reason);
  process.exit(1);
});

bootstrap().catch((error) => {
  logger.error('Error starting email worker:', error);
  process.exit(1);
});
