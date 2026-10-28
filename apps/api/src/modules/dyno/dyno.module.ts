import { Module } from '@nestjs/common';
import { EnvironmentModule } from '../environment/environment.module';
import { VehicleModule } from '../vehicle/vehicle.module';
import { DynoController } from './dyno.controller';
import { DynoService } from './dyno.service';

@Module({
  imports: [VehicleModule, EnvironmentModule],
  controllers: [DynoController],
  providers: [DynoService],
})
export class DynoModule {}
