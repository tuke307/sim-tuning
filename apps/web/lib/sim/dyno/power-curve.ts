import { getLeistung, wattToPs } from '@sim-tuning/common/logic';
import {
  MAX_DYNO_POINTS,
  type DrehzahlPointDto,
  type DynoPsPointDto,
  type EnvironmentDto,
} from '@sim-tuning/common/schemas';
import type { DrehzahlPoint } from '@sim-tuning/common/audio';

/** Inputs of the v2 Diagnose page (defaults from `DynoDiagnosisViewModel`). */
export interface DiagnosisParams {
  /** Drag coefficient c_w. */
  cw: number;
  /** Overall gear ratio ("Gesamtübersetzung"). */
  gesamtuebersetzung: number;
  /** Vehicle + rider mass in kg. */
  gewicht: number;
  /** Frontal area in m². */
  frontA: number;
  /** Air density in kg/m³. */
  luftdichte: number;
}

/** v2 constants passed to `DynoLogic.GetLeistung`. */
export const DIAGNOSIS_CONSTANTS = {
  radius: 0.277,
  rollwiderstand: 0.005,
  gravity: 9.81,
  steigung: 0,
} as const;

export const DEFAULT_DIAGNOSIS_PARAMS: DiagnosisParams = {
  cw: 0.8,
  gesamtuebersetzung: 14,
  gewicht: 170,
  frontA: 0.75,
  luftdichte: 1.2,
};

const SPECIFIC_GAS_CONSTANT_DRY_AIR = 287.05;

/**
 * Air density from the dyno environment (ideal gas, dry air). v2 hard-coded 1.2 kg/m³ and only
 * displayed the environment's pressure; v3 uses the environment when both values are set and
 * plausible and falls back to 1.2 kg/m³ otherwise. Environment values are stored in hPa / °C.
 */
export function airDensity(
  environment: Pick<EnvironmentDto, 'luftdruckP' | 'temperaturT'> | null | undefined,
): number {
  const p = environment?.luftdruckP;
  const t = environment?.temperaturT;
  if (typeof p !== 'number' || typeof t !== 'number') return DEFAULT_DIAGNOSIS_PARAMS.luftdichte;
  const kelvin = t + 273.15;
  if (p <= 0 || kelvin <= 0) return DEFAULT_DIAGNOSIS_PARAMS.luftdichte;
  return (p * 100) / (SPECIFIC_GAS_CONSTANT_DRY_AIR * kelvin);
}

/**
 * Port of v2 `DynoDiagnosisViewModel.RefreshPlot`: takes the stored RPM curve up to (excluding)
 * its maximum, drops the first point (t = 0 would divide by zero) and computes the power for
 * every point with `DynoLogic.GetLeistung` (t in s). v2 plotted the raw watt value labelled as PS;
 * v3 converts with `wattToPs`. Non-finite results are skipped.
 */
export function computePowerCurve(
  drehzahl: readonly DrehzahlPointDto[],
  params: DiagnosisParams,
): DynoPsPointDto[] {
  if (drehzahl.length < 3) return [];
  let maxIndex = 0;
  for (let i = 1; i < drehzahl.length; i++) {
    if (drehzahl[i]!.drehzahl > drehzahl[maxIndex]!.drehzahl) maxIndex = i;
  }
  const range = drehzahl.slice(1, maxIndex);
  const out: DynoPsPointDto[] = [];
  for (const point of range) {
    const t = point.zeit / 1000;
    if (t === 0 || point.drehzahl === 0) continue;
    const watt = getLeistung(
      point.drehzahl,
      t,
      params.luftdichte,
      params.cw,
      params.gesamtuebersetzung,
      DIAGNOSIS_CONSTANTS.radius,
      params.gewicht,
      DIAGNOSIS_CONSTANTS.rollwiderstand,
      DIAGNOSIS_CONSTANTS.gravity,
      params.frontA,
      DIAGNOSIS_CONSTANTS.steigung,
    );
    const ps = wattToPs(watt);
    if (Number.isFinite(ps)) out.push({ drehzahl: point.drehzahl, ps });
  }
  return out.slice(0, MAX_DYNO_POINTS);
}

/** Peak of a power curve (or `null` for an empty curve). */
export function peakPower(curve: readonly DynoPsPointDto[]): DynoPsPointDto | null {
  return curve.reduce<DynoPsPointDto | null>(
    (best, p) => (!best || p.ps > best.ps ? p : best),
    null,
  );
}

/** v2 `ToDrehzahlModel`: fitted (x ms, y U/min) points → stored `drehzahl` series. */
export function toDrehzahlSeries(curve: readonly DrehzahlPoint[]): DrehzahlPointDto[] {
  return curve
    .filter((p) => Number.isFinite(p.x) && Number.isFinite(p.y))
    .slice(0, MAX_DYNO_POINTS)
    .map((p) => ({ zeit: p.x, drehzahl: p.y }));
}
