import prisma from './client';
import logger from '@sim-tuning/logger';

async function main(): Promise<void> {
  try {
    await prisma.$transaction(async (tx) => {
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
    });

    logger.info('SimTuning email templates seeded successfully');
  } catch (error) {
    logger.error('Error seeding SimTuning database:', error);
    await prisma.$disconnect();
    process.exit(1);
  } finally {
    await prisma.$disconnect();
  }
}

main();
