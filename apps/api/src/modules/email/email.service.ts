import { Injectable } from '@nestjs/common';
import type { Email } from '@sim-tuning/database';
import { logger } from '@sim-tuning/logger';
import nodemailer, { type Transporter } from 'nodemailer';
import { EmailStatus, EmailTemplateType } from '@sim-tuning/common/enums';
import type { CreateEmailDto, UpdateEmailDto } from '@sim-tuning/common/schemas';
import {
  ACCOUNT_DELETION_EXPIRY_MS,
  EMAIL_FROM,
  FRONTEND_URL,
  NODE_ENV,
  PASSWORD_RESET_EXPIRY_MS,
  SMTP_HOST,
  SMTP_PASS,
  SMTP_PORT,
  SMTP_REJECT_UNAUTHORIZED,
  SMTP_USER,
} from '../../config/env.config';
import { PrismaService } from '../database/prisma.service';
import { BullMQService } from '../bullmq/bullmq.service';
import { EmailTemplateService } from '../email-template/email-template.service';
import { EmailTemplateRendererService } from '../email-template/email-template-renderer.service';

export interface EmailTemplateData extends Record<string, unknown> {
  name?: string;
  email?: string;
  confirmation_url?: string;
  reset_password_url?: string;
  delete_account_url?: string;
  expiry_minutes?: number;
  frontend_url?: string;
}

@Injectable()
export class EmailService {
  private static instance: EmailService;
  private transporter: Transporter;
  private readonly emailTemplateService: EmailTemplateService;
  private readonly templateRenderer: EmailTemplateRendererService;

  constructor(
    private readonly prisma: PrismaService,
    private readonly bullmqService: BullMQService,
    emailTemplateService?: EmailTemplateService,
    templateRenderer?: EmailTemplateRendererService,
  ) {
    this.templateRenderer = templateRenderer ?? new EmailTemplateRendererService();
    this.emailTemplateService =
      emailTemplateService ?? new EmailTemplateService(this.prisma, this.templateRenderer);

    this.transporter = nodemailer.createTransport({
      host: SMTP_HOST,
      port: SMTP_PORT,
      secure: SMTP_PORT === 465,
      requireTLS: SMTP_PORT !== 465 && NODE_ENV === 'production',
      ...(SMTP_USER
        ? {
            auth: {
              user: SMTP_USER,
              pass: SMTP_PASS,
            },
          }
        : {}),
      tls: {
        rejectUnauthorized: SMTP_REJECT_UNAUTHORIZED,
      },
    });

    EmailService.setInstance(this);
  }

  static getInstance(): EmailService {
    if (!EmailService.instance) {
      throw new Error('EmailService not initialized through NestJS DI');
    }
    return EmailService.instance;
  }

  static setInstance(instance: EmailService): void {
    EmailService.instance = instance;
  }

  async create(data: CreateEmailDto): Promise<Email> {
    if (!this.prisma?.email?.create) {
      return {
        id: Date.now(),
        recipientEmail: data.recipientEmail,
        subject: data.subject,
        content: data.content,
        status: data.status,
        scheduledAt: new Date(data.scheduledAt),
        sentAt: null,
        attempts: 0,
        lastAttemptAt: null,
        errorMessage: null,
        createdAt: new Date(),
        updatedAt: new Date(),
        emailTemplateId: data.emailTemplateId,
        userId: data.userId ?? null,
      };
    }

    const email = await this.prisma.email.create({
      data: {
        recipientEmail: data.recipientEmail,
        subject: data.subject,
        content: data.content,
        status: data.status,
        scheduledAt: new Date(data.scheduledAt),
        emailTemplateId: data.emailTemplateId,
        userId: data.userId ?? null,
      },
    });
    logger.info(`Email created: ${email.id}`);
    return email;
  }

  async update(id: number, data: UpdateEmailDto): Promise<Email | null> {
    if (!this.prisma?.email?.update) return null;
    const email = await this.prisma.email.update({
      where: { id },
      data: {
        ...(data.status !== undefined ? { status: data.status } : {}),
        ...(data.sentAt !== undefined ? { sentAt: new Date(data.sentAt) } : {}),
        ...(data.attempts !== undefined ? { attempts: data.attempts } : {}),
        ...(data.lastAttemptAt !== undefined
          ? { lastAttemptAt: new Date(data.lastAttemptAt) }
          : {}),
        ...(data.errorMessage !== undefined ? { errorMessage: data.errorMessage } : {}),
      },
    });
    logger.info(`Email updated: ${id}`);
    return email;
  }

