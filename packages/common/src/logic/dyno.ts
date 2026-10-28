/** Dyno calculations, ported 1:1 from v2 `DynoLogic` (golden-tested). */

/**
 * Wheel power from a speed sample. Parameters as in v2: `n` engine speed (U/min), `t` time (s),
 * `p` air density, `cw` drag coefficient, `i` gear ratio, `r` wheel radius (m), `m` mass (kg),
 * `c` rolling resistance, `g` gravity, `aw` frontal area (m²), `s` slope.
 *
 * v2 quirks kept: the aero term uses km/h, a zero slope is treated as `1`, and the result is
 * returned in watts even though v2 also computed PS.
 */
export function getLeistung(
  n: number,
  t: number,
  p: number,
  cw: number,
  i: number,
  r: number,
  m: number,
  c: number,
  g: number,
  aw: number,
  s: number,
): number {
  const v = getGeschwindigkeit(r, n, i);
  const a = v / t;
  const fr = c * m * g;
  const fa = m * a;
  const fw = (p / 2) * cw * aw * (v * 3.6) ** 2;
  const fs = m * g * (s === 0 ? 1 : s);
  const leistung = (fr + fw + fs + fa) * v;
  return ((leistung * 1000) / n) * 2 * Math.PI * (n / 60);
}

/** Watt → PS (metric horsepower). */
export function wattToPs(watt: number): number {
  return watt / 735.49875;
}

function getGeschwindigkeit(r: number, n: number, i: number): number {
  return 2 * r * Math.PI * 60 * (n / i);
}
