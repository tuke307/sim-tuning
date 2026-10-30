import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { parse as parseJsonc } from 'jsonc-parser';
import type { EmailTemplate } from './generated/prisma/client';
import prisma from './client';
import logger from '@sim-tuning/logger';
import { STARTER_ENVIRONMENTS, STARTER_VEHICLES } from '@sim-tuning/common/catalog';

interface SeedUser {
  email: string;
  name: string;
  passwordHash: string;
}

type SeedEmailTemplate = Pick<EmailTemplate, 'type' | 'name' | 'subject' | 'htmlFile'>;

const currentDir = path.dirname(fileURLToPath(import.meta.url));

async function loadJsonFile<T>(filename: string): Promise<T> {
  const filePath = path.join(currentDir, '..', 'data', filename);
  const fileContent = await fs.promises.readFile(filePath, 'utf-8');
  return filename.endsWith('.jsonc') ? parseJsonc(fileContent) : JSON.parse(fileContent);
}

async function main(): Promise<void> {
  // Load fixtures from disk first (no DB work, safe outside the transaction).
  const seedUsers = await loadJsonFile<SeedUser[]>('users.jsonc');
  const emailTemplates = await loadJsonFile<SeedEmailTemplate[]>('email_templates.json');

  try {
    await prisma.$transaction(async (tx) => {
      // Skipped in production so fixture credentials never reach a live database.
      if (process.env.NODE_ENV !== 'production') {
        for (const seedUser of seedUsers) {
          const email = seedUser.email.toLowerCase();
          await tx.user.upsert({
            where: { email },
            update: {},
            create: { email, name: seedUser.name, passwordHash: seedUser.passwordHash },
          });
        }
      }

      for (const { type, ...template } of emailTemplates) {
        await tx.emailTemplate.upsert({
          where: { type },
          update: template,
          create: { type, ...template },
        });
      }

      for (const env of STARTER_ENVIRONMENTS) {
        const { systemKey, ...data } = env;
        await tx.environment.upsert({
          where: { systemKey },
          update: data,
          create: { systemKey, ...data },
        });
      }

      // Starter vehicles are shared read-only templates (userId = null). They are only created
      // once; later edits to the catalog do not overwrite rows (users duplicate them instead).
      for (const vehicle of STARTER_VEHICLES) {
        const existing = await tx.vehicle.findUnique({ where: { systemKey: vehicle.systemKey } });
        if (existing) continue;
        const { motor } = vehicle;
        await tx.vehicle.create({
          data: {
            systemKey: vehicle.systemKey,
            name: vehicle.name,
            beschreibung: vehicle.beschreibung,
            gewicht: vehicle.gewicht,
            motor: {
              create: {
                name: motor.name,
                hubL: motor.hubL,
                bohrungD: motor.bohrungD,
                pleulL: motor.pleulL,
                deachsierungL: motor.deachsierungL,
                resonanzU: motor.resonanzU,
                hubraumV: motor.hubraumV,
                brennraumV: motor.brennraumV,
                kurbelgehaeuseV: motor.kurbelgehaeuseV,
                einlass: {
                  create: { steuerzeitSZ: motor.einlassSteuerzeitSZ, vergaser: { create: {} } },
                },
                auslass: {
                  create: { steuerzeitSZ: motor.auslassSteuerzeitSZ, auspuff: { create: {} } },
                },
                ueberstroemer: { create: { steuerzeitSZ: motor.ueberstroemerSteuerzeitSZ } },
              },
            },
          },
        });
      }
    });

    logger.info('SimTuning email templates, environments and starter vehicles seeded');
  } catch (error) {
    logger.error('Error seeding SimTuning database:', error);
    await prisma.$disconnect();
    process.exit(1);
  } finally {
    await prisma.$disconnect();
  }
}

main();
