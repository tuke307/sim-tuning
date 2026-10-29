import { Module } from '@nestjs/common';
import { EmailTemplateService } from './email-template.service';
import { EmailTemplateRendererService } from './email-template-renderer.service';

@Module({
  providers: [EmailTemplateService, EmailTemplateRendererService],
  exports: [EmailTemplateService, EmailTemplateRendererService],
})
export class EmailTemplateModule {}
