import { auslass } from '@sim-tuning/common/logic';
import type { VehicleDto, VehicleInputDto } from '@sim-tuning/common/schemas';
import { convertUnit } from '@sim-tuning/common/units';

/**
 * Pure glue between the Auslass views and the golden-tested v2 formulas (`auslass.*`). Draft
 * values are kept in field base units (mm, mm², °C, °KW, U/min, factors, degrees).
 */

export type Maybe = number | null | undefined;
export type AuspuffInput = Parameters<typeof auslass.calculateAuspuff>[0];
export type AuspuffResult = ReturnType<typeof auslass.calculateAuspuff>;

const has = (value: Maybe): value is number => typeof value === 'number' && Number.isFinite(value);
const finite = (value: number): number | null => (Number.isFinite(value) ? value : null);
const orNull = (value: Maybe): number | null => value ?? null;

/** Torque factor slider range of v2 (`spitz` = peaky … `flach` = flat). */
export const KRUEMMER_FAKTOR_MIN = 6;
export const KRUEMMER_FAKTOR_MAX = 12;

/** Percentages v2 shows as header diameter range (`D(10 %) – D(20 %)`). */
export const KRUEMMER_PERCENT_RANGE = [10, 20] as const;

/** Diffusor stages offered by v2; only stage 1 is computed (stages 2/3 zero the diffusor). */
export const DIFFUSOR_STAGES = [1, 2, 3] as const;
export type DiffusorStage = (typeof DIFFUSOR_STAGES)[number];

// ─────────────────────────────────────────────────────────────────────────────
// Theorie
// ─────────────────────────────────────────────────────────────────────────────

export interface TheorieDraft {
  /** Header diameter (mm), input for the header length. */
  kruemmerD: Maybe;
  /** Torque factor (6 … 12). */
  kruemmerF: Maybe;
  /** Exhaust port area (mm²). */
  flaecheA: Maybe;
  /** Gas temperature for the velocity calculator (°C). v2 kept it on a scratch model. */
  gasT: Maybe;
  /** Exhaust timing (°KW). */
  steuerzeitSZ: Maybe;
  /** Exhaust gas temperature used for the tuned length (°C). */
  abgasT: Maybe;
  /** Resonance rpm (U/min). */
  resonanzU: Maybe;
}

export function theorieDraftFromVehicle(vehicle: VehicleDto | null): TheorieDraft {
  const motor = vehicle?.motor;
  const auspuff = motor?.auslass?.auspuff;
  return {
    kruemmerD: auspuff?.kruemmerD ?? null,
    kruemmerF: auspuff?.kruemmerF ?? null,
    flaecheA: motor?.auslass?.flaecheA ?? null,
    gasT: auspuff?.abgasT ?? null,
    steuerzeitSZ: motor?.auslass?.steuerzeitSZ ?? null,
    abgasT: auspuff?.abgasT ?? null,
    resonanzU: motor?.resonanzU ?? null,
  };
}

export interface TheorieResult {
  /** Header length (mm), v2: `getKruemmerLaenge(D, F, 0)`. */
  kruemmerL: number | null;
  /**
   * Header diameter range (mm) for 10 % and 20 %. v2 converts the area to cm², so the formula
   * yields cm; v2 labelled it "mm" — we convert the cm result to mm for display.
   */
  kruemmerDRange: [number, number] | null;
  /** Gas velocity (m/s). */
  gasV: number | null;
  /** Tuned length (mm). */
  resonanzL: number | null;
}

