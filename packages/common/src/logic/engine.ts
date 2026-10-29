import { roundTo, toInt32 } from '../utils/round.util';

/**
 * Engine calculations, ported 1:1 from v2 `SimTuning.Core.ModuleLogic.EngineLogic`.
 * Units follow the v2 base units (mm, mm³, °KW, U/min). Known v2 quirks are kept on purpose and
 * pinned by golden tests (see `__fixtures__/v2-golden.json`); fixes belong in separate changes.
 */

/** Verdichtung ε = (Vh + Vc) / Vc, rounded to 1 decimal (v2 also took an unused bore). */
export function getCompression(hubraum: number, brennraum: number): number {
  return roundTo((hubraum + brennraum) / brennraum, 1);
}

/** Bohrungsdurchmesser d = 2·√(Vh / h) / √π, rounded to 2 decimals. */
export function getCylinderHoleDiameter(hubraum: number, hub: number): number {
  return roundTo((2 * Math.sqrt(hubraum / hub)) / Math.sqrt(Math.PI), 2);
}

/** Displacement `π·d²/4·h` (mm³ from mm), rounded to 1 decimal. v2 omitted the square. */
export function getDisplacement(bohrungsdurchmesser: number, hub: number): number {
  return roundTo(((Math.PI * bohrungsdurchmesser ** 2) / 4) * hub, 1);
}

/** Piston distance below TDC (mm) for a crank angle in °KW, with connecting rod and offset. */
export function getDistanceToOT(
  pleullaenge: number,
  hubradius: number,
  deachsierung: number,
  kwgrad: number,
): number {
  const kw = (kwgrad / 180) * Math.PI * 1;
  const beta = Math.asin(deachsierung / (pleullaenge + hubradius));
  const mmVorOt =
    Math.sqrt((pleullaenge + hubradius) ** 2 - deachsierung ** 2) -
    (hubradius * Math.cos(beta + kw) +
      Math.sqrt(pleullaenge ** 2 - (Math.sin(beta + kw) * hubradius - deachsierung) ** 2));
  return roundTo(mmVorOt, 2);
}

export interface GrindingDiameters {
  diameter1: number;
  diameter2: number;
  diameter3: number;
  diameter4: number;
}

/** Oversize (Schleifmaß) bores in 0.2 mm steps. */
export function getGrindingDiameters(diameter: number): GrindingDiameters {
  return {
    diameter1: diameter + 0.2,
    diameter2: diameter + 0.4,
    diameter3: diameter + 0.6,
    diameter4: diameter + 0.8,
  };
}

/** Crank radius from stroke, connecting rod length and offset (Deachsierung). */
export function getHubRadius(hub: number, pleullaenge: number, deachsierung: number): number {
  const hubradius =
    (hub * Math.sqrt(Math.abs((4 * deachsierung) ** 2 + hub ** 2 - 4 * pleullaenge ** 2))) /
    Math.sqrt(Math.abs(4 * hub ** 2 - 16 * pleullaenge ** 2));
  return roundTo(hubradius, 2);
}

/** Piston diameter = bore − clearance/100 (clearance in 1/100 mm). */
export function getKolbenDurchmesser(bohrungsdurchmesser: number, einbauspiel: number): number {
  return bohrungsdurchmesser - einbauspiel / 100;
}

/** Mean piston speed `h·n/30` (v2 formula, unit as in v2), rounded to 2 decimals. */
export function getKolbenGeschwindigkeit(hub: number, drehzahl: number): number {
  return roundTo((hub * drehzahl) / 30, 2);
}

/** Difference of two port timings, in °KW or (if `inMm`) in mm of piston travel. */
export function getPortTimingDifference(
  inMm: boolean,
  vorher: number,
  nachher: number,
  pleullaenge = 0,
  hubradius = 0,
  deachsierung = 0,
): number {
  let before = vorher;
  let after = nachher;
  if (inMm) {
    before = getDistanceToOT(pleullaenge, hubradius, deachsierung, before);
    after = getDistanceToOT(pleullaenge, hubradius, deachsierung, after);
  }
  return roundTo(Math.abs(after - before), 2);
}

export interface SteuerwinkelResult {
  oeffnetVorher: number;
  schliesstVorher: number;
  oeffnetNachher: number;
  schliesstNachher: number;
}

/**
 * Opening/closing crank angles before and after a timing change. Lower piston edge (exhaust,
 * transfer ports) is centred on BDC (180°), upper edge (piston-port intake) on TDC (0/360°).
 */
export function getSteuerwinkel(
  vorherSteuerzeit: number,
  nachherSteuerzeit: number,
  kolbenoberkante: boolean,
  kolbenunterkante: boolean,
): SteuerwinkelResult {
  if (kolbenunterkante) {
    return {
      oeffnetVorher: 180 - vorherSteuerzeit / 2,
      schliesstVorher: 180 + vorherSteuerzeit / 2,
      oeffnetNachher: 180 - nachherSteuerzeit / 2,
      schliesstNachher: 180 + nachherSteuerzeit / 2,
    };
  }
  if (kolbenoberkante) {
    return {
      oeffnetVorher: 360 - vorherSteuerzeit / 2,
      schliesstVorher: 0 + vorherSteuerzeit / 2,
      oeffnetNachher: 360 - nachherSteuerzeit / 2,
      schliesstNachher: 0 + nachherSteuerzeit / 2,
    };
  }
  return { oeffnetVorher: 0, schliesstVorher: 0, oeffnetNachher: 0, schliesstNachher: 0 };
}

