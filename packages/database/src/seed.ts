import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { parse as parseJsonc } from 'jsonc-parser';
import prisma from './client';
import logger from '@sim-tuning/logger';
import { STARTER_ENVIRONMENTS, STARTER_VEHICLES } from '@sim-tuning/common/catalog';

interface SeedUser {
  email: string;
  name: string;
  passwordHash: string;
}

const currentDir = path.dirname(fileURLToPath(import.meta.url));

async function loadSeedUsers(): Promise<SeedUser[]> {
  const filePath = path.join(currentDir, '..', 'data', 'users.jsonc');
  return parseJsonc(await fs.promises.readFile(filePath, 'utf-8'));
}

async function main(): Promise<void> {
  const seedUsers = await loadSeedUsers();

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

      await tx.emailTemplate.upsert({
        where: { type: 'ACCOUNT_CONFIRMATION' },
        update: {
          name: 'Account Confirmation',
          subject: 'SimTuning — Confirm your account ({{ name }})',
          htmlFile: 'account-confirmation.mjml',
        },
        create: {
          name: 'Account Confirmation',
          subject: 'SimTuning — Confirm your account ({{ name }})',
          htmlFile: 'account-confirmation.mjml',
          type: 'ACCOUNT_CONFIRMATION',
        },
      });

      await tx.emailTemplate.upsert({
        where: { type: 'PASSWORD_RESET' },
        update: {
          name: 'Password Reset',
          subject: 'SimTuning — Reset your password',
          htmlFile: 'password-reset.mjml',
        },
        create: {
          name: 'Password Reset',
          subject: 'SimTuning — Reset your password',
          htmlFile: 'password-reset.mjml',
          type: 'PASSWORD_RESET',
        },
      });

      await tx.emailTemplate.upsert({
        where: { type: 'ACCOUNT_DELETION' },
        update: {
          name: 'Account Deletion',
          subject: 'SimTuning — Confirm account deletion',
          htmlFile: 'account-deletion.mjml',
        },
        create: {
          name: 'Account Deletion',
          subject: 'SimTuning — Confirm account deletion',
          htmlFile: 'account-deletion.mjml',
          type: 'ACCOUNT_DELETION',
        },
      });

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
