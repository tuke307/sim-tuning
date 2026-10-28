import type { Prisma } from '@sim-tuning/database';
import { z } from 'zod';
import {
  EnvironmentInputSchema,
  MotorInputSchema,
  VehicleInputSchema,
} from '@sim-tuning/common/schemas';

/**
 * Field lists mirror the Zod input schemas in `@sim-tuning/common/schemas` (and the guest-mode
 * model in `apps/web/lib/data/model.ts`). They drive both the write mapping and the DTO mapping.
 */
export const VERGASER_KEYS = ['benzinLuftF', 'durchmesserD'] as const;
export const EINLASS_KEYS = [
  'breiteB',
  'durchmesserD',
  'flaecheA',
  'hoeheH',
  'laengeL',
  'luftBedarf',
  'steuerzeitSZ',
] as const;
export const AUSPUFF_KEYS = [
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
export const AUSLASS_KEYS = [
  'breiteB',
  'durchmesserD',
  'flaecheA',
  'hoeheH',
  'laengeL',
  'steuerzeitSZ',
] as const;
export const UEBERSTROEMER_KEYS = [
  'anzahl',
  'breiteB',
  'flaecheA',
  'hoeheH',
  'steuerzeitSZ',
] as const;
export const MOTOR_KEYS = [
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
export const VEHICLE_KEYS = ['name', 'beschreibung', 'gewicht'] as const;
export const GPS_KEYS = [
  'latitude',
  'longitude',
  'speed',
  'accuracy',
  'altitude',
  'altitudeAccuracy',
  'heading',
  'headingAccuracy',
] as const;

export const MAX_NAME_LENGTH = 120;

// ─────────────────────────────────────────────────────────────────────────────
// Prisma includes (always load the fully nested shapes the DTOs need)
// ─────────────────────────────────────────────────────────────────────────────
export const MOTOR_INCLUDE = {
  einlass: { include: { vergaser: true } },
  auslass: { include: { auspuff: true } },
  ueberstroemer: true,
} satisfies Prisma.MotorInclude;

export const VEHICLE_INCLUDE = {
  motor: { include: MOTOR_INCLUDE },
  dyno: { select: { id: true } },
} satisfies Prisma.VehicleInclude;

const byPosition = { orderBy: { position: 'asc' } } as const;

export const DYNO_INCLUDE = {
  drehzahl: byPosition,
  dynoPs: byPosition,
  ausrollen: byPosition,
  geschwindigkeit: byPosition,
} satisfies Prisma.DynoInclude;

export type MotorRow = Prisma.MotorGetPayload<{ include: typeof MOTOR_INCLUDE }>;
export type VehicleRow = Prisma.VehicleGetPayload<{ include: typeof VEHICLE_INCLUDE }>;
export type DynoRow = Prisma.DynoGetPayload<{ include: typeof DYNO_INCLUDE }>;

// ─────────────────────────────────────────────────────────────────────────────
// Ownership filters (deny by default)
// ─────────────────────────────────────────────────────────────────────────────
/** Rows the user may read: their own plus shared starter content (`userId = null` + systemKey). */
export const readableBy = (userId: string) => ({
  OR: [{ userId }, { userId: null, systemKey: { not: null } }],
});

export const isSystemRow = (row: { systemKey: string | null; userId: string | null }) =>
  row.systemKey !== null && row.userId === null;

// ─────────────────────────────────────────────────────────────────────────────
// Request schemas derived from the shared contract
// ─────────────────────────────────────────────────────────────────────────────
/** Route ids: non-empty, bounded strings (uuid in practice). */
export const IdParamSchema = z.string().trim().min(1).max(64);

/** PATCH /vehicles/:id — every field optional, nested parts partial (motor name too). */
export const VehicleUpdateSchema = VehicleInputSchema.partial().extend({
  motor: MotorInputSchema.partial().optional(),
});
export type VehicleUpdateDto = z.infer<typeof VehicleUpdateSchema>;

export const EnvironmentUpdateSchema = EnvironmentInputSchema.partial();
export type EnvironmentUpdateDto = z.infer<typeof EnvironmentUpdateSchema>;

export const DuplicateVehicleSchema = z.object({
  name: z.string().trim().min(1).max(MAX_NAME_LENGTH).optional(),
});
export type DuplicateVehicleDto = z.infer<typeof DuplicateVehicleSchema>;