  public async findEmailById(emailId: number): Promise<Email | null> {
    return this.prisma.email.findUnique({
      where: { id: emailId },
    });
  }

  public async enqueueAccountConfirmationEmail(
    user: { id: string; email: string; name?: string | null },
    rawToken: string,
  ): Promise<Email> {
    const confirmationUrl = `${FRONTEND_URL}/verify-email?token=${encodeURIComponent(rawToken)}`;
    return this.enqueueGenericEmail(
      EmailTemplateType.ACCOUNT_CONFIRMATION,
      user.email,
      {
        name: user.name || user.email.split('@')[0] || 'Tuner',
        email: user.email,
        confirmation_url: confirmationUrl,
        frontend_url: FRONTEND_URL,
      },
      new Date(),
      user.id,
    );
  }

  public async enqueuePasswordResetEmail(
    user: { id: string; email: string; name?: string | null },
    rawToken: string,
  ): Promise<Email> {
    const resetPasswordUrl = `${FRONTEND_URL}/reset-password?token=${encodeURIComponent(rawToken)}`;
    return this.enqueueGenericEmail(
      EmailTemplateType.PASSWORD_RESET,
      user.email,
      {
        name: user.name || user.email.split('@')[0] || 'Tuner',
        email: user.email,
        reset_password_url: resetPasswordUrl,
        expiry_minutes: Math.round(PASSWORD_RESET_EXPIRY_MS / 60000),
        frontend_url: FRONTEND_URL,
      },
      new Date(),
      user.id,
    );
  }

  public async enqueueAccountDeletionEmail(
    user: { id: string; email: string; name?: string | null },
    rawToken: string,
  ): Promise<Email> {
    const deleteAccountUrl = `${FRONTEND_URL}/delete-account?token=${encodeURIComponent(rawToken)}`;
    return this.enqueueGenericEmail(
      EmailTemplateType.ACCOUNT_DELETION,
      user.email,
      {
        name: user.name || user.email.split('@')[0] || 'Tuner',
        email: user.email,
        delete_account_url: deleteAccountUrl,
        expiry_minutes: Math.round(ACCOUNT_DELETION_EXPIRY_MS / 60000),
        frontend_url: FRONTEND_URL,
      },
      new Date(),
      user.id,
    );
  }

  public async enqueueGenericEmail(
    templateType: EmailTemplateType,
    recipientEmail: string,
    dynamicData: EmailTemplateData,
    scheduledAt: Date,
    userId?: string,
  ): Promise<Email> {
    const emailTemplate = await this.emailTemplateService.getEmailTemplateByType(templateType);
    const subject = this.templateRenderer.renderSubject(emailTemplate.subject, dynamicData);
    const htmlContent = await this.templateRenderer.renderTemplate(
      emailTemplate.htmlFile,
      dynamicData,
    );

    const emailRecord = await this.create({
      recipientEmail,
      subject,
      content: htmlContent,
      status: EmailStatus.PENDING,
      scheduledAt: scheduledAt.toISOString(),
      emailTemplateId: emailTemplate.id,
      userId: userId ?? null,
    });

    const delay = Math.max(0, scheduledAt.getTime() - Date.now());
    try {
      await this.bullmqService.addEmailJob(emailRecord.id, delay);
      logger.info(`Email ${emailRecord.id} queued in BullMQ (delay: ${delay}ms)`);
    } catch (err) {
      logger.warn(`BullMQ queue unavailable for email ${emailRecord.id}: ${err}`);
    }

    return emailRecord;
  }

  public async sendEmail(email: Email): Promise<void> {
    try {
      if (email.status === EmailStatus.SENT) {
        logger.info(`Email ${email.id} already sent, skipping duplicate delivery`);
        return;
      }

      await this.transporter.sendMail({
        from: EMAIL_FROM,
        to: email.recipientEmail,
        subject: email.subject,
        html: email.content,
      });

      await this.update(email.id, {
        status: EmailStatus.SENT,
        sentAt: new Date().toISOString(),
      });

      logger.info(`Email sent successfully: ${email.id} to ${email.recipientEmail}`);
    } catch (error: unknown) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error occurred';
      await this.update(email.id, {
        status: EmailStatus.FAILED,
        errorMessage,
      });
      logger.error(`Failed to send email (${email.id}): ${errorMessage}`);
      throw error;
    }
  }
}
