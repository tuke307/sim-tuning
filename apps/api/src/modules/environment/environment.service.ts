import { ForbiddenException, Injectable, NotFoundException } from '@nestjs/common';
import type { EnvironmentDto, EnvironmentInputDto } from '@sim-tuning/common/schemas';
import type { Environment } from '@sim-tuning/database';
import { PrismaService, type PrismaTx } from '../database/prisma.service';
import { isSystemRow, readableBy, type EnvironmentUpdateDto } from '../sim/sim.fields';
import { sortSystemFirst, toEnvironmentDto } from '../sim/sim.mapper';
import { assign } from '../sim/sim.writer';

export const READ_ONLY_ENVIRONMENT_MESSAGE = 'Starter environments are read-only';

const ENVIRONMENT_KEYS = ['name', 'temperaturT', 'luftdruckP'] as const;

@Injectable()
export class EnvironmentService {
  constructor(private readonly prisma: PrismaService) {}

  /** Own or starter environment; foreign rows are reported as missing. */
  async findReadable(
    userId: string,
    id: string,
    db: Pick<PrismaTx, 'environment'> = this.prisma,
  ): Promise<Environment> {
    const row = await db.environment.findFirst({ where: { id, ...readableBy(userId) } });
    if (!row) throw new NotFoundException('Environment not found');
    return row;
  }

  private async findWritable(
    userId: string,
    id: string,
    db: Pick<PrismaTx, 'environment'>,
  ): Promise<Environment> {
    const row = await this.findReadable(userId, id, db);
    if (isSystemRow(row) || row.userId !== userId) {
      throw new ForbiddenException(READ_ONLY_ENVIRONMENT_MESSAGE);
    }
    return row;
  }

  async list(userId: string): Promise<EnvironmentDto[]> {
    const rows = await this.prisma.environment.findMany({ where: readableBy(userId) });
    return sortSystemFirst(rows.map(toEnvironmentDto));
  }

  async create(userId: string, input: EnvironmentInputDto): Promise<EnvironmentDto> {
    const row = await this.prisma.environment.create({
      data: {
        userId,
        name: input.name,
        temperaturT: input.temperaturT ?? null,
        luftdruckP: input.luftdruckP ?? null,
      },
    });
    return toEnvironmentDto(row);
  }

  async update(userId: string, id: string, input: EnvironmentUpdateDto): Promise<EnvironmentDto> {
    const row = await this.prisma.$transaction(async (tx) => {
      await this.findWritable(userId, id, tx);
      return tx.environment.update({
        where: { id, userId },
        data: assign(input, ENVIRONMENT_KEYS),
      });
    });
    return toEnvironmentDto(row);
  }

  async remove(userId: string, id: string): Promise<null> {
    await this.prisma.$transaction(async (tx) => {
      await this.findWritable(userId, id, tx);
      // Dynos keep running without an environment (the FK also sets null on delete).
      await tx.dyno.updateMany({ where: { environmentId: id }, data: { environmentId: null } });
      await tx.environment.delete({ where: { id, userId } });
    });
    return null;
  }
}