export function computeTheorie(draft: TheorieDraft): TheorieResult {
  const kruemmerL =
    has(draft.kruemmerD) && has(draft.kruemmerF)
      ? finite(auslass.getKruemmerLaenge(draft.kruemmerD, draft.kruemmerF, 0))
      : null;

  let kruemmerDRange: TheorieResult['kruemmerDRange'] = null;
  if (has(draft.flaecheA)) {
    const areaCm2 = convertUnit(draft.flaecheA, 'squareMillimeter', 'squareCentimeter');
    const [lo, hi] = KRUEMMER_PERCENT_RANGE.map((p) =>
      finite(convertUnit(auslass.getKruemmerDurchmesser(areaCm2, p), 'centimeter', 'millimeter')),
    );
    if (lo !== null && lo !== undefined && hi !== null && hi !== undefined) {
      kruemmerDRange = [lo, hi];
    }
  }

  const gasV = has(draft.gasT) ? finite(auslass.getGasGeschwindigkeit(draft.gasT)) : null;

  const resonanzL =
    has(draft.steuerzeitSZ) && has(draft.abgasT) && has(draft.resonanzU)
      ? finite(auslass.getResonanzLaenge(draft.steuerzeitSZ, draft.abgasT, draft.resonanzU))
      : null;

  return { kruemmerL, kruemmerDRange, gasV, resonanzL };
}

/** Theorie inputs and results mirrored onto the vehicle like the v2 `VehicleMotor*` bindings. */
export function buildTheorieUpdate(
  motorName: string,
  draft: TheorieDraft,
  result: TheorieResult,
): Partial<VehicleInputDto> {
  return {
    motor: {
      name: motorName,
      resonanzU: orNull(draft.resonanzU),
      auslass: {
        flaecheA: orNull(draft.flaecheA),
        steuerzeitSZ: orNull(draft.steuerzeitSZ),
        auspuff: {
          kruemmerD: orNull(draft.kruemmerD),
          kruemmerF: orNull(draft.kruemmerF),
          kruemmerL: result.kruemmerL,
          abgasT: orNull(draft.abgasT),
          resonanzL: result.resonanzL,
        },
      },
    },
  };
}

// ─────────────────────────────────────────────────────────────────────────────
// Anwendung (expansion chamber)
// ─────────────────────────────────────────────────────────────────────────────

export interface AnwendungDraft {
  resonanzU: Maybe;
  /** Exhaust port data (mm, mm², °KW). */
  auslassDurchmesserD: Maybe;
  auslassLaengeL: Maybe;
  auslassSteuerzeitSZ: Maybe;
  auslassFlaecheA: Maybe;
  /** Exhaust gas temperature (°C). */
  abgasT: Maybe;
  kruemmerF: Maybe;
  /** Header cone angle (°). */
  kruemmerW: Maybe;
  mittelteilF: Maybe;
  diffusorStage: DiffusorStage;
  /** Diffusor angle (°). */
  diffusorW1: Maybe;
  /** Baffle cone angle (°). */
  gegenKonusW: Maybe;
  /** Stinger (mm). */
  endrohrL: Maybe;
  endrohrD: Maybe;
}

export type AnwendungField = Exclude<keyof AnwendungDraft, 'diffusorStage'>;

const toStage = (value: number | null | undefined): DiffusorStage =>
  value === 2 || value === 3 ? value : 1;

export function anwendungDraftFromVehicle(vehicle: VehicleDto | null): AnwendungDraft {
  const motor = vehicle?.motor;
  const port = motor?.auslass;
  const auspuff = port?.auspuff;
  return {
    resonanzU: motor?.resonanzU ?? null,
    auslassDurchmesserD: port?.durchmesserD ?? null,
    auslassLaengeL: port?.laengeL ?? null,
    auslassSteuerzeitSZ: port?.steuerzeitSZ ?? null,
    auslassFlaecheA: port?.flaecheA ?? null,
    abgasT: auspuff?.abgasT ?? null,
    kruemmerF: auspuff?.kruemmerF ?? null,
    kruemmerW: auspuff?.kruemmerW ?? null,
    mittelteilF: auspuff?.mittelteilF ?? null,
    // Stored 0 = "not chosen yet"; v2 only implements stage 1, so that is the default.
    diffusorStage: toStage(auspuff?.diffusorStage),
    diffusorW1: auspuff?.diffusorW1 ?? null,
    gegenKonusW: auspuff?.gegenKonusW ?? null,
    endrohrL: auspuff?.endrohrL ?? null,
    endrohrD: auspuff?.endrohrD ?? null,
  };
}

