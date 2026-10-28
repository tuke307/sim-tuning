import { Module } from '@nestjs/common';
import { BullMQService } from './bullmq.service';

@Module({
  providers: [BullMQService],
  exports: [BullMQService],
})
export class BullMQModule {}
