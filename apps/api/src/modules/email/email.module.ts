import { Module } from '@nestjs/common';
import { EmailService } from './email.service';
import { BullMQModule } from '../bullmq/bullmq.module';
import { EmailTemplateModule } from '../email-template/email-template.module';

@Module({
  imports: [BullMQModule, EmailTemplateModule],
  providers: [EmailService],
  exports: [EmailService],
})
export class EmailModule {}
