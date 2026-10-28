import { roundTo } from '../utils/round.util';

/**
 * Unit catalogue ported from the v2 `*Quantity` lists (UnitsNet). Every quantity is converted
 * through an SI reference unit (`factor` = size of one unit in the reference unit). Values are
 * stored in each field's *base unit* (see `FIELD_BASE_UNITS`), exactly like v2.
 */
export type QuantityKind =
  'length' | 'area' | 'volume' | 'mass' | 'speed' | 'pressure' | 'temperature' | 'duration';

export interface UnitDefinition {
  id: string;
  quantity: QuantityKind;
  abbreviation: string;
  /** Linear factor to the quantity's reference unit. Ignored for temperature. */
  factor: number;
}

const unit = (
  quantity: QuantityKind,
  id: string,
  abbreviation: string,
  factor: number,
): UnitDefinition => ({ id, quantity, abbreviation, factor });

export const UNITS = {
  // length → metre
  micrometer: unit('length', 'micrometer', 'µm', 1e-6),
  millimeter: unit('length', 'millimeter', 'mm', 1e-3),
  centimeter: unit('length', 'centimeter', 'cm', 1e-2),
  decimeter: unit('length', 'decimeter', 'dm', 1e-1),
  meter: unit('length', 'meter', 'm', 1),
  kilometer: unit('length', 'kilometer', 'km', 1e3),
  // area → square metre
  squareMillimeter: unit('area', 'squareMillimeter', 'mm²', 1e-6),
  squareCentimeter: unit('area', 'squareCentimeter', 'cm²', 1e-4),
  squareDecimeter: unit('area', 'squareDecimeter', 'dm²', 1e-2),
  squareMeter: unit('area', 'squareMeter', 'm²', 1),
  // volume → cubic metre
  cubicMicrometer: unit('volume', 'cubicMicrometer', 'µm³', 1e-18),
  cubicMillimeter: unit('volume', 'cubicMillimeter', 'mm³', 1e-9),
  cubicCentimeter: unit('volume', 'cubicCentimeter', 'cm³', 1e-6),
  cubicDecimeter: unit('volume', 'cubicDecimeter', 'dm³', 1e-3),
  milliliter: unit('volume', 'milliliter', 'ml', 1e-6),
  liter: unit('volume', 'liter', 'l', 1e-3),
  // mass → kilogram
  milligram: unit('mass', 'milligram', 'mg', 1e-6),
  gram: unit('mass', 'gram', 'g', 1e-3),
  decigram: unit('mass', 'decigram', 'dg', 1e-4),
  kilogram: unit('mass', 'kilogram', 'kg', 1),
  tonne: unit('mass', 'tonne', 't', 1e3),
  // speed → metre per second
  millimeterPerMinute: unit('speed', 'millimeterPerMinute', 'mm/min', 1e-3 / 60),
  centimeterPerMinute: unit('speed', 'centimeterPerMinute', 'cm/min', 1e-2 / 60),
  decimeterPerMinute: unit('speed', 'decimeterPerMinute', 'dm/min', 1e-1 / 60),
  meterPerMinute: unit('speed', 'meterPerMinute', 'm/min', 1 / 60),
  kilometerPerMinute: unit('speed', 'kilometerPerMinute', 'km/min', 1e3 / 60),
  millimeterPerSecond: unit('speed', 'millimeterPerSecond', 'mm/s', 1e-3),
  centimeterPerSecond: unit('speed', 'centimeterPerSecond', 'cm/s', 1e-2),
  decimeterPerSecond: unit('speed', 'decimeterPerSecond', 'dm/s', 1e-1),
  meterPerSecond: unit('speed', 'meterPerSecond', 'm/s', 1),
  kilometerPerSecond: unit('speed', 'kilometerPerSecond', 'km/s', 1e3),
  millimeterPerHour: unit('speed', 'millimeterPerHour', 'mm/h', 1e-3 / 3600),
  centimeterPerHour: unit('speed', 'centimeterPerHour', 'cm/h', 1e-2 / 3600),
  meterPerHour: unit('speed', 'meterPerHour', 'm/h', 1 / 3600),
  kilometerPerHour: unit('speed', 'kilometerPerHour', 'km/h', 1e3 / 3600),
  // pressure → pascal
  millibar: unit('pressure', 'millibar', 'mbar', 1e2),
  bar: unit('pressure', 'bar', 'bar', 1e5),
  kilobar: unit('pressure', 'kilobar', 'kbar', 1e8),
  megabar: unit('pressure', 'megabar', 'Mbar', 1e11),
  millipascal: unit('pressure', 'millipascal', 'mPa', 1e-3),
  pascal: unit('pressure', 'pascal', 'Pa', 1),
  hectopascal: unit('pressure', 'hectopascal', 'hPa', 1e2),
  kilopascal: unit('pressure', 'kilopascal', 'kPa', 1e3),
  megapascal: unit('pressure', 'megapascal', 'MPa', 1e6),
  // temperature (affine, see convert)
  degreeCelsius: unit('temperature', 'degreeCelsius', '°C', 1),
  degreeFahrenheit: unit('temperature', 'degreeFahrenheit', '°F', 1),
  kelvin: unit('temperature', 'kelvin', 'K', 1),
  // duration → second
  millisecond: unit('duration', 'millisecond', 'ms', 1e-3),
  second: unit('duration', 'second', 's', 1),
  hour: unit('duration', 'hour', 'h', 3600),
  day: unit('duration', 'day', 'd', 86400),
} as const satisfies Record<string, UnitDefinition>;

