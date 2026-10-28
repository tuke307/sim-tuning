import { Injectable, NotFoundException } from '@nestjs/common';
import type {
  DynoDataInputDto,
  DynoDto,
  DynoExportDto,
  DynoInputDto,
  VehicleDto,
} from '@sim-tuning/common/schemas';
import type { Prisma } from '@sim-tuning/database';
import { PrismaService, type PrismaTx } from '../database/prisma.service';
import { EnvironmentService } from '../environment/environment.service';
import { DYNO_INCLUDE, VEHICLE_INCLUDE, type DynoRow } from '../sim/sim.fields';
import { toDynoDto, toDynoExport, toVehicleDto } from '../sim/sim.mapper';
import { assign, seriesRows, vehicleCreate } from '../sim/sim.writer';
import { VehicleService } from '../vehicle/vehicle.service';

/** Series writes can carry up to 4 × MAX_DYNO_POINTS rows; allow more than the 5 s default. */
const SERIES_TX_OPTIONS = { timeout: 30_000, maxWait: 10_000 };

@Injectable()
export class DynoService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly vehicleService: VehicleService,
    private readonly environmentService: EnvironmentService,
  ) {}

  /** Dyno whose vehicle belongs to the user; foreign (and starter) dynos are reported missing. */
  private async findOwned(userId: string, id: string, db: Pick<PrismaTx, 'dyno'>) {
    const row = await db.dyno.findFirst({
      where: { id, vehicle: { userId } },
      include: DYNO_INCLUDE,
    });
    if (!row) throw new NotFoundException('Dyno not found');
    return row;
  }

  async getForVehicle(userId: string, vehicleId: string): Promise<DynoDto | null> {
    const vehicle = await this.vehicleService.findReadable(userId, vehicleId);
    const row = await this.prisma.dyno.findUnique({
      where: { vehicleId: vehicle.id },
      include: DYNO_INCLUDE,
    });
    return row ? toDynoDto(row) : null;
  }

  async save(userId: string, vehicleId: string, input: DynoInputDto): Promise<DynoDto> {
    const row = await this.prisma.$transaction(async (tx) => {
      // Starter vehicles cannot get a dyno (403); foreign vehicles are missing (404).
      const vehicle = await this.vehicleService.findWritable(userId, vehicleId, tx);
      if (input.environmentId) {
        await this.environmentService.findReadable(userId, input.environmentId, tx);
      }
      return tx.dyno.upsert({
        where: { vehicleId: vehicle.id },
        create: {
          vehicleId: vehicle.id,
          name: input.name,
          beschreibung: input.beschreibung ?? null,
          active: input.active ?? true,
          environmentId: input.environmentId ?? null,
        },
        update: {
          name: input.name,
          ...assign(input, ['beschreibung', 'active', 'environmentId'] as const),
        },
        include: DYNO_INCLUDE,
      });
    });
    return toDynoDto(row);
  }

  async saveData(userId: string, id: string, data: DynoDataInputDto): Promise<DynoDto> {
    const row = await this.prisma.$transaction(async (tx): Promise<DynoRow> => {
      await this.findOwned(userId, id, tx);
      const rows = seriesRows(data);
      const withDyno = <T extends object>(items: T[]) => items.map((r) => ({ ...r, dynoId: id }));
      // Each provided series replaces the stored one in order; omitted series stay untouched.
      if (rows.drehzahl) {
        await tx.drehzahl.deleteMany({ where: { dynoId: id } });
        if (rows.drehzahl.length) {
          await tx.drehzahl.createMany({ data: withDyno(rows.drehzahl) });
        }
      }
      if (rows.dynoPs) {
        await tx.dynoPs.deleteMany({ where: { dynoId: id } });
        if (rows.dynoPs.length) await tx.dynoPs.createMany({ data: withDyno(rows.dynoPs) });
      }
      if (rows.ausrollen) {
        await tx.ausrollen.deleteMany({ where: { dynoId: id } });
        if (rows.ausrollen.length) {
          await tx.ausrollen.createMany({ data: withDyno(rows.ausrollen) });
        }
      }
      if (rows.geschwindigkeit) {
        await tx.geschwindigkeit.deleteMany({ where: { dynoId: id } });
        if (rows.geschwindigkeit.length) {
          await tx.geschwindigkeit.createMany({ data: withDyno(rows.geschwindigkeit) });
        }
      }
      return tx.dyno.update({
        where: { id },
        data: { updatedAt: new Date() },
        include: DYNO_INCLUDE,
      });
    }, SERIES_TX_OPTIONS);
    return toDynoDto(row);
  }

  async remove(userId: string, id: string): Promise<null> {
    await this.prisma.$transaction(async (tx) => {
      await this.findOwned(userId, id, tx);
      await tx.dyno.delete({ where: { id } });
    });
    return null;
  }

  async exportDyno(userId: string, id: string): Promise<DynoExportDto> {
    const dyno = await this.findOwned(userId, id, this.prisma);
    const vehicle = await this.vehicleService.findReadable(userId, dyno.vehicleId);
    return toDynoExport(vehicle, dyno);
  }

  /** Creates a new vehicle owned by the caller together with the imported dyno + series. */
  async importDyno(userId: string, data: DynoExportDto): Promise<VehicleDto> {
    const { drehzahl, dynoPs, ausrollen, geschwindigkeit, ...dynoInput } = data.dyno;
    const rows = seriesRows({ drehzahl, dynoPs, ausrollen, geschwindigkeit });
    const many = <T>(items: T[] | undefined) =>
      items?.length ? { createMany: { data: items } } : undefined;
    const dyno: Prisma.DynoCreateWithoutVehicleInput = {
      name: dynoInput.name,
      beschreibung: dynoInput.beschreibung ?? null,
      active: dynoInput.active ?? true,
      drehzahl: many(rows.drehzahl),
      dynoPs: many(rows.dynoPs),
      ausrollen: many(rows.ausrollen),
      geschwindigkeit: many(rows.geschwindigkeit),
    };
    const row = await this.prisma.$transaction(
      (tx) =>
        tx.vehicle.create({
          data: vehicleCreate(userId, data.vehicle, dyno),
          include: VEHICLE_INCLUDE,
        }),
      SERIES_TX_OPTIONS,
    );
    return toVehicleDto(row);
  }
}
