import { z } from 'zod';

/**
 * SimTuning domain contract (v2 entities, German names kept). Every numeric field is stored in
 * its base unit (`FIELD_BASE_UNITS`); the UI converts for display only.
 */

const MAX_ABS = 1e12;
const num = () => z.number().finite().min(-MAX_ABS).max(MAX_ABS).nullable().optional();
const name = () => z.string().trim().min(1, 'Name is required').max(120);
const text = () => z.string().trim().max(2000).nullable().optional();

// ─────────────────────────────────────────────────────────────────────────────
// Inputs (create / update payloads)
// ─────────────────────────────────────────────────────────────────────────────
export const VergaserInputSchema = z.object({
  benzinLuftF: num(),
  durchmesserD: num(),
});

export const EinlassInputSchema = z.object({
  breiteB: num(),
  durchmesserD: num(),
  flaecheA: num(),
  hoeheH: num(),
  laengeL: num(),
  luftBedarf: num(),
  steuerzeitSZ: num(),
  vergaser: VergaserInputSchema.optional(),
});

export const AuspuffInputSchema = z.object({
  abgasT: num(),
  abgasV: num(),
  diffusorD: num(),
  diffusorD1: num(),
  diffusorD2: num(),
  diffusorD3: num(),
  diffusorL: num(),
  diffusorL1: num(),
  diffusorL2: num(),
  diffusorL3: num(),
  diffusorStage: z.number().int().min(0).max(3).optional(),
  diffusorW: num(),
  diffusorW1: num(),
  diffusorW2: num(),
  diffusorW3: num(),
  endrohrD: num(),
  endrohrL: num(),
  gegenkonusD: num(),
  gegenkonusL: num(),
  gegenKonusW: num(),
  gesamtL: num(),
  kruemmerD: num(),
  kruemmerF: num(),
  kruemmerL: num(),
  kruemmerW: num(),
  mittelteilD: num(),
  mittelteilF: num(),
  mittelteilL: num(),
  resonanzL: num(),
});

export const AuslassInputSchema = z.object({
  breiteB: num(),
  durchmesserD: num(),
  flaecheA: num(),
  hoeheH: num(),
  laengeL: num(),
  steuerzeitSZ: num(),
  auspuff: AuspuffInputSchema.optional(),
});

export const UeberstroemerInputSchema = z.object({
  anzahl: num(),
  breiteB: num(),
  flaecheA: num(),
  hoeheH: num(),
  steuerzeitSZ: num(),
});

export const MotorInputSchema = z.object({
  name: name(),
  bohrungD: num(),
  brennraumV: num(),
  deachsierungL: num(),
  heizwertU: num(),
  hubL: num(),
  hubraumV: num(),
  kolbenG: num(),
  kurbelgehaeuseV: num(),
  pleulL: num(),
  resonanzU: num(),
  verdichtungV: num(),
  zuendzeitpunkt: num(),
  zylinderAnz: num(),
  einlass: EinlassInputSchema.optional(),
  auslass: AuslassInputSchema.optional(),
  ueberstroemer: UeberstroemerInputSchema.optional(),
});

export const VehicleInputSchema = z.object({
  name: name(),
  beschreibung: text(),
  gewicht: num(),
  motor: MotorInputSchema.optional(),
});

export const EnvironmentInputSchema = z.object({
  name: name(),
  temperaturT: num(),
  luftdruckP: num(),
});

export const DynoInputSchema = z.object({
  name: name(),
  beschreibung: text(),
  active: z.boolean().optional(),
  environmentId: z.string().min(1).max(64).nullable().optional(),
});

export const MAX_DYNO_POINTS = 20000;

export const DrehzahlPointSchema = z.object({
  zeit: z.number().finite(),
  drehzahl: z.number().finite(),
});

export const DynoPsPointSchema = z.object({
  drehzahl: z.number().finite(),
  ps: z.number().finite(),
});

export const GpsSampleSchema = z.object({
  latitude: z.number().min(-90).max(90),
  longitude: z.number().min(-180).max(180),
  speed: num(),
  accuracy: num(),
  altitude: num(),
  altitudeAccuracy: num(),
  heading: num(),
  headingAccuracy: num(),
  zeitpunkt: z.iso.datetime().nullable().optional(),
});

