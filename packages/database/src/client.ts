import { PrismaPg } from '@prisma/adapter-pg';
import { PrismaClient } from './generated/prisma/client';
import { createPaginator } from 'prisma-extension-pagination';
import { DATABASE_URL } from './config/env.config';

const adapter = new PrismaPg({ connectionString: DATABASE_URL });

const paginate = createPaginator({
  pages: {
    limit: 20,
    includePageCount: true,
  },
});

const prisma = new PrismaClient({ adapter }).$extends({
  model: {
    $allModels: {
      paginate,
    },
  },
});

export default prisma;

export { Prisma } from './generated/prisma/client';

export type {
  User,
  UserPreferences,
  RefreshToken,
  EmailTemplate,
  Email,
  Environment,
  Vehicle,
  Motor,
  Einlass,
  Vergaser,
  Auslass,
  Auspuff,
  Ueberstroemer,
  Dyno,
  Drehzahl,
  DynoPs,
  Ausrollen,
  Geschwindigkeit,
} from './generated/prisma/client';

export { EmailTemplateType, EmailStatus } from './generated/prisma/client';
