import {
  getCompression,
  getCylinderHoleDiameter,
  getDistanceToOT,
  getGrindingDiameters,
  getHubRadius,
  getKolbenDurchmesser,
  getPortTimingDifference,
  getSteuerdiagramm,
  getSteuerwinkel,
  getSteuerwinkelOeffnet,
  getSteuerwinkelSchliesst,
  getToDecreasingLength,
  getVorauslass,
  type GrindingDiameters,
  type SteuerdiagrammGeometry,
  type SteuerdiagrammPort,
  type SteuerwinkelResult,
} from '@sim-tuning/common/logic';

/**
 * Null-safe glue between the motor views and `@sim-tuning/common/logic` (golden-tested v2
 * formulas). These helpers only validate inputs and pick the right formula; they never
 * re-implement maths. All lengths in mm, volumes in mm³, angles in °KW (v3 base units).
 */

export type MaybeNumber = number | null | undefined;

const isNum = (v: MaybeNumber): v is number => typeof v === 'number' && Number.isFinite(v);
const isPositive = (v: MaybeNumber): v is number => isNum(v) && v > 0;
const finiteOrNull = (v: number): number | null => (Number.isFinite(v) ? v : null);

/** Default piston clearance (Einbauspiel) from v2: 0.03 mm. */
export const DEFAULT_EINBAUSPIEL_MM = 0.03;

export interface HubraumResult {
  bohrungD: number;
  kolbenD: number;
  grinding: GrindingDiameters;
}

/** v2 MotorHubraum: bore from stroke + displacement, piston diameter from bore − clearance. */
export function computeHubraum(input: {
  hubL: MaybeNumber;
  hubraumV: MaybeNumber;
  einbauspielMm: MaybeNumber;
}): HubraumResult | null {
  if (!isPositive(input.hubL) || !isPositive(input.hubraumV)) return null;
  const bohrungD = getCylinderHoleDiameter(input.hubraumV, input.hubL);
  if (!Number.isFinite(bohrungD)) return null;
  const einbauspiel = isNum(input.einbauspielMm) ? input.einbauspielMm : DEFAULT_EINBAUSPIEL_MM;
  // getKolbenDurchmesser expects the clearance in 1/100 mm.
  const kolbenD = getKolbenDurchmesser(bohrungD, einbauspiel * 100);
  return { bohrungD, kolbenD, grinding: getGrindingDiameters(bohrungD) };
}

/** Target compression ratios offered by v2 (`Zielverdichtungen`). */
export const ZIELVERDICHTUNGEN = [8, 8.5, 9, 9.5, 10, 10.5, 11, 11.5, 12] as const;

export interface VerdichtungResult {
  current: number | null;
  abdrehen: number | null;
}

/** v2 MotorVerdichtung: current ε and how much to machine off for a target ε. */
export function computeVerdichtung(input: {
  hubraumV: MaybeNumber;
  brennraumV: MaybeNumber;
  bohrungD: MaybeNumber;
  zielVerdichtung: MaybeNumber;
}): VerdichtungResult {
  const { hubraumV, brennraumV, bohrungD, zielVerdichtung } = input;
  const current =
    isPositive(hubraumV) && isPositive(brennraumV)
      ? finiteOrNull(getCompression(hubraumV, brennraumV))
      : null;
  const abdrehen =
    isPositive(hubraumV) &&
    isPositive(brennraumV) &&
    isPositive(bohrungD) &&
    isNum(zielVerdichtung) &&
    zielVerdichtung > 1
      ? finiteOrNull(getToDecreasingLength(hubraumV, brennraumV, bohrungD, zielVerdichtung))
      : null;
  return { current, abdrehen };
}

/** v2 PortTimingPopup presets (Einlass / Auslass / Überströmer in °KW). */
export const PORT_TIMING_PRESETS = [
  { key: 'sehrKurz', einlass: 100, auslass: 125, ueberstroemer: 100 },
  { key: 'kurz', einlass: 120, auslass: 145, ueberstroemer: 110 },
  { key: 'mittel', einlass: 140, auslass: 165, ueberstroemer: 120 },
  { key: 'lang', einlass: 160, auslass: 185, ueberstroemer: 130 },
  { key: 'sehrLang', einlass: 180, auslass: 205, ueberstroemer: 140 },
] as const;

export type PortTimingPresetKey = (typeof PORT_TIMING_PRESETS)[number]['key'];

export const STEUERDIAGRAMM_PORTS: readonly SteuerdiagrammPort[] = [
  'einlass',
  'auslass',
  'ueberstroemer',
];

export interface PortAngles {
  steuerzeit: number;
  oeffnet: number;
  schliesst: number;
}

export interface SteuerzeitenResult {
  angles: Record<SteuerdiagrammPort, PortAngles | null>;
  vorauslass: number | null;
  /** Diagram geometry; lines only for ports with a valid timing. Null if no port is set. */
  diagram: SteuerdiagrammGeometry | null;
}

