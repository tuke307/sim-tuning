import { einlass } from '@sim-tuning/common/logic';
import type { VehicleDto, VehicleInputDto } from '@sim-tuning/common/schemas';
import { convertUnit } from '@sim-tuning/common/units';

/**
 * Pure glue between the Einlass views and the golden-tested v2 formulas. All draft values are in
 * field base units (mm, mm², mm³, °KW, U/min); the conversions below mirror the
 * `UnitsNet.UnitConverter.Convert` calls of the v2 view models.
 */

export type Maybe = number | null | undefined;

const has = (value: Maybe): value is number => typeof value === 'number' && Number.isFinite(value);
const finite = (value: number): number | null => (Number.isFinite(value) ? value : null);

/** Default resistance factors of v2 `EinlassLogic` (not exposed in the v2 UI). */
export const VERGASER_WIDERSTAND_DEFAULT = 0.9;
export const HAUPTDUESE_WIDERSTAND_DEFAULT = 0.95;

// ─────────────────────────────────────────────────────────────────────────────
// Kanal (intake tract resonance)
// ─────────────────────────────────────────────────────────────────────────────

export interface KanalDraft {
  /** Intake port area (mm²). */
  flaecheA: Maybe;
  /** Intake timing (°KW). */
  steuerzeitSZ: Maybe;
  /** Crankcase volume (mm³). */
  kurbelgehaeuseV: Maybe;
  /** Resonance rpm (U/min). */
  resonanzU: Maybe;
  /** Intake pipe diameter (mm). */
  durchmesserD: Maybe;
}

export function kanalDraftFromVehicle(vehicle: VehicleDto | null): KanalDraft {
  const motor = vehicle?.motor;
  return {
    flaecheA: motor?.einlass?.flaecheA ?? null,
    steuerzeitSZ: motor?.einlass?.steuerzeitSZ ?? null,
    kurbelgehaeuseV: motor?.kurbelgehaeuseV ?? null,
    resonanzU: motor?.resonanzU ?? null,
    durchmesserD: motor?.einlass?.durchmesserD ?? null,
  };
}

/**
 * Intake resonance length in mm (v2 result unit). Inputs are converted to cm², cm³ and cm like
 * v2. Returns `null` while inputs are incomplete or the result is not finite (e.g. rpm = 0).
 */
export function computeKanalResonanzLaenge(draft: KanalDraft): number | null {
  const { flaecheA, steuerzeitSZ, kurbelgehaeuseV, resonanzU, durchmesserD } = draft;
  if (!has(flaecheA) || !has(steuerzeitSZ) || !has(kurbelgehaeuseV)) return null;
  if (!has(resonanzU) || !has(durchmesserD)) return null;
  return finite(
    einlass.getResonanzLaenge(
      convertUnit(flaecheA, 'squareMillimeter', 'squareCentimeter'),
      steuerzeitSZ,
      convertUnit(kurbelgehaeuseV, 'cubicMillimeter', 'cubicCentimeter'),
      resonanzU,
      convertUnit(durchmesserD, 'millimeter', 'centimeter'),
    ),
  );
}

/** Kanal inputs mirrored onto the vehicle (v2 `VehicleMotor*` mirror properties). */
export function buildKanalUpdate(motorName: string, draft: KanalDraft): Partial<VehicleInputDto> {
  return {
    motor: {
      name: motorName,
      kurbelgehaeuseV: draft.kurbelgehaeuseV ?? null,
      resonanzU: draft.resonanzU ?? null,
      einlass: {
        flaecheA: draft.flaecheA ?? null,
        steuerzeitSZ: draft.steuerzeitSZ ?? null,
        durchmesserD: draft.durchmesserD ?? null,
      },
    },
  };
}

// ─────────────────────────────────────────────────────────────────────────────
// Vergaser (carburettor)
// ─────────────────────────────────────────────────────────────────────────────

export interface VergaserDraft {
  /** Displacement (mm³). */
  hubraumV: Maybe;
  /** Resonance rpm (U/min). */
  resonanzU: Maybe;
  /** Resistance factor of the bore formula. */
  vergaserFaktor: Maybe;
  /** Resistance factor of the main jet formula. */
  hauptdueseFaktor: Maybe;
}

export function vergaserDraftFromVehicle(vehicle: VehicleDto | null): VergaserDraft {
  return {
    hubraumV: vehicle?.motor?.hubraumV ?? null,
    resonanzU: vehicle?.motor?.resonanzU ?? null,
    vergaserFaktor: VERGASER_WIDERSTAND_DEFAULT,
    hauptdueseFaktor: HAUPTDUESE_WIDERSTAND_DEFAULT,
  };
}

export interface VergaserResult {
  /** Carburettor bore (mm). */
  durchmesserD: number | null;
  /** Main jet size (v2 unit: µm). */
  hauptdueseD: number | null;
}

export function computeVergaser(draft: VergaserDraft): VergaserResult {
  const { hubraumV, resonanzU } = draft;
  const kV = has(draft.vergaserFaktor) ? draft.vergaserFaktor : VERGASER_WIDERSTAND_DEFAULT;
  const kH = has(draft.hauptdueseFaktor) ? draft.hauptdueseFaktor : HAUPTDUESE_WIDERSTAND_DEFAULT;
  if (!has(hubraumV) || !has(resonanzU)) return { durchmesserD: null, hauptdueseD: null };
  const durchmesserD = finite(
    einlass.getVergaserDurchmesser(
      convertUnit(hubraumV, 'cubicMillimeter', 'cubicCentimeter'),
      resonanzU,
      kV,
    ),
  );
  const hauptdueseD =
    durchmesserD === null
      ? null
      : finite(einlass.getVergaserHauptduesenDurchmesser(durchmesserD, kH));
  return { durchmesserD, hauptdueseD };
}

/** Vergaser inputs plus the computed bore (v2 `VehicleMotorEinlassVergaserDurchmesserD`). */
export function buildVergaserUpdate(
  motorName: string,
  draft: VergaserDraft,
  result: VergaserResult,
): Partial<VehicleInputDto> {
  return {
    motor: {
      name: motorName,
      hubraumV: draft.hubraumV ?? null,
      resonanzU: draft.resonanzU ?? null,
      einlass: { vergaser: { durchmesserD: result.durchmesserD } },
    },
  };
}