export const DynoDataInputSchema = z.object({
  drehzahl: z.array(DrehzahlPointSchema).max(MAX_DYNO_POINTS).optional(),
  dynoPs: z.array(DynoPsPointSchema).max(MAX_DYNO_POINTS).optional(),
  ausrollen: z.array(GpsSampleSchema).max(MAX_DYNO_POINTS).optional(),
  geschwindigkeit: z.array(GpsSampleSchema).max(MAX_DYNO_POINTS).optional(),
});

/** Portable dyno export (replaces the v2 ZIP with `DataExport.json`). */
export const DYNO_EXPORT_FORMAT = 'sim-tuning-dyno' as const;
export const DynoExportSchema = z.object({
  format: z.literal(DYNO_EXPORT_FORMAT),
  version: z.literal(3),
  exportedAt: z.iso.datetime(),
  vehicle: VehicleInputSchema,
  dyno: DynoInputSchema.omit({ environmentId: true }).extend(DynoDataInputSchema.shape),
});

export type VergaserInputDto = z.infer<typeof VergaserInputSchema>;
export type EinlassInputDto = z.infer<typeof EinlassInputSchema>;
export type AuspuffInputDto = z.infer<typeof AuspuffInputSchema>;
export type AuslassInputDto = z.infer<typeof AuslassInputSchema>;
export type UeberstroemerInputDto = z.infer<typeof UeberstroemerInputSchema>;
export type MotorInputDto = z.infer<typeof MotorInputSchema>;
export type VehicleInputDto = z.infer<typeof VehicleInputSchema>;
export type EnvironmentInputDto = z.infer<typeof EnvironmentInputSchema>;
export type DynoInputDto = z.infer<typeof DynoInputSchema>;
export type DrehzahlPointDto = z.infer<typeof DrehzahlPointSchema>;
export type DynoPsPointDto = z.infer<typeof DynoPsPointSchema>;
export type GpsSampleDto = z.infer<typeof GpsSampleSchema>;
export type DynoDataInputDto = z.infer<typeof DynoDataInputSchema>;
export type DynoExportDto = z.infer<typeof DynoExportSchema>;

// ─────────────────────────────────────────────────────────────────────────────
// Responses
// ─────────────────────────────────────────────────────────────────────────────
type Nullable<T> = { [K in keyof T]-?: Exclude<T[K], undefined> | null };
type Fields<T, Omit extends keyof T> = Nullable<Pick<T, Exclude<keyof T, Omit>>>;

interface Timestamps {
  id: string;
  createdAt: string;
  updatedAt: string;
}

export type VergaserDto = Timestamps & Fields<VergaserInputDto, never>;
export type EinlassDto = Timestamps &
  Fields<EinlassInputDto, 'vergaser'> & { vergaser: VergaserDto | null };
export type AuspuffDto = Timestamps &
  Fields<AuspuffInputDto, 'diffusorStage'> & { diffusorStage: number };
export type AuslassDto = Timestamps &
  Fields<AuslassInputDto, 'auspuff'> & { auspuff: AuspuffDto | null };
export type UeberstroemerDto = Timestamps & Fields<UeberstroemerInputDto, never>;
export type MotorDto = Timestamps &
  Fields<MotorInputDto, 'name' | 'einlass' | 'auslass' | 'ueberstroemer'> & {
    name: string;
    einlass: EinlassDto | null;
    auslass: AuslassDto | null;
    ueberstroemer: UeberstroemerDto | null;
  };

export interface VehicleDto extends Timestamps {
  /** Set for shared starter vehicles (read-only, can be duplicated). */
  systemKey: string | null;
  isSystem: boolean;
  name: string;
  beschreibung: string | null;
  gewicht: number | null;
  motor: MotorDto | null;
  dynoId: string | null;
}

export interface EnvironmentDto extends Timestamps {
  systemKey: string | null;
  isSystem: boolean;
  name: string;
  temperaturT: number | null;
  luftdruckP: number | null;
}

export type GpsSampleResponseDto = Nullable<GpsSampleDto>;

export interface DynoDto extends Timestamps {
  vehicleId: string;
  environmentId: string | null;
  name: string;
  beschreibung: string | null;
  active: boolean;
  drehzahl: DrehzahlPointDto[];
  dynoPs: DynoPsPointDto[];
  ausrollen: GpsSampleResponseDto[];
  geschwindigkeit: GpsSampleResponseDto[];
}