const REQUIRED_FIELDS: readonly AnwendungField[] = [
  'abgasT',
  'resonanzU',
  'auslassDurchmesserD',
  'auslassLaengeL',
  'auslassSteuerzeitSZ',
  'auslassFlaecheA',
  'kruemmerF',
  'mittelteilF',
  'gegenKonusW',
  'endrohrD',
  'endrohrL',
];

/** Fields that must be filled before `calculateAuspuff` gives a meaningful result. */
export function missingAnwendungFields(draft: AnwendungDraft): AnwendungField[] {
  const required: AnwendungField[] = [...REQUIRED_FIELDS];
  if (draft.diffusorStage === 1) required.push('kruemmerW', 'diffusorW1');
  return required.filter((key) => !has(draft[key]));
}

export function toAuspuffInput(draft: AnwendungDraft): AuspuffInput {
  return {
    resonanzU: orNull(draft.resonanzU),
    auslass: {
      laengeL: orNull(draft.auslassLaengeL),
      steuerzeitSZ: orNull(draft.auslassSteuerzeitSZ),
      durchmesserD: orNull(draft.auslassDurchmesserD),
      flaecheA: orNull(draft.auslassFlaecheA),
    },
    auspuff: {
      abgasT: orNull(draft.abgasT),
      diffusorStage: draft.diffusorStage,
      diffusorW1: orNull(draft.diffusorW1),
      kruemmerF: orNull(draft.kruemmerF),
      kruemmerW: orNull(draft.kruemmerW),
      mittelteilF: orNull(draft.mittelteilF),
      gegenKonusW: orNull(draft.gegenKonusW),
      endrohrL: orNull(draft.endrohrL),
      endrohrD: orNull(draft.endrohrD),
    },
  };
}

export function isFiniteAuspuffResult(result: AuspuffResult): boolean {
  return Object.values(result).every((value) => Number.isFinite(value));
}

/**
 * Total pipe length (mm): sum of all section lengths. v2 showed a "Gesamtlänge" field but never
 * filled it; this is a plain sum, not a new formula.
 */
export function getGesamtLaenge(result: AuspuffResult, endrohrL: Maybe): number {
  const sum =
    result.kruemmerL +
    result.diffusorL1 +
    result.diffusorL2 +
    result.diffusorL3 +
    result.mittelteilL +
    result.gegenkonusL +
    (endrohrL ?? 0);
  return Math.round(sum * 100) / 100;
}

export type AnwendungOutcome =
  | { status: 'incomplete'; missing: AnwendungField[] }
  | { status: 'invalid' }
  | {
      status: 'ok';
      input: AuspuffInput;
      result: AuspuffResult;
      geometry: ReturnType<typeof auslass.getAuspuffGeometry>;
      gesamtL: number;
    };

/** Runs v2 `AuslassLogic.Auspuff`: calculation plus drawing primitives. */
export function runAnwendung(draft: AnwendungDraft): AnwendungOutcome {
  const missing = missingAnwendungFields(draft);
  if (missing.length > 0) return { status: 'incomplete', missing };
  const input = toAuspuffInput(draft);
  const result = auslass.calculateAuspuff(input);
  if (!isFiniteAuspuffResult(result)) return { status: 'invalid' };
  const geometry = auslass.getAuspuffGeometry(result, input);
  return {
    status: 'ok',
    input,
    result,
    geometry,
    gesamtL: getGesamtLaenge(result, draft.endrohrL),
  };
}

