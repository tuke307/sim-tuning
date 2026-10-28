import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import {
  getCompression,
  getCylinderHoleDiameter,
  getDisplacement,
  getDistanceToOT,
  getGrindingDiameters,
  getHubRadius,
  getKolbenDurchmesser,
  getKolbenGeschwindigkeit,
  getPortTimingDifference,
  getSteuerdiagramm,
  getSteuerwinkel,
  getSteuerwinkelOeffnet,
  getSteuerwinkelSchliesst,
  getToDecreasingLength,
  getVorauslass,
} from './engine';
import * as einlass from './einlass';
import * as auslass from './auslass';
import { calculateAuspuff, getAuspuffGeometry } from './auslass';
import { getLeistung } from './dyno';

/**
 * Golden values produced by running the real v2 `SimTuning.Core` (.NET) with the same inputs
 * (see docs/v3-rewrite-plan.md, "numeric parity"). Exact equality is expected: the port keeps
 * .NET banker's rounding and integer-division quirks.
 */
const golden = JSON.parse(
  readFileSync(fileURLToPath(new URL('../__fixtures__/v2-golden.json', import.meta.url)), 'utf8'),
) as Record<string, unknown>;

const g = (key: string) => golden[key];
const close = (actual: number, expected: number, relTol = 1e-12) =>
  assert.ok(
    Math.abs(actual - expected) <= Math.max(1e-9, Math.abs(expected) * relTol),
    `${actual} ≉ ${expected}`,
  );

describe('v2 parity — EngineLogic', () => {
  it('matches compression, bore, displacement and kinematics', () => {
    assert.equal(getCompression(50000, 17000), g('compression'));
    assert.equal(getCompression(49900, 5880), g('compression2'));
    assert.equal(getCylinderHoleDiameter(50000, 17000), g('cylinderHoleDiameter'));
    assert.equal(getCylinderHoleDiameter(49900, 44), g('cylinderHoleDiameter2'));
    assert.equal(getDisplacement(38, 17000), g('displacement'));
    assert.equal(getDisplacement(38, 44), g('displacement2'));
    assert.equal(getDistanceToOT(44, 22, 2, 120), g('distanceToOT'));
    assert.equal(getDistanceToOT(95, 22, 2, 67.5), g('distanceToOT2'));
    assert.equal(getHubRadius(44, 44, 2), g('hubRadius'));
    assert.equal(getHubRadius(44, 95, 2), g('hubRadius2'));
    close(getKolbenDurchmesser(38, 0.03), g('kolbenDurchmesser') as number);
    assert.equal(getKolbenGeschwindigkeit(44, 8000), g('kolbenGeschwindigkeit'));
    const grinding = g('grinding') as Record<string, number>;
    const ours = getGrindingDiameters(38);
    close(ours.diameter1, grinding.Diameter1!);
    close(ours.diameter4, grinding.Diameter4!);
  });

  it('matches port timing helpers', () => {
    assert.equal(getPortTimingDifference(false, 120, 130), g('portTimingDiffDeg'));
    assert.equal(getPortTimingDifference(true, 120, 130, 95, 22, 2), g('portTimingDiffMm'));
    const oben = getSteuerwinkel(120, 130, true, false);
    assert.deepEqual(
      [oben.oeffnetVorher, oben.schliesstVorher, oben.oeffnetNachher, oben.schliesstNachher],
      g('steuerwinkelOben'),
    );
    const unten = getSteuerwinkel(120, 130, false, true);
    assert.deepEqual(
      [unten.oeffnetVorher, unten.schliesstVorher, unten.oeffnetNachher, unten.schliesstNachher],
      g('steuerwinkelUnten'),
    );
    assert.deepEqual(
      [
        getSteuerwinkelOeffnet(135),
        getSteuerwinkelOeffnet(0, 145.5),
        getSteuerwinkelOeffnet(0, 0, 117),
        getSteuerwinkelOeffnet(),
      ],
      g('swOeffnet'),
    );
    assert.deepEqual(
      [
        getSteuerwinkelSchliesst(135),
        getSteuerwinkelSchliesst(0, 145.5),
        getSteuerwinkelSchliesst(0, 0, 117),
        getSteuerwinkelSchliesst(),
      ],
      g('swSchliesst'),
    );
    assert.equal(getToDecreasingLength(50000, 17000, 38, 12), g('toDecreasingLength'));
    assert.equal(getToDecreasingLength(49900, 5880, 38, 10), g('toDecreasingLength2'));
    assert.equal(getVorauslass(190, 125), g('vorauslass'));
    assert.equal(getVorauslass(145, 117.25), g('vorauslass2'));
  });

  it('draws the timing diagram with TDC on top', () => {
    const diagram = getSteuerdiagramm(135, 145, 117);
    assert.equal(diagram.size, 600);
    assert.equal(diagram.lines.length, 6);
    // Intake opens/closes symmetric around TDC: equal y, mirrored x.
    const [open, close] = diagram.lines;
    assert.equal(open!.y2, close!.y2);
    assert.equal(open!.x2 + close!.x2, 2 * diagram.center);
    assert.ok(open!.y2 < diagram.center);
  });
});

