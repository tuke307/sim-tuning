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
 * .NET banker's rounding. Formulas that v2 got wrong (intake/exhaust resonance length, speed of
 * sound, header diameter, displacement) are pinned to the literature instead — see the
 * "literature" suite below and docs/extracted/AUDIT-logic-vs-literature.md.
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
  it('matches carburettor sizing', () => {
    assert.equal(einlass.getVergaserDurchmesser(50000, 8000, 0.9), g('vergaserD'));
    assert.equal(einlass.getVergaserDurchmesser(49900, 5500, 0.85), g('vergaserD2'));
    assert.equal(einlass.getVergaserHauptduesenDurchmesser(18, 0.95), g('hauptduese'));
    assert.equal(einlass.getVergaserHauptduesenDurchmesser(16.3, 0.9), g('hauptduese2'));
  });
});

describe('v2 parity — AuslassLogic', () => {
  it('matches the single formulas', () => {
    assert.equal(auslass.getKruemmerLaenge(20, 20, 20), g('kruemmerL'));
    assert.equal(auslass.getVehiclePortDuration(180, 8000), g('portDuration'));
  });

  for (const stage of [1, 2, 3]) {
    it(`matches the expansion chamber for diffusor stage ${stage}`, () => {
      const input = {
        resonanzU: 8000,
        auslass: { laengeL: 100, steuerzeitSZ: 190, durchmesserD: 45.83, flaecheA: 20 },
        auspuff: {
          // v2 users typed the speed of sound (548.69 m/s) into this field; 500 °C gives exactly
          // that speed of sound, so the v2 golden chamber still applies.
          abgasT: 500,
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

describe('literature — formulas v2 got wrong', () => {
  it('displacement π·d²/4·h', () => {
    assert.equal(getDisplacement(38, 44), 49901.1); // S 51: 49,8 cm³ (Werkstatthandbuch)
    assert.equal(getDisplacement(45, 44), 69979); // S 70: 70 cm³
  });

  it('intake resonance length l = 3 062 500·φ²·F / (n²·V)', () => {
    // Rechensheet-2Takt `Berechnung!O42`: F = 10 cm², φ = 360°, n = 9 500, V = 290 cm³ → 151.65 cm
    assert.equal(einlass.getResonanzLaenge(10, 360, 290, 9500), 1516.48);
    assert.equal(einlass.getResonanzLaenge(3, 135, 149.7, 5500), 369.76);
  });

  it('speed of sound c = √(1.32·295·T) in the exhaust gas', () => {
    // Rechensheet-2Takt `Resonanzkörper!F24` at 500 °C: 548.69 m/s (guide 540–560 m/s)
    assert.equal(auslass.getGasGeschwindigkeit(500), 548.69);
    assert.equal(auslass.getGasGeschwindigkeit(550), 566.16);
  });

  it('exhaust resonance length l = c·φ / (12·n)', () => {
    // Rechensheet-2Takt `Resonanzkörper!F29`: φ = 190°, n = 8 000, 500 °C → 1 085.96 mm
    assert.equal(auslass.getResonanzLaenge(190, 548.69, 8000), 1085.95);
    assert.equal(auslass.getResonanzLaengeFromTemperatur(190, 500, 8000), 1085.95);
  });

  it('header diameter from 1.1–1.5 × port area', () => {
    // Rechensheet-2Takt `Resonanzkörper!I25/I27`: port 1 100 mm² → 39.25 / 45.83 mm
    assert.equal(auslass.getKruemmerDurchmesser(1100, 10), 39.25);
    assert.equal(auslass.getKruemmerDurchmesser(1100, 50), 45.83);
  });
});
