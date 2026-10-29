import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import { roundTo } from '../utils/round.util';
import { drehzahlToGeschwindigkeit, getLeistungskurve, getRadleistung, wattToPs } from './dyno';

const params = {
  masse: 170,
  luftdichte: 1.2,
  cw: 0.8,
  frontA: 0.75,
  rollwiderstand: 0.005,
  steigung: 0,
};

describe('wheel power (v3 physics)', () => {
  it('derives road speed from rpm, gearing and wheel radius', () => {
    // 6000 U/min, i = 14, r = 0.277 m -> 12.43 m/s (44.8 km/h)
    assert.equal(roundTo(drehzahlToGeschwindigkeit(6000, 14, 0.277), 2), 12.43);
  });

  it('sums rolling, aero, slope and inertial forces', () => {
    // v = 10 m/s, a = 1 m/s²: Fr 8.34 N, Fw 36 N, Fa 170 N -> 2143.4 W
    assert.equal(roundTo(getRadleistung(10, 1, params), 1), 2143.4);
    assert.equal(roundTo(getRadleistung(10, 0, { ...params, steigung: 0.1 }), 1), 2111.1);
  });

  it('gives a plausible moped power curve from an rpm run', () => {
    // 3000 -> 7000 U/min in 6 s on a 50 cc moped: a few PS, not 1e14.
    const run = Array.from({ length: 61 }, (_, k) => ({
      zeit: k * 100,
      drehzahl: 3000 + k * 66.67,
    }));
    const curve = getLeistungskurve(run, 14, 0.277, params);
    assert.equal(curve.length, 59);
    const peak = Math.max(...curve.map((p) => wattToPs(p.watt)));
    assert.ok(peak > 1 && peak < 10, `peak ${peak} PS`);
    assert.ok(curve.every((p, i) => i === 0 || p.watt >= curve[i - 1]!.watt));
  });
});