const validTiming = (v: MaybeNumber): v is number => isNum(v) && v > 0 && v < 360;

/** v2 MotorSteuerdiagramm: opening/closing angles, Vorauslass and the SVG geometry. */
export function computeSteuerzeiten(input: {
  einlass: MaybeNumber;
  auslass: MaybeNumber;
  ueberstroemer: MaybeNumber;
}): SteuerzeitenResult {
  const timings: Record<SteuerdiagrammPort, number | null> = {
    einlass: validTiming(input.einlass) ? input.einlass : null,
    auslass: validTiming(input.auslass) ? input.auslass : null,
    ueberstroemer: validTiming(input.ueberstroemer) ? input.ueberstroemer : null,
  };
  const args = (port: SteuerdiagrammPort, sz: number): [number, number, number] =>
    port === 'einlass' ? [sz, 0, 0] : port === 'auslass' ? [0, sz, 0] : [0, 0, sz];
  const angles = Object.fromEntries(
    STEUERDIAGRAMM_PORTS.map((port) => {
      const sz = timings[port];
      if (sz === null) return [port, null];
      return [
        port,
        {
          steuerzeit: sz,
          oeffnet: getSteuerwinkelOeffnet(...args(port, sz)),
          schliesst: getSteuerwinkelSchliesst(...args(port, sz)),
        },
      ];
    }),
  ) as Record<SteuerdiagrammPort, PortAngles | null>;

  const vorauslass =
    timings.auslass !== null && timings.ueberstroemer !== null
      ? getVorauslass(timings.auslass, timings.ueberstroemer)
      : null;

  const anySet = STEUERDIAGRAMM_PORTS.some((p) => timings[p] !== null);
  let diagram: SteuerdiagrammGeometry | null = null;
  if (anySet) {
    const geometry = getSteuerdiagramm(
      timings.einlass ?? 0,
      timings.auslass ?? 0,
      timings.ueberstroemer ?? 0,
    );
    diagram = { ...geometry, lines: geometry.lines.filter((l) => timings[l.port] !== null) };
  }
  return { angles, vorauslass, diagram };
}

/** v2 MotorUmrechnung: crank radius from stroke, rod length and offset. */
export function computeHubradius(input: {
  hubL: MaybeNumber;
  pleulL: MaybeNumber;
  deachsierungL: MaybeNumber;
}): number | null {
  const { hubL, pleulL } = input;
  const deachsierung = isNum(input.deachsierungL) ? input.deachsierungL : 0;
  if (!isPositive(hubL) || !isPositive(pleulL) || pleulL * 2 <= hubL) return null;
  return finiteOrNull(getHubRadius(hubL, pleulL, deachsierung));
}

/** Piston distance below TDC (mm) for a crank angle before TDC. */
export function computeDistanceToOT(input: {
  pleulL: MaybeNumber;
  hubradius: MaybeNumber;
  deachsierungL: MaybeNumber;
  degree: MaybeNumber;
}): number | null {
  const { pleulL, hubradius, degree } = input;
  if (!isPositive(pleulL) || !isPositive(hubradius) || !isNum(degree)) return null;
  const deachsierung = isNum(input.deachsierungL) ? input.deachsierungL : 0;
  return finiteOrNull(getDistanceToOT(pleulL, hubradius, deachsierung, degree));
}

export type Steuerkante = 'oberkante' | 'unterkante';

/** Which piston edge controls a port: piston-port intake = top edge, others = bottom edge. */
export function steuerkanteForPort(port: SteuerdiagrammPort): Steuerkante {
  return port === 'einlass' ? 'oberkante' : 'unterkante';
}

export interface PortTimingChangeResult {
  winkel: SteuerwinkelResult;
  differenceDegree: number;
  differenceLength: number | null;
}

/** v2 MotorUmrechnung "Steuerzeit-Änderung": angles before/after and difference in ° and mm. */
export function computePortTimingChange(input: {
  vorher: MaybeNumber;
  nachher: MaybeNumber;
  kante: Steuerkante;
  pleulL: MaybeNumber;
  hubradius: MaybeNumber;
  deachsierungL: MaybeNumber;
}): PortTimingChangeResult | null {
  const { vorher, nachher, kante, pleulL, hubradius } = input;
  if (!validTiming(vorher) || !validTiming(nachher)) return null;
  const winkel = getSteuerwinkel(vorher, nachher, kante === 'oberkante', kante === 'unterkante');
  const differenceDegree = getPortTimingDifference(false, vorher, nachher);
  const deachsierung = isNum(input.deachsierungL) ? input.deachsierungL : 0;
  // v2 measures the travel between the opening angles (see TODO in MotorUmrechnungViewModel).
  const differenceLength =
    isPositive(pleulL) && isPositive(hubradius)
      ? finiteOrNull(
          getPortTimingDifference(
            true,
            winkel.oeffnetVorher,
            winkel.oeffnetNachher,
            pleulL,
            hubradius,
            deachsierung,
          ),
        )
      : null;
  return { winkel, differenceDegree, differenceLength };
}
