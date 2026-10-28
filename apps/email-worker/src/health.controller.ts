import { Controller, Get } from '@nestjs/common';
import { WorkerService } from './worker.service';

@Controller()
export class HealthController {
  constructor(private readonly workerService: WorkerService) {}

  @Get('health')
  async getHealth() {
    try {
      const stats = await this.workerService.getQueueStats();
      return {
        status: 'healthy',
        queue: stats,
        redis: 'connected',
      };
    } catch {
      return {
        status: 'unhealthy',
      };
    }
  }
}
