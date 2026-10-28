import {
  STARTER_ENVIRONMENTS,
  STARTER_VEHICLES,
  type StarterVehicle,
} from '@sim-tuning/common/catalog';
import type {
  AuslassDto,
  AuspuffDto,
  DynoDataInputDto,
  DynoDto,
  DynoExportDto,
  DynoInputDto,
  EinlassDto,
  EnvironmentDto,
  EnvironmentInputDto,
  GpsSampleDto,
  MotorDto,
  UeberstroemerDto,
  VehicleDto,
  VehicleInputDto,
  VergaserDto,
} from '@sim-tuning/common/schemas';
import { DYNO_EXPORT_FORMAT } from '@sim-tuning/common/schemas';

/**
 * Pure builders/mergers that turn contract inputs into fully nested DTOs. Used by the local
 * (guest) repository and by tests; the API produces the same shapes server-side.
 */

export type IdFactory = (prefix: string) => string;

const nowIso = () => new Date().toISOString();

function meta(id: IdFactory, prefix: string, at = nowIso()) {
  return { id: id(prefix), createdAt: at, updatedAt: at };
}

/** Copies the listed keys from `input` (undefined → keep `base`, otherwise value or null). */
function pick<T extends object>(base: T, input: object | undefined, keys: readonly (keyof T)[]): T {
  if (!input) return base;
  const next = { ...base };
  for (const key of keys) {
    const value = (input as Record<string, unknown>)[key as string];
    if (value !== undefined) (next as Record<string, unknown>)[key as string] = value ?? null;
  }
  return next;
}

const VERGASER_KEYS = ['benzinLuftF', 'durchmesserD'] as const;
const EINLASS_KEYS = [
  'breiteB',
  'durchmesserD',
  'flaecheA',
  'hoeheH',
  'laengeL',
  'luftBedarf',
  'steuerzeitSZ',
] as const;
const AUSPUFF_KEYS = [
  'abgasT',
  'abgasV',
  'diffusorD',
  'diffusorD1',
  'diffusorD2',
  'diffusorD3',
  'diffusorL',
  'diffusorL1',
  'diffusorL2',
  'diffusorL3',
  'diffusorStage',
  'diffusorW',
  'diffusorW1',
  'diffusorW2',
  'diffusorW3',
  'endrohrD',
  'endrohrL',
  'gegenkonusD',
  'gegenkonusL',
  'gegenKonusW',
  'gesamtL',
  'kruemmerD',
  'kruemmerF',
  'kruemmerL',
  'kruemmerW',
  'mittelteilD',
  'mittelteilF',
  'mittelteilL',
  'resonanzL',
] as const;
const AUSLASS_KEYS = [
  'breiteB',
  'durchmesserD',
  'flaecheA',
  'hoeheH',
  'laengeL',
  'steuerzeitSZ',
] as const;
const UEBERSTROEMER_KEYS = ['anzahl', 'breiteB', 'flaecheA', 'hoeheH', 'steuerzeitSZ'] as const;
const MOTOR_KEYS = [
  'name',
  'bohrungD',
  'brennraumV',
  'deachsierungL',
  'heizwertU',
  'hubL',
  'hubraumV',
  'kolbenG',
  'kurbelgehaeuseV',
  'pleulL',
  'resonanzU',
  'verdichtungV',
  'zuendzeitpunkt',
  'zylinderAnz',
] as const;

const nulls = <K extends string>(keys: readonly K[]) =>
  Object.fromEntries(keys.map((k) => [k, null])) as Record<K, null>;

export function emptyMotor(id: IdFactory, name = 'Motor'): MotorDto {
  const vergaser: VergaserDto = { ...meta(id, 'verg'), ...nulls(VERGASER_KEYS) };
  const einlass: EinlassDto = { ...meta(id, 'einl'), ...nulls(EINLASS_KEYS), vergaser };
  const auspuff: AuspuffDto = {
    ...meta(id, 'ausp'),
    ...nulls(AUSPUFF_KEYS.filter((k) => k !== 'diffusorStage')),
    diffusorStage: 0,
  } as AuspuffDto;
  const auslass: AuslassDto = { ...meta(id, 'ausl'), ...nulls(AUSLASS_KEYS), auspuff };
  const ueberstroemer: UeberstroemerDto = { ...meta(id, 'ueb'), ...nulls(UEBERSTROEMER_KEYS) };
  return {
    ...meta(id, 'mot'),
    ...nulls(MOTOR_KEYS),
    name,
    einlass,
    auslass,
    ueberstroemer,
  } as MotorDto;
}

