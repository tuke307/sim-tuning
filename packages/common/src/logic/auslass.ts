import { roundTo, truncate } from '../utils/round.util';

/** Exhaust calculations, ported 1:1 from v2 `AuslassLogic` (golden-tested). */

/** Exhaust gas velocity (m/s) from gas temperature (°C): `331 + 0.6·T`. */
export function getGasGeschwindigkeit(abgastemperatur: number): number {
  return roundTo(331 + 0.6 * abgastemperatur, 2);
}

/**
 * Header diameter from the exhaust port area. v2 quirk kept: `percentage / 100` is an integer
 * division, so percentages below 100 do not enlarge the area.
 */
export function getKruemmerDurchmesser(auslassflaeche: number, percentage = 10): number {
  const factor = 1 + truncate(percentage / 100);
  return roundTo((Math.sqrt(auslassflaeche * factor) / Math.sqrt(Math.PI)) * 2, 2);
}

/** Header length = torque factor · header diameter − port length. */
export function getKruemmerLaenge(
  kruemmerdurchmesser: number,
  drehmomentfaktor: number,
  auslassLaenge: number,
): number {
  return roundTo(drehmomentfaktor * kruemmerdurchmesser - auslassLaenge, 2);
}

/** Tuned length (mm) from exhaust timing (°KW), gas velocity/temperature and resonance rpm. */
export function getResonanzLaenge(
  auslassSteuerwinkel: number,
  abgasTemperatur: number,
  resonanzDrehzahl: number,
): number {
  return roundTo(
    ((auslassSteuerwinkel * abgasTemperatur) / ((360 * 2 * resonanzDrehzahl) / 60)) * 1000,
    2,
  );
}

/** Port open duration (v2 formula, unrounded). */
export function getVehiclePortDuration(auslassSteuerzeit: number, drehzahl: number): number {
  return ((60 * auslassSteuerzeit) / drehzahl) * 360;
}

// ─────────────────────────────────────────────────────────────────────────────
// Expansion chamber (Auspuff)
// ─────────────────────────────────────────────────────────────────────────────

export interface AuspuffInput {
  resonanzU?: number | null;
  auslass: {
    laengeL?: number | null;
    steuerzeitSZ?: number | null;
    durchmesserD?: number | null;
    flaecheA?: number | null;
  };
  auspuff: {
    abgasT?: number | null;
    diffusorStage: number;
    diffusorW1?: number | null;
    kruemmerF?: number | null;
    kruemmerW?: number | null;
    mittelteilF?: number | null;
    gegenKonusW?: number | null;
    endrohrL?: number | null;
    endrohrD?: number | null;
  };
}

export interface AuspuffResult {
  resonanzL: number;
  kruemmerD: number;
  kruemmerL: number;
  mittelteilD: number;
  diffusorD1: number;
  diffusorL1: number;
  diffusorD2: number;
  diffusorL2: number;
  diffusorD3: number;
  diffusorL3: number;
  gegenkonusL: number;
  gegenkonusD: number;
  mittelteilL: number;
}

const v = (value: number | null | undefined) => value ?? 0;

/**
 * v2 `AuslassLogic.Calculate`. Only the one-stage diffusor is implemented in v2 — stages 2 and 3
 * reset every diffusor section to 0; that behaviour is preserved.
 */
