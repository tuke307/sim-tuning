import { roundTo } from '../utils/round.util';

/** Intake calculations, ported from v2 `EinlassLogic` (golden-tested unless noted). */

/**
 * Resonance length of the intake tract in mm, from `n = 1750·φ / √(V·l / F)` (Rieck; Bitterli
 * §5.2; Rechensheet-2Takt `Berechnung!O42`) solved for `l = 3 062 500·φ²·F / (n²·V)` [cm].
 * `F` = mean intake cross-section (cm²), `φ` = intake timing (°KW), `V` = crankcase volume
 * (cm³), `n` = resonance rpm. v2 multiplied by `V` and subtracted `1/d`, giving lengths ~10⁵× too
 * long; that formula is deliberately not kept.
 */
export function getResonanzLaenge(
  einlassflaeche: number,
  einlasssteuerwinkel: number,
  kurbelgehausevolumen: number,
  resonanzdrehzahl: number,
): number {
  const laengeCm =
    (3062500 * einlasssteuerwinkel ** 2 * einlassflaeche) /
    (resonanzdrehzahl ** 2 * kurbelgehausevolumen);
  return roundTo(laengeCm * 10, 2);
}

/** Carburettor bore: `k · √(Vh/1000 · n)`, default resistance factor 0.9. */
export function getVergaserDurchmesser(
  hubvolumen: number,
  resonanzdrehzahl: number,
  widerstandsFaktor = 0.9,
): number {
  return roundTo(widerstandsFaktor * Math.sqrt((hubvolumen / 1000) * resonanzdrehzahl), 2);
}

/** Main jet size: `carburettor bore · 5 · k`, rounded to an integer, default k = 0.95. */
export function getVergaserHauptduesenDurchmesser(
  vergasergroesse: number,
  widerstandsFaktor = 0.95,
): number {
  return roundTo(vergasergroesse * 5 * widerstandsFaktor, 0);
}
