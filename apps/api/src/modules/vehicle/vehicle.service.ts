import { ForbiddenException, Injectable, NotFoundException } from '@nestjs/common';
import type { VehicleDto, VehicleInputDto } from '@sim-tuning/common/schemas';
import { PrismaService, type PrismaTx } from '../database/prisma.service';
import {
  MAX_NAME_LENGTH,
  VEHICLE_INCLUDE,
  isSystemRow,
  readableBy,
  type VehicleRow,
  type VehicleUpdateDto,
} from '../sim/sim.fields';
import { sortSystemFirst, toVehicleDto, vehicleRowToInput } from '../sim/sim.mapper';
import { assign, motorCreate, motorUpdate, vehicleCreate } from '../sim/sim.writer';

export const READ_ONLY_VEHICLE_MESSAGE = 'Starter vehicles are read-only';

@Injectable()
export class VehicleService {
  constructor(private readonly prisma: PrismaService) {}

  /** Own or shared starter vehicle; everything else is reported as missing (no id probing). */
  async findReadable(
    userId: string,
    id: string,
    db: Pick<PrismaTx, 'vehicle'> = this.prisma,
  ): Promise<VehicleRow> {
    const row = await db.vehicle.findFirst({
      where: { id, ...readableBy(userId) },
      include: VEHICLE_INCLUDE,
    });
    if (!row) throw new NotFoundException('Vehicle not found');
    return row;
  }

  /** Vehicle the user may modify: 404 for foreign rows, 403 for shared starter vehicles. */
  async findWritable(
    userId: string,
    id: string,
    db: Pick<PrismaTx, 'vehicle'> = this.prisma,
  ): Promise<VehicleRow> {
    const row = await this.findReadable(userId, id, db);
    if (isSystemRow(row) || row.userId !== userId) {
      throw new ForbiddenException(READ_ONLY_VEHICLE_MESSAGE);
    }
    return row;
  }

  async list(userId: string): Promise<VehicleDto[]> {
    const rows = await this.prisma.vehicle.findMany({
      where: readableBy(userId),
      include: VEHICLE_INCLUDE,
    });
    return sortSystemFirst(rows.map(toVehicleDto));
  }

  async get(userId: string, id: string): Promise<VehicleDto> {
    return toVehicleDto(await this.findReadable(userId, id));
  }

  async create(userId: string, input: VehicleInputDto): Promise<VehicleDto> {
    const row = await this.prisma.$transaction((tx) =>
      tx.vehicle.create({ data: vehicleCreate(userId, input), include: VEHICLE_INCLUDE }),
    );
    return toVehicleDto(row);
  }

  async update(userId: string, id: string, input: VehicleUpdateDto): Promise<VehicleDto> {
    const row = await this.prisma.$transaction(async (tx) => {
      const existing = await this.findWritable(userId, id, tx);
      const name = input.name ?? existing.name;
      const motor = existing.motor
        ? input.motor
          ? { update: motorUpdate(existing.motor, input.motor) }
          : undefined
        : { create: motorCreate(input.motor, input.motor?.name ?? name) };
      return tx.vehicle.update({
        // `userId` in the unique filter keeps the write scoped to the owner (defence in depth).
        where: { id, userId },
        data: { ...assign(input, ['name', 'beschreibung', 'gewicht'] as const), motor },
        include: VEHICLE_INCLUDE,
      });
    });
    return toVehicleDto(row);
  }

  async remove(userId: string, id: string): Promise<null> {
    await this.prisma.$transaction(async (tx) => {
      const existing = await this.findWritable(userId, id, tx);
      // Dyno + series cascade from the vehicle; the motor is referenced (SetNull), so it and
      // its parts (cascade from motor) are removed explicitly.
      await tx.vehicle.delete({ where: { id, userId } });
      if (existing.motorId) await tx.motor.delete({ where: { id: existing.motorId } });
    });
    return null;
  }

  async duplicate(userId: string, id: string, name?: string): Promise<VehicleDto> {
    const source = await this.findReadable(userId, id);
    const input = vehicleRowToInput(source);
    const copyName = name?.trim() || `${source.name} (Kopie)`.slice(0, MAX_NAME_LENGTH);
    const row = await this.prisma.$transaction((tx) =>
      tx.vehicle.create({
        data: vehicleCreate(userId, { ...input, name: copyName }),
        include: VEHICLE_INCLUDE,
      }),
    );
    return toVehicleDto(row);
  }
}