/** Opening angle; the first non-zero timing wins (intake, exhaust, transfer), like v2. */
export function getSteuerwinkelOeffnet(
  steuerzeitEinlass = 0,
  steuerzeitAuslass = 0,
  steuerzeitUeberstroemer = 0,
): number {
  let angle = 0;
  if (steuerzeitEinlass !== 0) angle = 360 - steuerzeitEinlass / 2;
  else if (steuerzeitAuslass !== 0) angle = 180 - steuerzeitAuslass / 2;
  else if (steuerzeitUeberstroemer !== 0) angle = 180 - steuerzeitUeberstroemer / 2;
  return roundTo(angle, 2);
}

/** Closing angle; the first non-zero timing wins (intake, exhaust, transfer), like v2. */
export function getSteuerwinkelSchliesst(
  steuerzeitEinlass = 0,
  steuerzeitAuslass = 0,
  steuerzeitUeberstroemer = 0,
): number {
  let angle = 0;
  if (steuerzeitEinlass !== 0) angle = steuerzeitEinlass / 2;
  else if (steuerzeitAuslass !== 0) angle = 180 + steuerzeitAuslass / 2;
  else if (steuerzeitUeberstroemer !== 0) angle = 180 + steuerzeitUeberstroemer / 2;
  return roundTo(angle, 2);
}

/** How much (mm) to machine off the cylinder to reach a target compression ratio. */
export function getToDecreasingLength(
  hubraum: number,
  brennraum: number,
  durchmesser: number,
  zielVerdichtung: number,
): number {
  const abdrehen =
    (4 * (brennraum * zielVerdichtung - brennraum - hubraum)) /
    (Math.PI * (durchmesser ** 2 * zielVerdichtung - durchmesser ** 2));
  return roundTo(abdrehen, 2);
}

/** Vorauslass = (exhaust timing − transfer timing) / 2. */
export function getVorauslass(auslassSW: number, ueberstroemerSW: number): number {
  return roundTo((auslassSW - ueberstroemerSW) / 2, 2);
}

// ─────────────────────────────────────────────────────────────────────────────
// Steuerdiagramm (port timing diagram) geometry — replaces the v2 SkiaSharp bitmap
// ─────────────────────────────────────────────────────────────────────────────

export type SteuerdiagrammPort = 'einlass' | 'auslass' | 'ueberstroemer';

export interface SteuerdiagrammLine {
  port: SteuerdiagrammPort;
  edge: 'oeffnet' | 'schliesst';
  angle: number;
  x1: number;
  y1: number;
  x2: number;
  y2: number;
}

export interface SteuerdiagrammGeometry {
  size: number;
  center: number;
  radius: number;
  lines: SteuerdiagrammLine[];
  labels: Array<{ text: 'OT' | 'UT'; x: number; y: number }>;
}

const DIAGRAM_DIAMETER = 500;
const DIAGRAM_MARGIN = 50;

/** v2 `Functions.GetPointOnCircle` (integer pixel coordinates, y pointing down). */
export function getPointOnCircle(angle: number, radius: number): { x: number; y: number } {
  const xr = radius * Math.cos((angle * Math.PI) / 180);
  const yr = radius * Math.sin((angle * Math.PI) / 180);
  return {
    x: toInt32(radius + xr),
    y: yr >= 0 ? toInt32(radius - yr) : toInt32(radius + Math.abs(yr)),
  };
}

/**
 * Same picture as v2 `GetSteuerdiagramm` (600×600 px): circle, dashed TDC/BDC and 90°/270°
 * axes, one line per port edge, then rotated by 270° so TDC (OT) is on top.
 */
export function getSteuerdiagramm(
  einlass: number,
  auslass: number,
  ueberstroemer: number,
): SteuerdiagrammGeometry {
  const radius = DIAGRAM_DIAMETER / 2;
  const size = DIAGRAM_DIAMETER + DIAGRAM_MARGIN * 2;
  const center = size / 2;
  // Canvas rotation by 270° (clockwise, y down): x' = c + (y − c), y' = c − (x − c).
  const rotate = (x: number, y: number) => ({ x: center + (y - center), y: center - (x - center) });

  const edges: Array<[SteuerdiagrammPort, SteuerdiagrammLine['edge'], number]> = [
    ['einlass', 'oeffnet', getSteuerwinkelOeffnet(einlass)],
    ['einlass', 'schliesst', getSteuerwinkelSchliesst(einlass)],
    ['auslass', 'oeffnet', getSteuerwinkelOeffnet(0, auslass)],
    ['auslass', 'schliesst', getSteuerwinkelSchliesst(0, auslass)],
    ['ueberstroemer', 'oeffnet', getSteuerwinkelOeffnet(0, 0, ueberstroemer)],
    ['ueberstroemer', 'schliesst', getSteuerwinkelSchliesst(0, 0, ueberstroemer)],
  ];

  const start = rotate(center, center);
  const lines = edges.map(([port, edge, angle]) => {
    const p = getPointOnCircle(angle, radius);
    const end = rotate(p.x + DIAGRAM_MARGIN, p.y + DIAGRAM_MARGIN);
    return { port, edge, angle, x1: start.x, y1: start.y, x2: end.x, y2: end.y };
  });

  return {
    size,
    center,
    radius,
    lines,
    labels: [
      { text: 'OT', x: center + 10, y: 30 },
      { text: 'UT', x: center - 40, y: 580 },
    ],
  };
}
