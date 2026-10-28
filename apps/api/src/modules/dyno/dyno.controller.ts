import { Body, Controller, Delete, Get, Param, Post, Put, UseGuards } from '@nestjs/common';
import { ApiOperation, ApiTags } from '@nestjs/swagger';
import {
  DynoDataInputSchema,
  DynoExportSchema,
  DynoInputSchema,
  type DynoDataInputDto,
  type DynoDto,
  type DynoExportDto,
  type DynoInputDto,
  type VehicleDto,
} from '@sim-tuning/common/schemas';
import { AuthGuard } from '@/guards/auth.guard';
import { CsrfGuard } from '@/guards/csrf.guard';
import { CurrentUser, type AuthenticatedUser } from '@/decorators/current-user.decorator';
import { ZodValidationPipe } from '@/pipes/zod-validation.pipe';
import { IdParamSchema } from '../sim/sim.fields';
import { DynoService } from './dyno.service';

@ApiTags('Dyno')
@Controller()
@UseGuards(AuthGuard)
export class DynoController {
  constructor(private readonly dynoService: DynoService) {}

  @Get('vehicles/:vehicleId/dyno')
  @ApiOperation({ summary: 'Get the dyno of a vehicle (null when none exists)' })
  getForVehicle(
    @CurrentUser() user: AuthenticatedUser,
    @Param('vehicleId', new ZodValidationPipe(IdParamSchema)) vehicleId: string,
  ): Promise<DynoDto | null> {
    return this.dynoService.getForVehicle(user.id, vehicleId);
  }

  @Put('vehicles/:vehicleId/dyno')
  @UseGuards(CsrfGuard)
  @ApiOperation({ summary: 'Create or update the dyno of an own vehicle' })
  save(
    @CurrentUser() user: AuthenticatedUser,
    @Param('vehicleId', new ZodValidationPipe(IdParamSchema)) vehicleId: string,
    @Body(new ZodValidationPipe(DynoInputSchema)) body: DynoInputDto,
  ): Promise<DynoDto> {
    return this.dynoService.save(user.id, vehicleId, body);
  }

  @Post('dynos/import')
  @UseGuards(CsrfGuard)
  @ApiOperation({ summary: 'Import a dyno export as a new own vehicle' })
  importDyno(
    @CurrentUser() user: AuthenticatedUser,
    @Body(new ZodValidationPipe(DynoExportSchema)) body: DynoExportDto,
  ): Promise<VehicleDto> {
    return this.dynoService.importDyno(user.id, body);
  }

  @Put('dynos/:id/data')
  @UseGuards(CsrfGuard)
  @ApiOperation({ summary: 'Replace the provided measurement series of an own dyno' })
  saveData(
    @CurrentUser() user: AuthenticatedUser,
    @Param('id', new ZodValidationPipe(IdParamSchema)) id: string,
    @Body(new ZodValidationPipe(DynoDataInputSchema)) body: DynoDataInputDto,
  ): Promise<DynoDto> {
    return this.dynoService.saveData(user.id, id, body);
  }

  @Delete('dynos/:id')
  @UseGuards(CsrfGuard)
  @ApiOperation({ summary: 'Delete an own dyno and its series' })
  remove(
    @CurrentUser() user: AuthenticatedUser,
    @Param('id', new ZodValidationPipe(IdParamSchema)) id: string,
  ): Promise<null> {
    return this.dynoService.remove(user.id, id);
  }

  @Get('dynos/:id/export')
  @ApiOperation({ summary: 'Export an own dyno with its vehicle as portable JSON' })
  exportDyno(
    @CurrentUser() user: AuthenticatedUser,
    @Param('id', new ZodValidationPipe(IdParamSchema)) id: string,
  ): Promise<DynoExportDto> {
    return this.dynoService.exportDyno(user.id, id);
  }
}