/** Applies a (partial) motor input onto a motor DTO, creating missing nested parts. */
export function mergeMotor(
  base: MotorDto,
  input: Partial<NonNullable<VehicleInputDto['motor']>> | undefined,
  id: IdFactory,
): MotorDto {
  if (!input) return base;
  const at = nowIso();
  const empty = emptyMotor(id, base.name);
  const einlassBase = base.einlass ?? empty.einlass!;
  const auslassBase = base.auslass ?? empty.auslass!;
  const einlass: EinlassDto = input.einlass
    ? {
        ...pick(einlassBase, input.einlass, EINLASS_KEYS),
        vergaser: input.einlass.vergaser
          ? {
              ...pick(
                einlassBase.vergaser ?? empty.einlass!.vergaser!,
                input.einlass.vergaser,
                VERGASER_KEYS,
              ),
              updatedAt: at,
            }
          : einlassBase.vergaser,
        updatedAt: at,
      }
    : einlassBase;
  const auslass: AuslassDto = input.auslass
    ? {
        ...pick(auslassBase, input.auslass, AUSLASS_KEYS),
        auspuff: input.auslass.auspuff
          ? {
              ...pick(
                auslassBase.auspuff ?? empty.auslass!.auspuff!,
                input.auslass.auspuff,
                AUSPUFF_KEYS,
              ),
              updatedAt: at,
            }
          : auslassBase.auspuff,
        updatedAt: at,
      }
    : auslassBase;
  const ueberstroemerBase = base.ueberstroemer ?? empty.ueberstroemer!;
  const ueberstroemer = input.ueberstroemer
    ? { ...pick(ueberstroemerBase, input.ueberstroemer, UEBERSTROEMER_KEYS), updatedAt: at }
    : ueberstroemerBase;
  return {
    ...pick(base, input, MOTOR_KEYS),
    einlass,
    auslass,
    ueberstroemer,
    updatedAt: at,
  };
}

export function buildVehicle(
  input: VehicleInputDto,
  id: IdFactory,
  extra: { systemKey?: string | null; dynoId?: string | null } = {},
): VehicleDto {
  const base = meta(id, 'veh');
  const motor = mergeMotor(emptyMotor(id, input.motor?.name ?? input.name), input.motor, id);
  return {
    ...base,
    systemKey: extra.systemKey ?? null,
    isSystem: Boolean(extra.systemKey),
    name: input.name,
    beschreibung: input.beschreibung ?? null,
    gewicht: input.gewicht ?? null,
    motor,
    dynoId: extra.dynoId ?? null,
  };
}

export function mergeVehicle(
  base: VehicleDto,
  input: Partial<VehicleInputDto>,
  id: IdFactory,
): VehicleDto {
  return {
    ...pick(base, input, ['name', 'beschreibung', 'gewicht'] as const),
    motor: mergeMotor(base.motor ?? emptyMotor(id, base.name), input.motor, id),
    updatedAt: nowIso(),
  };
}

/** Converts a vehicle DTO back into an input payload (for duplicate/export). */
export function vehicleToInput(vehicle: VehicleDto): VehicleInputDto {
  // Field lists above mirror the Zod input schemas, so the stripped records are valid inputs.
  const strip = <T extends object>(
    dto: T | null | undefined,
    keys: readonly string[],
  ): Record<string, unknown> | undefined =>
    dto ? Object.fromEntries(keys.map((k) => [k, (dto as Record<string, unknown>)[k]])) : undefined;
  const motor = vehicle.motor;
  return {
    name: vehicle.name,
    beschreibung: vehicle.beschreibung,
    gewicht: vehicle.gewicht,
    motor: (motor
      ? {
          ...strip(motor, MOTOR_KEYS),
          name: motor.name,
          einlass: motor.einlass
            ? {
                ...strip(motor.einlass, EINLASS_KEYS),
                vergaser: strip(motor.einlass.vergaser, VERGASER_KEYS),
              }
            : undefined,
          auslass: motor.auslass
            ? {
                ...strip(motor.auslass, AUSLASS_KEYS),
                auspuff: strip(motor.auslass.auspuff, AUSPUFF_KEYS),
              }
            : undefined,
          ueberstroemer: strip(motor.ueberstroemer, UEBERSTROEMER_KEYS),
        }
      : undefined) as VehicleInputDto['motor'],
  };
}

