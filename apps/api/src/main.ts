import { NestFactory } from '@nestjs/core';
import { NestExpressApplication } from '@nestjs/platform-express';
import { DocumentBuilder, SwaggerModule } from '@nestjs/swagger';
import helmet from 'helmet';
import cookieParser from 'cookie-parser';
import { AppModule } from './app.module';
import { API_PORT, NODE_ENV, FRONTEND_URL } from './config/env.config';
import { logger, NestLoggerService } from '@sim-tuning/logger';
import { ZodValidationPipe } from './pipes/zod-validation.pipe';
import { HttpExceptionFilter } from './filters/http-exception.filter';
import { LoggingInterceptor } from './interceptors/logging.interceptor';
import { ApiResponseInterceptor } from './interceptors/api-response.interceptor';

/** docs/sim-api.md: request bodies up to 5 MB (MAX_DYNO_POINTS per series). */
const JSON_BODY_LIMIT = '5mb';

async function bootstrap() {
  const app = await NestFactory.create<NestExpressApplication>(AppModule, {
    // Route Nest's own framework logs through Winston so every line is JSON in
    // production (see @sim-tuning/logger NestLoggerService). Keeps Loki's | json parser
    // from erroring on bootstrap/routing output like "Mapped {/api/media, GET}".
    logger: new NestLoggerService(),
    // Registered below with an explicit limit (dyno series uploads/imports up to 5 MB).
    bodyParser: false,
  });

  app.useBodyParser('json', { limit: JSON_BODY_LIMIT });
  app.useBodyParser('urlencoded', { limit: '100kb', extended: true });

  // Security middleware
  app.use(
    helmet({
      contentSecurityPolicy: {
        // Provide a complete directive set: in Helmet v8, passing any
        // directives replaces the default set, so specifying only
        // frame-ancestors previously dropped default-src/script-src/etc.
        directives: {
          'default-src': ["'self'"],
          'script-src': ["'self'", "'unsafe-inline'"],
          'style-src': ["'self'", "'unsafe-inline'"],
          'img-src': ["'self'", 'data:', 'https:'],
          'frame-ancestors': ["'self'"],
          'object-src': ["'none'"],
          'base-uri': ["'self'"],
        },
      },
    }),
  );

  // CORS configuration
  app.enableCors({
    origin: [FRONTEND_URL],
    credentials: true,
    optionsSuccessStatus: 200,
  });

  // Trust proxy for X-Forwarded-For headers
  app.set('trust proxy', 1);

  // Cookie parser
  app.use(cookieParser());

  // Global pipes, filters, and interceptors
  app.useGlobalPipes(new ZodValidationPipe());
  app.useGlobalFilters(new HttpExceptionFilter());
  app.useGlobalInterceptors(new ApiResponseInterceptor());

  // Only use logging interceptor in development
  if (NODE_ENV === 'development') {
    app.useGlobalInterceptors(new LoggingInterceptor());
  }

  // Global prefix
  app.setGlobalPrefix('api');

  // Swagger documentation (non-production only)
  if (NODE_ENV !== 'production') {
    const config = new DocumentBuilder()
      .setTitle('SimTuning API')
      .setDescription('Two-stroke engine simulation & dyno tuning API')
      .setVersion('1.0')
      .addBearerAuth()
      .addServer('/')
      .build();

    const document = SwaggerModule.createDocument(app, config);
    SwaggerModule.setup('api-docs', app, document);
  }

  // Enable graceful shutdown hooks
  app.enableShutdownHooks();

  await app.listen(API_PORT);
  logger.info(`Server is running on port ${API_PORT}`);
}

// Graceful shutdown handlers
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
  logger.error('Error starting server:', error);
  process.exit(1);
});
