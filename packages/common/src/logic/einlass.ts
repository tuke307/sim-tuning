import { roundTo } from '../utils/round.util';

/** Intake calculations, ported 1:1 from v2 `EinlassLogic` (golden-tested). */

/** Resonance length of the intake tract (v2 formula and scaling kept as-is). */
export function getResonanzLaenge(
  einlassflaeche: number,
  einlasssteuerwinkel: number,
  kurbelgehausevolumen: number,
  resonanzdrehzahl: number,
  ansaugleitungsdurchmesser: number,
): number {
  const laenge =
    einlassflaeche *
    (((3062500 * einlasssteuerwinkel ** 2) / resonanzdrehzahl ** 2) * kurbelgehausevolumen -
      1 / ansaugleitungsdurchmesser) *
    10;
  return roundTo(laenge, 2);
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
