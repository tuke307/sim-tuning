import type {
  AuslassDto,
  DynoDto,
  DynoExportDto,
  EinlassDto,
  EnvironmentDto,
  GpsSampleDto,
  GpsSampleResponseDto,
  MotorDto,
  VehicleDto,
  VehicleInputDto,
} from '@sim-tuning/common/schemas';
import { DYNO_EXPORT_FORMAT } from '@sim-tuning/common/schemas';
import type { Environment } from '@sim-tuning/database';
import {
  AUSLASS_KEYS,
  AUSPUFF_KEYS,
  EINLASS_KEYS,
  GPS_KEYS,
  MOTOR_KEYS,
  UEBERSTROEMER_KEYS,
  VERGASER_KEYS,
  isSystemRow,
  type DynoRow,
  type MotorRow,
  type VehicleRow,
} from './sim.fields';

/** Prisma row → response DTO mapping (dates become ISO strings). */

const byName = (a: { name: string }, b: { name: string }) => a.name.localeCompare(b.name);

/** Shared starter rows first, then own rows, each by name. */
export const sortSystemFirst = <T extends { isSystem: boolean; name: string }>(rows: T[]) =>
  [...rows].sort((a, b) => Number(b.isSystem) - Number(a.isSystem) || byName(a, b));

function pickKeys<T extends object, K extends keyof T>(row: T, keys: readonly K[]): Pick<T, K> {
  const out = {} as Pick<T, K>;
  for (const key of keys) out[key] = row[key];
  return out;
}

const meta = (row: { id: string; createdAt: Date; updatedAt: Date }) => ({
  id: row.id,
  createdAt: row.createdAt.toISOString(),
  updatedAt: row.updatedAt.toISOString(),
});

export function toMotorDto(motor: MotorRow): MotorDto {
  const einlass: EinlassDto | null = motor.einlass
    ? {
        ...meta(motor.einlass),
        ...pickKeys(motor.einlass, EINLASS_KEYS),
        vergaser: motor.einlass.vergaser
          ? { ...meta(motor.einlass.vergaser), ...pickKeys(motor.einlass.vergaser, VERGASER_KEYS) }
          : null,
      }
    : null;
  const auslass: AuslassDto | null = motor.auslass
    ? {
        ...meta(motor.auslass),
        ...pickKeys(motor.auslass, AUSLASS_KEYS),
        auspuff: motor.auslass.auspuff
          ? { ...meta(motor.auslass.auspuff), ...pickKeys(motor.auslass.auspuff, AUSPUFF_KEYS) }
          : null,
      }
    : null;
  return {
    ...meta(motor),
    ...pickKeys(motor, MOTOR_KEYS),
    einlass,
    auslass,
    ueberstroemer: motor.ueberstroemer
      ? { ...meta(motor.ueberstroemer), ...pickKeys(motor.ueberstroemer, UEBERSTROEMER_KEYS) }
      : null,
  };
}

export function toVehicleDto(row: VehicleRow): VehicleDto {
  return {
    ...meta(row),
    systemKey: row.systemKey,
    isSystem: isSystemRow(row),
    name: row.name,
    beschreibung: row.beschreibung,
    gewicht: row.gewicht,
    motor: row.motor ? toMotorDto(row.motor) : null,
    dynoId: row.dyno?.id ?? null,
  };
}

export function toEnvironmentDto(row: Environment): EnvironmentDto {
  return {
    ...meta(row),
    systemKey: row.systemKey,
    isSystem: isSystemRow(row),
    name: row.name,
    temperaturT: row.temperaturT,
    luftdruckP: row.luftdruckP,
  };
}

type GpsRow = DynoRow['ausrollen'][number];

const toGpsDto = (row: GpsRow): GpsSampleResponseDto => ({
  ...pickKeys(row, GPS_KEYS),
  zeitpunkt: row.zeitpunkt ? row.zeitpunkt.toISOString() : null,
});

export function toDynoDto(row: DynoRow): DynoDto {
  return {
    ...meta(row),
    vehicleId: row.vehicleId,
    environmentId: row.environmentId,
    name: row.name,
    beschreibung: row.beschreibung,
    active: row.active,
    drehzahl: row.drehzahl.map((p) => ({ zeit: p.zeit, drehzahl: p.drehzahl })),
    dynoPs: row.dynoPs.map((p) => ({ drehzahl: p.drehzahl, ps: p.ps })),
    ausrollen: row.ausrollen.map(toGpsDto),
    geschwindigkeit: row.geschwindigkeit.map(toGpsDto),
  };
}

/** Vehicle row → input payload (used for duplicate and export). Ids/timestamps are dropped. */
export function vehicleRowToInput(row: VehicleRow): VehicleInputDto {
  const motor = row.motor;
  return {
    name: row.name,
    beschreibung: row.beschreibung,
    gewicht: row.gewicht,
    motor: motor
      ? {
          ...pickKeys(motor, MOTOR_KEYS),
          einlass: motor.einlass
            ? {
                ...pickKeys(motor.einlass, EINLASS_KEYS),
                vergaser: motor.einlass.vergaser
                  ? pickKeys(motor.einlass.vergaser, VERGASER_KEYS)
                  : undefined,
              }
            : undefined,
          auslass: motor.auslass
            ? {
                ...pickKeys(motor.auslass, AUSLASS_KEYS),
                auspuff: motor.auslass.auspuff
                  ? pickKeys(motor.auslass.auspuff, AUSPUFF_KEYS)
                  : undefined,
              }
            : undefined,
          ueberstroemer: motor.ueberstroemer
            ? pickKeys(motor.ueberstroemer, UEBERSTROEMER_KEYS)
            : undefined,
        }
      : undefined,
  };
}

/** GPS sample for the export file: null optionals are omitted (matches the guest export). */
const toGpsExport = (row: GpsRow): GpsSampleDto => {
  const sample: GpsSampleDto = { latitude: row.latitude, longitude: row.longitude };
  for (const key of GPS_KEYS) {
    const value = row[key];
    if (value !== null) sample[key] = value;
  }
  if (row.zeitpunkt) sample.zeitpunkt = row.zeitpunkt.toISOString();
  return sample;
};

export function toDynoExport(vehicle: VehicleRow, dyno: DynoRow, now = new Date()): DynoExportDto {
  return {
    format: DYNO_EXPORT_FORMAT,
    version: 3,
    exportedAt: now.toISOString(),
    vehicle: vehicleRowToInput(vehicle),
    dyno: {
      name: dyno.name,
      beschreibung: dyno.beschreibung,
      active: dyno.active,
      drehzahl: dyno.drehzahl.map((p) => ({ zeit: p.zeit, drehzahl: p.drehzahl })),
      dynoPs: dyno.dynoPs.map((p) => ({ drehzahl: p.drehzahl, ps: p.ps })),
      ausrollen: dyno.ausrollen.map(toGpsExport),
      geschwindigkeit: dyno.geschwindigkeit.map(toGpsExport),
    },
  };
}