export function calculateAuspuff(input: AuspuffInput): AuspuffResult {
  const { auslass, auspuff } = input;
  const resonanzL = getResonanzLaenge(
    v(auslass.steuerzeitSZ),
    v(auspuff.abgasT),
    v(input.resonanzU),
  );
  const kruemmerD = v(auslass.durchmesserD);
  const kruemmerL = getKruemmerLaenge(
    v(auslass.durchmesserD),
    v(auspuff.kruemmerF),
    v(auslass.laengeL),
  );
  const mittelteilD = roundTo(
    Math.sqrt((v(auslass.flaecheA) * 4) / Math.PI) * v(auspuff.mittelteilF),
    2,
  );

  let diffusorD1 = 0;
  let diffusorL1 = 0;
  if (auspuff.diffusorStage === 1) {
    diffusorD1 = roundTo(
      2 * Math.tan((v(auspuff.kruemmerW) * Math.PI) / 360) * kruemmerL + kruemmerD,
      2,
    );
    diffusorL1 = roundTo(
      (mittelteilD - diffusorD1) / (2 * Math.tan(v(auspuff.diffusorW1) * ((2 * Math.PI) / 360))),
      2,
    );
  }

  const gegenkonusL = roundTo(
    (mittelteilD - v(auspuff.endrohrD)) / (2 * Math.tan((v(auspuff.gegenKonusW) * Math.PI) / 180)),
    2,
  );
  const mittelteilL = roundTo(resonanzL - (kruemmerL + diffusorL1 + gegenkonusL / 2), 2);

  return {
    resonanzL,
    kruemmerD,
    kruemmerL,
    mittelteilD,
    diffusorD1,
    diffusorL1,
    diffusorD2: 0,
    diffusorL2: 0,
    diffusorD3: 0,
    diffusorL3: 0,
    gegenkonusL,
    gegenkonusD: mittelteilD,
    mittelteilL,
  };
}

export interface AuspuffSegment {
  kind: 'frame' | 'boundary' | 'dimension';
  x1: number;
  y1: number;
  x2: number;
  y2: number;
}

export interface AuspuffRect {
  x: number;
  y: number;
  width: number;
  height: number;
}

export type AuspuffLabelKey = 'kruemmer' | 'konus' | 'mittelteil' | 'gegenkonus' | 'endrohr';

export interface AuspuffGeometry {
  width: number;
  height: number;
  segments: AuspuffSegment[];
  rects: AuspuffRect[];
  /** Dimension codes (LK, LD, LM, …, D1, D2) placed like v2. */
  valueLabels: Array<{ text: string; x: number; y: number }>;
  /** Part names, translated by the UI. */
  partLabels: Array<{ key: AuspuffLabelKey; x: number; y: number }>;
}