describe('v2 parity — EinlassLogic', () => {
  it('matches resonance length and carburettor sizing', () => {
    assert.equal(einlass.getResonanzLaenge(20, 120, 17000, 8000, 20), g('einlassResonanz'));
    assert.equal(einlass.getResonanzLaenge(300, 135, 149700, 5500, 16), g('einlassResonanz2'));
    assert.equal(einlass.getVergaserDurchmesser(50000, 8000, 0.9), g('vergaserD'));
    assert.equal(einlass.getVergaserDurchmesser(49900, 5500, 0.85), g('vergaserD2'));
    assert.equal(einlass.getVergaserHauptduesenDurchmesser(18, 0.95), g('hauptduese'));
    assert.equal(einlass.getVergaserHauptduesenDurchmesser(16.3, 0.9), g('hauptduese2'));
  });
});

describe('v2 parity — AuslassLogic', () => {
  it('matches the single formulas', () => {
    assert.equal(auslass.getGasGeschwindigkeit(550), g('gasV'));
    assert.equal(auslass.getKruemmerDurchmesser(2, 10), g('kruemmerD'));
    assert.equal(auslass.getKruemmerDurchmesser(400, 150), g('kruemmerD2'));
    assert.equal(auslass.getKruemmerLaenge(20, 20, 20), g('kruemmerL'));
    assert.equal(auslass.getResonanzLaenge(180, 550, 8000), g('auslassResonanz'));
    assert.equal(auslass.getVehiclePortDuration(180, 8000), g('portDuration'));
  });

  for (const stage of [1, 2, 3]) {
    it(`matches the expansion chamber for diffusor stage ${stage}`, () => {
      const input = {
        resonanzU: 8000,
        auslass: { laengeL: 100, steuerzeitSZ: 190, durchmesserD: 45.83, flaecheA: 20 },
        auspuff: {
          abgasT: 548.69,
          diffusorStage: stage,
          diffusorW1: 7.5,
          kruemmerF: 10,
          kruemmerW: 2,
          mittelteilF: 2.9,
          gegenKonusW: 8,
          endrohrL: 295,
          endrohrD: 27,
        },
      };
      const result = calculateAuspuff(input);
      const expected = g(`auspuffStage${stage}`) as Record<string, number>;
      for (const [key, value] of Object.entries(expected)) {
        const ours = result[(key[0]!.toLowerCase() + key.slice(1)) as keyof typeof result];
        assert.equal(ours, value, key);
      }
      const geometry = getAuspuffGeometry(result, input);
      assert.equal(geometry.rects.length, 2);
      assert.equal(geometry.partLabels.length, 5);
    });
  }
});

describe('v2 parity — DynoLogic', () => {
  it('matches the power formula', () => {
    close(
      getLeistung(6000, 0.5, 1.2, 0.6, 7.5, 0.28, 120, 0.02, 9.81, 0.6, 0),
      g('leistung') as number,
    );
    close(
      getLeistung(4500, 2, 1.18, 0.8, 9, 0.3, 160, 0.015, 9.81, 0.7, 0.05),
      g('leistung2') as number,
    );
  });
});
