import { Injectable } from '@nestjs/common';
import type { EmailTemplate } from '@sim-tuning/database';
import { EmailTemplateType } from '@sim-tuning/common/enums';
import { logger } from '@sim-tuning/logger';
import { PrismaService } from '../database/prisma.service';
import { EmailTemplateRendererService } from './email-template-renderer.service';

const DEFAULT_TEMPLATES: Record<
  EmailTemplateType,
  { name: string; subject: string; htmlFile: string }
> = {
  [EmailTemplateType.ACCOUNT_CONFIRMATION]: {
    name: 'Account Confirmation',
    subject: 'SimTuning — Confirm your account ({{ name }})',
    htmlFile: 'account-confirmation.mjml',
  },
  [EmailTemplateType.PASSWORD_RESET]: {
    name: 'Password Reset',
    subject: 'SimTuning — Reset your password',
    htmlFile: 'password-reset.mjml',
  },
  [EmailTemplateType.ACCOUNT_DELETION]: {
    name: 'Account Deletion',
    subject: 'SimTuning — Confirm account deletion',
    htmlFile: 'account-deletion.mjml',
  },
};

@Injectable()
export class EmailTemplateService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly templateRenderer: EmailTemplateRendererService = new EmailTemplateRendererService(),
  ) {}

  async getEmailTemplateByType(type: EmailTemplateType): Promise<EmailTemplate> {
    if (!this.prisma?.emailTemplate) {
      const fallback = DEFAULT_TEMPLATES[type];
      return {
        id:
          type === EmailTemplateType.ACCOUNT_CONFIRMATION
            ? 1
            : type === EmailTemplateType.PASSWORD_RESET
              ? 2
              : 3,
        name: fallback.name,
        subject: fallback.subject,
        htmlFile: fallback.htmlFile,
        type,
        updatedAt: new Date(),
      };
    }

    let template = await this.prisma.emailTemplate.findUnique({ where: { type } });
    if (!template) {
      const def = DEFAULT_TEMPLATES[type];
      template = await this.prisma.emailTemplate.upsert({
        where: { type },
        update: {},
        create: {
          name: def.name,
          subject: def.subject,
          htmlFile: def.htmlFile,
          type,
        },
      });
      logger.info(`Auto-seeded default EmailTemplate for ${type}`);
    }
    return template;
  }

  async getPreviewHtml(type: EmailTemplateType): Promise<string> {
    const template = await this.getEmailTemplateByType(type);
    return this.templateRenderer.renderTemplate(template.htmlFile, {
      name: 'Tuner',
      email: 'tuner@example.com',
      confirmation_url: 'http://localhost:3000/verify-email?token=sample-token',
      reset_password_url: 'http://localhost:3000/reset-password?token=sample-token',
      delete_account_url: 'http://localhost:3000/delete-account?token=sample-token',
      expiry_minutes: 60,
      frontend_url: 'http://localhost:3000',
    });
  }
}