/** v2 `ComputeGeometry` + `Draw*` as vector primitives (2000×1000 canvas, px = mm). */
export function getAuspuffGeometry(result: AuspuffResult, input: AuspuffInput): AuspuffGeometry {
  const width = 2000;
  const height = 1000;
  const i = truncate;
  const w1 = i(result.kruemmerL);
  const w2 = i(result.diffusorL1);
  const w3 = i(result.diffusorL2);
  const w4 = i(result.diffusorL3);
  const w5 = i(result.mittelteilL);
  const w6 = i(result.gegenkonusL);
  const w7 = i(v(input.auspuff.endrohrL));
  // v2: `(int)value * 2` casts first, then doubles; ydiff = height / 2 (integer division).
  const yd = (d: number) => i((i(d) * 2) / 2);
  const yd1 = yd(result.kruemmerD);
  const yd2 = yd(result.diffusorD1);
  const yd3 = yd(result.diffusorD2);
  const yd4 = yd(result.diffusorD3);
  const yd5 = yd(result.mittelteilD);
  const yd6 = yd(result.gegenkonusD);
  const yd7 = yd(v(input.auspuff.endrohrD));
  const h5 = i(result.mittelteilD) * 2;
  const h7 = i(v(input.auspuff.endrohrD)) * 2;

  const x1 = 100;
  const x2 = x1 + w1;
  const x3 = x2 + w2;
  const x4 = x3 + w3;
  const x5 = x4 + w4;
  const x6 = x5 + w5;
  const x7 = x6 + w6;
  const ym = height / 2 + height / 4;

  const hasD1 = result.diffusorL1 !== 0;
  const hasD2 = hasD1 && result.diffusorL2 !== 0;
  const hasD3 = hasD2 && result.diffusorL3 !== 0;

  const segments: AuspuffSegment[] = [];
  const rects: AuspuffRect[] = [];
  const line = (kind: AuspuffSegment['kind'], ax: number, ay: number, bx: number, by: number) =>
    segments.push({ kind, x1: ax, y1: ay, x2: bx, y2: by });
  const cone = (xa: number, ya: number, xb: number, yb: number) => {
    line('frame', xa, ym - ya, xb, ym - yb);
    line('frame', xa, ym + ya, xb, ym + yb);
    line('frame', xa, ym - ya, xa, ym + ya);
  };

  // Framework (red)
  cone(x1, yd1, x2, yd2);
  if (hasD1) {
    if (hasD2) {
      if (hasD3) cone(x4, yd4, x5, yd5);
      else cone(x3, yd3, x4, yd4);
    } else {
      cone(x2, yd2, x5, yd5);
    }
  }
  rects.push({ x: x5, y: ym - yd5, width: w5, height: h5 });
  line('frame', x6, ym - yd6, x7, ym - yd7);
  line('frame', x6, ym + yd6, x7, ym + yd7);
  rects.push({ x: x7, y: ym - yd7, width: w7, height: h7 });

  // Length dimensions (black)
  const dim = (xa: number, ya: number, xb: number) => {
    line('boundary', xa, ym + ya, xa, ym + 200);
    line('dimension', xa, ym + 175, xb, ym + 175);
  };
  dim(x1, yd1, x2);
  if (hasD1) {
    if (hasD2) {
      dim(x2, yd2, x3);
      dim(x4, yd3, x5);
      line('dimension', x4, ym + 175, x5, ym + 175);
      if (hasD3) {
        line('boundary', x6, ym + yd4, x6, ym + 200);
        line('dimension', x4, ym + 175, x5, ym + 175);
      }
    } else {
      dim(x2, yd2, x3);
    }
  }
  dim(x5, yd5, x6);
  dim(x6, yd6, x7);
  line('boundary', x7, ym + yd7, x7, ym + 200);
  line('boundary', x7 + w7, ym + yd7, x7 + w7, ym + 200);
  line('dimension', x7, ym + 175, x7 + w7, ym + 175);
  line('dimension', x2, ym + 175, x5, ym + 175);

  // Diameter dimensions (header and stinger)
  line('boundary', x1, ym - yd1, x1 - 50, ym - yd1);
  line('boundary', x1, ym + yd1, x1 - 50, ym + yd1);
  line('dimension', x1 - 35, ym - yd1, x1 - 35, ym + yd1);
  line('boundary', x7 + w7, ym - yd7, x7 + w7 + 50, ym - yd7);
  line('boundary', x7 + w7, ym + yd7, x7 + w7 + 50, ym + yd7);
  line('dimension', x7 + w7 + 35, ym - yd7, x7 + w7 + 35, ym + yd7);

  const valueLabels: AuspuffGeometry['valueLabels'] = [
    { text: 'LK', x: x1 + i(w1 / 2) - 15, y: ym + 200 },
  ];
  if (hasD1) {
    if (hasD2) {
      valueLabels.push({ text: 'LD1', x: x2 + i(w2 / 2) - 15, y: ym + 150 });
      valueLabels.push({ text: 'LD2', x: x3 + i(w3 / 2) - 15, y: ym + 150 });
      if (hasD3) valueLabels.push({ text: 'LD3', x: x4 + i(w4 / 2) - 15, y: ym + 150 });
    } else {
      valueLabels.push({ text: 'LD', x: x2 + i(w2 / 2) - 15, y: ym + 200 });
    }
  }
  valueLabels.push(
    { text: 'LM', x: x5 + i(w5 / 2) - 15, y: ym + 200 },
    { text: 'LG', x: x6 + i(w6 / 2) - 15, y: ym + 200 },
    { text: 'LE', x: x7 + i(w7 / 2) - 15, y: ym + 200 },
    { text: 'D1', x: x1 - 90, y: ym - 15 },
    { text: 'D2', x: x7 + w7 + 65, y: ym - 15 },
  );

  return {
    width,
    height,
    segments,
    rects,
    valueLabels,
    partLabels: [
      { key: 'kruemmer', x: x1 + 20, y: ym },
      { key: 'konus', x: x2 + 20, y: ym },
      { key: 'mittelteil', x: x5 + 20, y: ym },
      { key: 'gegenkonus', x: x6 + 20, y: ym },
      { key: 'endrohr', x: x7 + 20, y: ym },
    ],
  };
}