export type UnitId = keyof typeof UNITS;

/** Units offered per quantity, in the order of the v2 pickers. */
export const QUANTITY_UNITS: Record<QuantityKind, readonly UnitId[]> = {
  length: ['micrometer', 'millimeter', 'centimeter', 'decimeter', 'meter', 'kilometer'],
  area: ['squareMillimeter', 'squareCentimeter', 'squareDecimeter', 'squareMeter'],
  volume: [
    'cubicMicrometer',
    'cubicMillimeter',
    'cubicCentimeter',
    'cubicDecimeter',
    'milliliter',
    'liter',
  ],
  mass: ['milligram', 'gram', 'decigram', 'kilogram', 'tonne'],
  speed: [
    'millimeterPerMinute',
    'centimeterPerMinute',
    'decimeterPerMinute',
    'meterPerMinute',
    'kilometerPerMinute',
    'millimeterPerSecond',
    'centimeterPerSecond',
    'decimeterPerSecond',
    'meterPerSecond',
    'kilometerPerSecond',
    'millimeterPerHour',
    'centimeterPerHour',
    'meterPerHour',
    'kilometerPerHour',
  ],
  pressure: [
    'millibar',
    'bar',
    'kilobar',
    'megabar',
    'millipascal',
    'pascal',
    'kilopascal',
    'hectopascal',
    'megapascal',
  ],
  temperature: ['degreeCelsius', 'degreeFahrenheit', 'kelvin'],
  duration: ['millisecond', 'second', 'hour', 'day'],
};

function toKelvin(value: number, from: UnitId): number {
  if (from === 'kelvin') return value;
  if (from === 'degreeFahrenheit') return ((value - 32) * 5) / 9 + 273.15;
  return value + 273.15;
}

function fromKelvin(value: number, to: UnitId): number {
  if (to === 'kelvin') return value;
  if (to === 'degreeFahrenheit') return ((value - 273.15) * 9) / 5 + 32;
  return value - 273.15;
}

/** Converts a value between two units of the same quantity. */
export function convertUnit(value: number, from: UnitId, to: UnitId): number {
  if (from === to) return value;
  const a = UNITS[from];
  const b = UNITS[to];
  if (a.quantity !== b.quantity) {
    throw new Error(`Cannot convert ${a.quantity} (${from}) to ${b.quantity} (${to})`);
  }
  if (a.quantity === 'temperature') return fromKelvin(toKelvin(value, from), to);
  return (value * a.factor) / b.factor;
}

export interface UnitRoundingOptions {
  roundingAccuracy: number;
  roundOnUnitChange: boolean;
}

/** v2 `BaseEntityModel.ConvertValueForUnit`: converts and optionally rounds a nullable value. */
export function convertValueForUnit(
  value: number | null | undefined,
  from: UnitId,
  to: UnitId,
  options: UnitRoundingOptions,
): number | null | undefined {
  if (value === null || value === undefined) return value;
  const converted = convertUnit(value, from, to);
  return options.roundOnUnitChange ? roundTo(converted, options.roundingAccuracy) : converted;
}

/** Base unit of every unit-bearing field (v2 `*BaseUnit` statics). Values are persisted in these. */
export const FIELD_BASE_UNITS = {
  vehicle: { gewicht: 'kilogram' },
  motor: {
    bohrungD: 'millimeter',
    brennraumV: 'cubicMillimeter',
    deachsierungL: 'millimeter',
    hubL: 'millimeter',
    hubraumV: 'cubicMillimeter',
    kolbenG: 'meterPerSecond',
    kurbelgehaeuseV: 'cubicMillimeter',
    pleulL: 'millimeter',
  },
  einlass: {
    breiteB: 'millimeter',
    durchmesserD: 'millimeter',
    flaecheA: 'squareMillimeter',
    hoeheH: 'millimeter',
    laengeL: 'millimeter',
  },
  vergaser: { durchmesserD: 'millimeter' },
  auslass: {
    breiteB: 'millimeter',
    durchmesserD: 'millimeter',
    flaecheA: 'squareMillimeter',
    hoeheH: 'millimeter',
    laengeL: 'millimeter',
  },
  auspuff: {
    abgasT: 'degreeCelsius',
    abgasV: 'meterPerSecond',
    diffusorD: 'millimeter',
    diffusorD1: 'millimeter',
    diffusorD2: 'millimeter',
    diffusorD3: 'millimeter',
    diffusorL: 'millimeter',
    diffusorL1: 'millimeter',
    diffusorL2: 'millimeter',
    diffusorL3: 'millimeter',
    endrohrD: 'millimeter',
    endrohrL: 'millimeter',
    gegenkonusD: 'millimeter',
    gegenkonusL: 'millimeter',
    gesamtL: 'centimeter',
    kruemmerD: 'millimeter',
    kruemmerL: 'millimeter',
    mittelteilD: 'millimeter',
    mittelteilL: 'millimeter',
    resonanzL: 'millimeter',
  },
  ueberstroemer: {
    breiteB: 'millimeter',
    flaecheA: 'squareMillimeter',
    hoeheH: 'millimeter',
  },
  environment: { luftdruckP: 'hectopascal', temperaturT: 'degreeCelsius' },
} as const satisfies Record<string, Record<string, UnitId>>;
