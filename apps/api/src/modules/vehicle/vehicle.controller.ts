import { Body, Controller, Delete, Get, Param, Patch, Post, UseGuards } from '@nestjs/common';
import { ApiOperation, ApiTags } from '@nestjs/swagger';
import {
  VehicleInputSchema,
  type VehicleDto,
  type VehicleInputDto,
} from '@sim-tuning/common/schemas';
import { AuthGuard } from '@/guards/auth.guard';
import { CsrfGuard } from '@/guards/csrf.guard';
import { CurrentUser, type AuthenticatedUser } from '@/decorators/current-user.decorator';
import { ZodValidationPipe } from '@/pipes/zod-validation.pipe';
import {
  DuplicateVehicleSchema,
  IdParamSchema,
  VehicleUpdateSchema,
  type DuplicateVehicleDto,
  type VehicleUpdateDto,
} from '../sim/sim.fields';
import { VehicleService } from './vehicle.service';

@ApiTags('Vehicles')
@Controller('vehicles')
@UseGuards(AuthGuard)
export class VehicleController {
  constructor(private readonly vehicleService: VehicleService) {}

  @Get()
  @ApiOperation({ summary: 'List starter vehicles and own vehicles' })
  list(@CurrentUser() user: AuthenticatedUser): Promise<VehicleDto[]> {
    return this.vehicleService.list(user.id);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get a vehicle with its fully nested motor' })
  get(
    @CurrentUser() user: AuthenticatedUser,
    @Param('id', new ZodValidationPipe(IdParamSchema)) id: string,
  ): Promise<VehicleDto> {
    return this.vehicleService.get(user.id, id);
  }

  @Post()
  @UseGuards(CsrfGuard)
  @ApiOperation({ summary: 'Create a vehicle (missing motor parts are created empty)' })
  create(
    @CurrentUser() user: AuthenticatedUser,
    @Body(new ZodValidationPipe(VehicleInputSchema)) body: VehicleInputDto,
  ): Promise<VehicleDto> {
    return this.vehicleService.create(user.id, body);
  }

  @Patch(':id')
  @UseGuards(CsrfGuard)
  @ApiOperation({ summary: 'Partially update an own vehicle (nested fields allowed)' })
  update(
    @CurrentUser() user: AuthenticatedUser,
    @Param('id', new ZodValidationPipe(IdParamSchema)) id: string,
    @Body(new ZodValidationPipe(VehicleUpdateSchema)) body: VehicleUpdateDto,
  ): Promise<VehicleDto> {
    return this.vehicleService.update(user.id, id, body);
  }

  @Delete(':id')
  @UseGuards(CsrfGuard)
  @ApiOperation({ summary: 'Delete an own vehicle including motor parts and dyno' })
  remove(
    @CurrentUser() user: AuthenticatedUser,
    @Param('id', new ZodValidationPipe(IdParamSchema)) id: string,
  ): Promise<null> {
    return this.vehicleService.remove(user.id, id);
  }

  @Post(':id/duplicate')
  @UseGuards(CsrfGuard)
  @ApiOperation({ summary: 'Deep-copy a vehicle (own or starter) into the caller account' })
  duplicate(
    @CurrentUser() user: AuthenticatedUser,
    @Param('id', new ZodValidationPipe(IdParamSchema)) id: string,
    @Body(new ZodValidationPipe(DuplicateVehicleSchema)) body: DuplicateVehicleDto,
  ): Promise<VehicleDto> {
    return this.vehicleService.duplicate(user.id, id, body.name);
  }
}
