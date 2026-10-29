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

// ─────────────────────────────────────────────────────────────────────────────
// Physically correct wheel power (v3). `getLeistung` above keeps the v2 formula for parity, but
// it multiplies by 60 instead of dividing and uses absolute time as Δt, giving ~10¹⁴ PS.
// ─────────────────────────────────────────────────────────────────────────────

export interface FahrwiderstandParams {
  /** Total mass vehicle + rider in kg. */
  masse: number;
  /** Air density in kg/m³. */
  luftdichte: number;
  /** Drag coefficient c_w. */
  cw: number;
  /** Frontal area in m². */
  frontA: number;
  /** Rolling resistance coefficient. */
  rollwiderstand: number;
  /** Road grade (rise/run, 0 = flat). */
  steigung: number;
  gravity?: number;
}

/** Road speed (m/s) from engine speed (U/min), overall gear ratio and dynamic wheel radius (m). */
export function drehzahlToGeschwindigkeit(
  drehzahl: number,
  gesamtuebersetzung: number,
  radius: number,
): number {
  return (2 * Math.PI * radius * drehzahl) / (60 * gesamtuebersetzung);
}

/**
 * Wheel power in watt: (rolling + aerodynamic + slope + inertial force) · v.
 * `v` in m/s, `a` in m/s².
 */
export function getRadleistung(v: number, a: number, p: FahrwiderstandParams): number {
  const g = p.gravity ?? 9.81;
  const fr = p.rollwiderstand * p.masse * g;
  const fw = 0.5 * p.luftdichte * p.cw * p.frontA * v * v;
  const fs = p.masse * g * p.steigung;
  const fa = p.masse * a;
  return (fr + fw + fs + fa) * v;
}

export interface LeistungsPunkt {
  drehzahl: number;
  watt: number;
}

/**
 * Power curve from an RPM run (`zeit` in ms): speed from the gearing, acceleration by central
 * differences of neighbouring samples. Points without a usable Δt are skipped.
 */
export function getLeistungskurve(
  run: readonly { zeit: number; drehzahl: number }[],
  gesamtuebersetzung: number,
  radius: number,
  params: FahrwiderstandParams,
): LeistungsPunkt[] {
  const out: LeistungsPunkt[] = [];
  for (let i = 1; i < run.length - 1; i++) {
    const prev = run[i - 1]!;
    const next = run[i + 1]!;
    const dt = (next.zeit - prev.zeit) / 1000;
    if (!(dt > 0)) continue;
    const v = drehzahlToGeschwindigkeit(run[i]!.drehzahl, gesamtuebersetzung, radius);
    const a =
      (drehzahlToGeschwindigkeit(next.drehzahl, gesamtuebersetzung, radius) -
        drehzahlToGeschwindigkeit(prev.drehzahl, gesamtuebersetzung, radius)) /
      dt;
    const watt = getRadleistung(v, a, params);
    if (Number.isFinite(watt)) out.push({ drehzahl: run[i]!.drehzahl, watt });
  }
  return out;
}