/** Inputs plus every computed section written to the vehicle's Auspuff (v2 `Calculate`). */
export function buildAnwendungUpdate(
  motorName: string,
  draft: AnwendungDraft,
  result: AuspuffResult,
  gesamtL: number,
): Partial<VehicleInputDto> {
  return {
    motor: {
      name: motorName,
      resonanzU: orNull(draft.resonanzU),
      auslass: {
        durchmesserD: orNull(draft.auslassDurchmesserD),
        laengeL: orNull(draft.auslassLaengeL),
        steuerzeitSZ: orNull(draft.auslassSteuerzeitSZ),
        flaecheA: orNull(draft.auslassFlaecheA),
        auspuff: {
          abgasT: orNull(draft.abgasT),
          kruemmerF: orNull(draft.kruemmerF),
          kruemmerW: orNull(draft.kruemmerW),
          mittelteilF: orNull(draft.mittelteilF),
          diffusorStage: draft.diffusorStage,
          diffusorW1: orNull(draft.diffusorW1),
          gegenKonusW: orNull(draft.gegenKonusW),
          endrohrL: orNull(draft.endrohrL),
          endrohrD: orNull(draft.endrohrD),
          resonanzL: result.resonanzL,
          kruemmerD: result.kruemmerD,
          kruemmerL: result.kruemmerL,
          mittelteilD: result.mittelteilD,
          mittelteilL: result.mittelteilL,
          diffusorD1: result.diffusorD1,
          diffusorL1: result.diffusorL1,
          diffusorD2: result.diffusorD2,
          diffusorL2: result.diffusorL2,
          diffusorD3: result.diffusorD3,
          diffusorL3: result.diffusorL3,
          gegenkonusD: result.gegenkonusD,
          gegenkonusL: result.gegenkonusL,
          // `gesamtL` is stored in centimetres (FIELD_BASE_UNITS).
          gesamtL: Math.round(convertUnit(gesamtL, 'millimeter', 'centimeter') * 1e4) / 1e4,
        },
      },
    },
  };
}

// ─────────────────────────────────────────────────────────────────────────────
// Drawing helpers
// ─────────────────────────────────────────────────────────────────────────────

const LABEL_ALLOWANCE_X = 60;
const LABEL_ALLOWANCE_Y = 30;

/**
 * ViewBox of the drawing: the v2 canvas (`0 0 2000 1000`) when everything fits, otherwise the
 * canvas grown (with padding) to the content, so long pipes are scaled down instead of clipped.
 * With `crop`, the viewBox hugs the content (plus padding) instead, so the chamber fills the
 * drawing area rather than sitting in the lower third of the v2 canvas.
 */
export function getAuspuffViewBox(
  geometry: ReturnType<typeof auslass.getAuspuffGeometry>,
  padding = 40,
  { crop = false }: { crop?: boolean } = {},
): { x: number; y: number; width: number; height: number } {
  const xs: number[] = [];
  const ys: number[] = [];
  for (const s of geometry.segments) {
    xs.push(s.x1, s.x2);
    ys.push(s.y1, s.y2);
  }
  for (const r of geometry.rects) {
    xs.push(r.x, r.x + r.width);
    ys.push(r.y, r.y + r.height);
  }
  // Labels are anchored at their start/baseline; reserve room for the text itself.
  for (const l of [...geometry.valueLabels, ...geometry.partLabels]) {
    xs.push(l.x, l.x + LABEL_ALLOWANCE_X);
    ys.push(l.y, l.y - LABEL_ALLOWANCE_Y);
  }
  const finiteXs = xs.filter(Number.isFinite);
  const finiteYs = ys.filter(Number.isFinite);
  const canvas = { x: 0, y: 0, width: geometry.width, height: geometry.height };
  if (finiteXs.length === 0 || finiteYs.length === 0) return canvas;
  const contentMinX = Math.min(...finiteXs);
  const contentMinY = Math.min(...finiteYs);
  const contentMaxX = Math.max(...finiteXs);
  const contentMaxY = Math.max(...finiteYs);
  if (crop) {
    return {
      x: contentMinX - padding,
      y: contentMinY - padding,
      width: contentMaxX - contentMinX + padding * 2,
      height: contentMaxY - contentMinY + padding * 2,
    };
  }
  const fits =
    contentMinX >= 0 &&
    contentMinY >= 0 &&
    contentMaxX <= geometry.width &&
    contentMaxY <= geometry.height;
  if (fits) return canvas;
  const minX = Math.min(0, contentMinX - padding);
  const minY = Math.min(0, contentMinY - padding);
  const maxX = Math.max(geometry.width, contentMaxX + padding);
  const maxY = Math.max(geometry.height, contentMaxY + padding);
  return { x: minX, y: minY, width: maxX - minX, height: maxY - minY };
}
