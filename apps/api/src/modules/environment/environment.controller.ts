import { Body, Controller, Delete, Get, Param, Patch, Post, UseGuards } from '@nestjs/common';
import { ApiOperation, ApiTags } from '@nestjs/swagger';
import {
  EnvironmentInputSchema,
  type EnvironmentDto,
  type EnvironmentInputDto,
} from '@sim-tuning/common/schemas';
import { AuthGuard } from '@/guards/auth.guard';
import { CsrfGuard } from '@/guards/csrf.guard';
import { CurrentUser, type AuthenticatedUser } from '@/decorators/current-user.decorator';
import { ZodValidationPipe } from '@/pipes/zod-validation.pipe';
import {
  EnvironmentUpdateSchema,
  IdParamSchema,
  type EnvironmentUpdateDto,
} from '../sim/sim.fields';
import { EnvironmentService } from './environment.service';

@ApiTags('Environments')
@Controller('environments')
@UseGuards(AuthGuard)
export class EnvironmentController {
  constructor(private readonly environmentService: EnvironmentService) {}

  @Get()
  @ApiOperation({ summary: 'List starter environments and own environments' })
  list(@CurrentUser() user: AuthenticatedUser): Promise<EnvironmentDto[]> {
    return this.environmentService.list(user.id);
  }

  @Post()
  @UseGuards(CsrfGuard)
  @ApiOperation({ summary: 'Create an environment (temperature, air pressure)' })
  create(
    @CurrentUser() user: AuthenticatedUser,
    @Body(new ZodValidationPipe(EnvironmentInputSchema)) body: EnvironmentInputDto,
  ): Promise<EnvironmentDto> {
    return this.environmentService.create(user.id, body);
  }

  @Patch(':id')
  @UseGuards(CsrfGuard)
  @ApiOperation({ summary: 'Partially update an own environment' })
  update(
    @CurrentUser() user: AuthenticatedUser,
    @Param('id', new ZodValidationPipe(IdParamSchema)) id: string,
    @Body(new ZodValidationPipe(EnvironmentUpdateSchema)) body: EnvironmentUpdateDto,
  ): Promise<EnvironmentDto> {
    return this.environmentService.update(user.id, id, body);
  }

  @Delete(':id')
  @UseGuards(CsrfGuard)
  @ApiOperation({ summary: 'Delete an own environment (dynos keep running without it)' })
  remove(
    @CurrentUser() user: AuthenticatedUser,
    @Param('id', new ZodValidationPipe(IdParamSchema)) id: string,
  ): Promise<null> {
    return this.environmentService.remove(user.id, id);
  }
}