export function starterVehicleToInput(starter: StarterVehicle): VehicleInputDto {
  const { motor } = starter;
  return {
    name: starter.name,
    beschreibung: starter.beschreibung,
    gewicht: starter.gewicht,
    motor: {
      name: motor.name,
      hubL: motor.hubL,
      bohrungD: motor.bohrungD,
      pleulL: motor.pleulL,
      deachsierungL: motor.deachsierungL,
      resonanzU: motor.resonanzU,
      hubraumV: motor.hubraumV,
      brennraumV: motor.brennraumV,
      kurbelgehaeuseV: motor.kurbelgehaeuseV,
      einlass: { steuerzeitSZ: motor.einlassSteuerzeitSZ },
      auslass: { steuerzeitSZ: motor.auslassSteuerzeitSZ },
      ueberstroemer: { steuerzeitSZ: motor.ueberstroemerSteuerzeitSZ },
    },
  };
}

/** Stable ids for starter content so guest data survives reloads. */
export function starterVehicles(): VehicleDto[] {
  return STARTER_VEHICLES.map((starter) => {
    let n = 0;
    const id: IdFactory = (prefix) => `sys-${starter.systemKey}-${prefix}-${n++}`;
    const vehicle = buildVehicle(starterVehicleToInput(starter), id, {
      systemKey: starter.systemKey,
    });
    return { ...vehicle, id: `sys-${starter.systemKey}` };
  });
}

export function starterEnvironments(): EnvironmentDto[] {
  const at = '2026-01-01T00:00:00.000Z';
  return STARTER_ENVIRONMENTS.map((env) => ({
    id: `sys-${env.systemKey}`,
    createdAt: at,
    updatedAt: at,
    systemKey: env.systemKey,
    isSystem: true,
    name: env.name,
    temperaturT: env.temperaturT,
    luftdruckP: env.luftdruckP,
  }));
}

export function buildEnvironment(input: EnvironmentInputDto, id: IdFactory): EnvironmentDto {
  return {
    ...meta(id, 'env'),
    systemKey: null,
    isSystem: false,
    name: input.name,
    temperaturT: input.temperaturT ?? null,
    luftdruckP: input.luftdruckP ?? null,
  };
}

const gps = (samples: GpsSampleDto[]) =>
  samples.map((s) => ({
    latitude: s.latitude,
    longitude: s.longitude,
    speed: s.speed ?? null,
    accuracy: s.accuracy ?? null,
    altitude: s.altitude ?? null,
    altitudeAccuracy: s.altitudeAccuracy ?? null,
    heading: s.heading ?? null,
    headingAccuracy: s.headingAccuracy ?? null,
    zeitpunkt: s.zeitpunkt ?? null,
  }));

export function upsertDyno(
  existing: DynoDto | null,
  vehicleId: string,
  input: DynoInputDto,
  id: IdFactory,
): DynoDto {
  const base: DynoDto = existing ?? {
    ...meta(id, 'dyno'),
    vehicleId,
    environmentId: null,
    name: input.name,
    beschreibung: null,
    active: true,
    drehzahl: [],
    dynoPs: [],
    ausrollen: [],
    geschwindigkeit: [],
  };
  return {
    ...base,
    name: input.name,
    beschreibung:
      input.beschreibung !== undefined ? (input.beschreibung ?? null) : base.beschreibung,
    active: input.active ?? base.active,
    environmentId:
      input.environmentId !== undefined ? (input.environmentId ?? null) : base.environmentId,
    updatedAt: nowIso(),
  };
}

export function applyDynoData(dyno: DynoDto, data: DynoDataInputDto): DynoDto {
  return {
    ...dyno,
    drehzahl: data.drehzahl ? data.drehzahl.map((p) => ({ ...p })) : dyno.drehzahl,
    dynoPs: data.dynoPs ? data.dynoPs.map((p) => ({ ...p })) : dyno.dynoPs,
    ausrollen: data.ausrollen ? gps(data.ausrollen) : dyno.ausrollen,
    geschwindigkeit: data.geschwindigkeit ? gps(data.geschwindigkeit) : dyno.geschwindigkeit,
    updatedAt: nowIso(),
  };
}

const definedGps = (samples: DynoDto['ausrollen']): GpsSampleDto[] =>
  samples.map(
    (s) => Object.fromEntries(Object.entries(s).filter(([, v]) => v !== null)) as GpsSampleDto,
  );

export function buildDynoExport(vehicle: VehicleDto, dyno: DynoDto): DynoExportDto {
  return {
    format: DYNO_EXPORT_FORMAT,
    version: 3,
    exportedAt: nowIso(),
    vehicle: vehicleToInput(vehicle),
    dyno: {
      name: dyno.name,
      beschreibung: dyno.beschreibung,
      active: dyno.active,
      drehzahl: dyno.drehzahl,
      dynoPs: dyno.dynoPs,
      ausrollen: definedGps(dyno.ausrollen),
      geschwindigkeit: definedGps(dyno.geschwindigkeit),
    },
  };
}
