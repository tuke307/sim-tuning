import type {
  AuslassInputDto,
  DynoDataInputDto,
  EinlassInputDto,
  GpsSampleDto,
  MotorInputDto,
  UeberstroemerInputDto,
  VehicleInputDto,
} from '@sim-tuning/common/schemas';
import type { Prisma } from '@sim-tuning/database';
import {
  AUSLASS_KEYS,
  AUSPUFF_KEYS,
  EINLASS_KEYS,
  MOTOR_KEYS,
  UEBERSTROEMER_KEYS,
  VERGASER_KEYS,
  type MotorRow,
} from './sim.fields';

/**
 * Contract input → Prisma write data. Semantics mirror the guest model
 * (`apps/web/lib/data/model.ts`): `undefined` keeps the stored value, `null` clears it, and
 * missing nested motor parts are created (empty unless values are provided).
 */

type Assigned<T, K extends keyof T> = { [P in K]?: Exclude<T[P], undefined> };

/** Copies only the provided keys (`undefined` → omitted, `null` → null). */
export function assign<T extends object, K extends keyof T>(
  input: T | undefined,
  keys: readonly K[],
): Assigned<T, K> {
  const out: Assigned<T, K> = {};
  if (!input) return out;
  for (const key of keys) {
    const value = input[key];
    if (value !== undefined) out[key] = value as Exclude<T[K], undefined>;
  }
  return out;
}

type PartialMotorInput = Partial<MotorInputDto>;

function einlassCreate(input?: EinlassInputDto): Prisma.EinlassCreateWithoutMotorInput {
  return {
    ...assign(input, EINLASS_KEYS),
    vergaser: { create: assign(input?.vergaser, VERGASER_KEYS) },
  };
}

function auslassCreate(input?: AuslassInputDto): Prisma.AuslassCreateWithoutMotorInput {
  return {
    ...assign(input, AUSLASS_KEYS),
    auspuff: { create: assign(input?.auspuff, AUSPUFF_KEYS) },
  };
}

function ueberstroemerCreate(
  input?: UeberstroemerInputDto,
): Prisma.UeberstroemerCreateWithoutMotorInput {
  return assign(input, UEBERSTROEMER_KEYS);
}

/** Full motor create (all nested parts, missing ones empty). */
export function motorCreate(
  input: PartialMotorInput | undefined,
  fallbackName: string,
): Prisma.MotorCreateWithoutVehicleInput {
  return {
    ...assign(input, MOTOR_KEYS),
    name: input?.name ?? fallbackName,
    einlass: { create: einlassCreate(input?.einlass) },
    auslass: { create: auslassCreate(input?.auslass) },
    ueberstroemer: { create: ueberstroemerCreate(input?.ueberstroemer) },
  };
}

export function vehicleCreate(
  userId: string,
  input: VehicleInputDto,
  dyno?: Prisma.DynoCreateWithoutVehicleInput,
): Prisma.VehicleCreateInput {
  return {
    user: { connect: { id: userId } },
    name: input.name,
    beschreibung: input.beschreibung ?? null,
    gewicht: input.gewicht ?? null,
    motor: { create: motorCreate(input.motor, input.motor?.name ?? input.name) },
    ...(dyno ? { dyno: { create: dyno } } : {}),
  };
}

function einlassWrite(
  existing: MotorRow['einlass'],
  input: EinlassInputDto | undefined,
): Prisma.EinlassUpdateOneWithoutMotorNestedInput | undefined {
  if (!existing) return { create: einlassCreate(input) };
  const vergaser = existing.vergaser
    ? input?.vergaser
      ? { update: assign(input.vergaser, VERGASER_KEYS) }
      : undefined
    : { create: assign(input?.vergaser, VERGASER_KEYS) };
  if (!input && !vergaser) return undefined;
  return { update: { ...assign(input, EINLASS_KEYS), ...(vergaser ? { vergaser } : {}) } };
}

function auslassWrite(
  existing: MotorRow['auslass'],
  input: AuslassInputDto | undefined,
): Prisma.AuslassUpdateOneWithoutMotorNestedInput | undefined {
  if (!existing) return { create: auslassCreate(input) };
  const auspuff = existing.auspuff
    ? input?.auspuff
      ? { update: assign(input.auspuff, AUSPUFF_KEYS) }
      : undefined
    : { create: assign(input?.auspuff, AUSPUFF_KEYS) };
  if (!input && !auspuff) return undefined;
  return { update: { ...assign(input, AUSLASS_KEYS), ...(auspuff ? { auspuff } : {}) } };
}

function ueberstroemerWrite(
  existing: MotorRow['ueberstroemer'],
  input: UeberstroemerInputDto | undefined,
): Prisma.UeberstroemerUpdateOneWithoutMotorNestedInput | undefined {
  if (!existing) return { create: ueberstroemerCreate(input) };
  return input ? { update: assign(input, UEBERSTROEMER_KEYS) } : undefined;
}

/** Partial nested motor update; parts that do not exist yet are created. */
export function motorUpdate(
  existing: MotorRow,
  input: PartialMotorInput,
): Prisma.MotorUpdateWithoutVehicleInput {
  const data: Prisma.MotorUpdateWithoutVehicleInput = { ...assign(input, MOTOR_KEYS) };
  const einlass = einlassWrite(existing.einlass, input.einlass);
  const auslass = auslassWrite(existing.auslass, input.auslass);
  const ueberstroemer = ueberstroemerWrite(existing.ueberstroemer, input.ueberstroemer);
  if (einlass) data.einlass = einlass;
  if (auslass) data.auslass = auslass;
  if (ueberstroemer) data.ueberstroemer = ueberstroemer;
  return data;
}

// ─────────────────────────────────────────────────────────────────────────────
// Dyno series
// ─────────────────────────────────────────────────────────────────────────────
export const gpsRow = (sample: GpsSampleDto, position: number) => ({
  position,
  latitude: sample.latitude,
  longitude: sample.longitude,
  speed: sample.speed ?? null,
  accuracy: sample.accuracy ?? null,
  altitude: sample.altitude ?? null,
  altitudeAccuracy: sample.altitudeAccuracy ?? null,
  heading: sample.heading ?? null,
  headingAccuracy: sample.headingAccuracy ?? null,
  zeitpunkt: sample.zeitpunkt ? new Date(sample.zeitpunkt) : null,
});

/** Series rows (without `dynoId`) in input order, `position` = array index. */
export function seriesRows(data: DynoDataInputDto) {
  return {
    drehzahl: data.drehzahl?.map((p, position) => ({
      position,
      zeit: p.zeit,
      drehzahl: p.drehzahl,
    })),
    dynoPs: data.dynoPs?.map((p, position) => ({ position, drehzahl: p.drehzahl, ps: p.ps })),
    ausrollen: data.ausrollen?.map(gpsRow),
    geschwindigkeit: data.geschwindigkeit?.map(gpsRow),
  };
}
