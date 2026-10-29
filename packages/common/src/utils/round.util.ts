/**
 * Round half to even ("banker's rounding"), matching .NET `Math.Round(double)` which v2 used
 * everywhere. JavaScript's `Math.round` rounds halves up, which would drift from v2 results.
 */
export function roundHalfEven(value: number): number {
  if (!Number.isFinite(value)) return value;
  if (Math.abs(value % 1) === 0.5) return 2 * Math.round(value / 2);
  return Math.round(value);
}

/** .NET `Math.Round(value, digits)` (MidpointRounding.ToEven) semantics. */
export function roundTo(value: number, digits: number): number {
  if (!Number.isFinite(value)) return value;
  if (digits <= 0) return roundHalfEven(value);
  // .NET skips rounding when the value cannot carry more fractional precision.
  if (Math.abs(value) >= 1e16) return value;
  const power10 = 10 ** digits;
  return roundHalfEven(value * power10) / power10;
}

/** C# `(int)x` / `Convert.ToInt32`-free truncation toward zero. */
export const truncate = Math.trunc;

/** C# `Convert.ToInt32(double)`: banker's rounding to an integer. */
export function toInt32(value: number): number {
  return roundHalfEven(value);
}
