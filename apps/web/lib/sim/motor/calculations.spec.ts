import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import {
  getCompression,
  getCylinderHoleDiameter,
  getDistanceToOT,
  getHubRadius,
  getSteuerdiagramm,
  getToDecreasingLength,
} from '@sim-tuning/common/logic';
import {
  computeDistanceToOT,
  computeHubradius,
  computeHubraum,
  computePortTimingChange,
  computeSteuerzeiten,
  computeVerdichtung,
  PORT_TIMING_PRESETS,
  steuerkanteForPort,
} from './calculations';

describe('computeHubraum', () => {
  it('derives bore, piston and oversize diameters via the v2 formulas', () => {
    const result = computeHubraum({ hubL: 39.5, hubraumV: 49600, einbauspielMm: 0.03 });
    assert.ok(result);
    assert.equal(result.bohrungD, getCylinderHoleDiameter(49600, 39.5));
    assert.ok(Math.abs(result.bohrungD - 39.99) < 0.02);
    assert.ok(Math.abs(result.kolbenD - (result.bohrungD - 0.03)) < 1e-9);
    assert.ok(Math.abs(result.grinding.diameter1 - (result.bohrungD + 0.2)) < 1e-9);
    assert.ok(Math.abs(result.grinding.diameter4 - (result.bohrungD + 0.8)) < 1e-9);
  });

  it('falls back to the default clearance and rejects missing inputs', () => {
    const result = computeHubraum({ hubL: 42, hubraumV: 47600, einbauspielMm: null });
    assert.ok(result);
    assert.ok(Math.abs(result.kolbenD - (result.bohrungD - 0.03)) < 1e-9);
    assert.equal(computeHubraum({ hubL: null, hubraumV: 47600, einbauspielMm: 0.03 }), null);
    assert.equal(computeHubraum({ hubL: 42, hubraumV: 0, einbauspielMm: 0.03 }), null);
  });
});

describe('computeVerdichtung', () => {
  it('computes current compression and machining length', () => {
    const r = computeVerdichtung({
      hubraumV: 49600,
      brennraumV: 5000,
      bohrungD: 40,
      zielVerdichtung: 11,
    });
    assert.equal(r.current, getCompression(49600, 5000));
    assert.equal(r.abdrehen, getToDecreasingLength(49600, 5000, 40, 11));
  });

  it('returns nulls for incomplete input', () => {
    const r = computeVerdichtung({
      hubraumV: 49600,
      brennraumV: null,
      bohrungD: 40,
      zielVerdichtung: 11,
    });
    assert.deepEqual(r, { current: null, abdrehen: null });
    const noTarget = computeVerdichtung({
      hubraumV: 49600,
      brennraumV: 5000,
      bohrungD: 40,
      zielVerdichtung: null,
    });
    assert.equal(noTarget.abdrehen, null);
    assert.notEqual(noTarget.current, null);
  });
});

describe('computeSteuerzeiten', () => {
  it('matches the v2 angles, Vorauslass and diagram geometry', () => {
    const r = computeSteuerzeiten({ einlass: 136, auslass: 148, ueberstroemer: 122 });
    assert.deepEqual(r.angles.einlass, { steuerzeit: 136, oeffnet: 292, schliesst: 68 });
    assert.deepEqual(r.angles.auslass, { steuerzeit: 148, oeffnet: 106, schliesst: 254 });
    assert.deepEqual(r.angles.ueberstroemer, { steuerzeit: 122, oeffnet: 119, schliesst: 241 });
    assert.equal(r.vorauslass, 13);
    assert.deepEqual(r.diagram, getSteuerdiagramm(136, 148, 122));
  });

  it('only draws lines for ports that have a timing', () => {
    const r = computeSteuerzeiten({ einlass: null, auslass: 160, ueberstroemer: undefined });
    assert.equal(r.angles.einlass, null);
    assert.equal(r.vorauslass, null);
    assert.ok(r.diagram);
    assert.deepEqual(new Set(r.diagram.lines.map((l) => l.port)), new Set(['auslass']));
    assert.equal(
      computeSteuerzeiten({ einlass: 0, auslass: null, ueberstroemer: 400 }).diagram,
      null,
    );
  });

  it('exposes the v2 port timing presets', () => {
    assert.equal(PORT_TIMING_PRESETS.length, 5);
    assert.deepEqual(PORT_TIMING_PRESETS[2], {
      key: 'mittel',
      einlass: 140,
      auslass: 165,
      ueberstroemer: 120,
    });
  });
});

describe('Umrechnung helpers', () => {
  it('computes crank radius and distance to TDC', () => {
    const hubradius = computeHubradius({ hubL: 44, pleulL: 85, deachsierungL: null });
    assert.equal(hubradius, getHubRadius(44, 85, 0));
    assert.equal(hubradius, 22);
    assert.equal(
      computeDistanceToOT({ pleulL: 85, hubradius, deachsierungL: 0, degree: 90 }),
      getDistanceToOT(85, 22, 0, 90),
    );
    assert.equal(computeHubradius({ hubL: 44, pleulL: null, deachsierungL: 0 }), null);
    assert.equal(computeHubradius({ hubL: 44, pleulL: 20, deachsierungL: 0 }), null);
  });

  it('computes port timing change in degrees and millimetres', () => {
    const r = computePortTimingChange({
      vorher: 160,
      nachher: 170,
      kante: steuerkanteForPort('auslass'),
      pleulL: 85,
      hubradius: 22,
      deachsierungL: 0,
    });
    assert.ok(r);
    assert.deepEqual(r.winkel, {
      oeffnetVorher: 100,
      schliesstVorher: 260,
      oeffnetNachher: 95,
      schliesstNachher: 265,
    });
    assert.equal(r.differenceDegree, 10);
    const expected = Math.abs(getDistanceToOT(85, 22, 0, 95) - getDistanceToOT(85, 22, 0, 100));
    assert.ok(r.differenceLength !== null && Math.abs(r.differenceLength - expected) < 0.011);
    assert.equal(steuerkanteForPort('einlass'), 'oberkante');
    const noGeometry = computePortTimingChange({
      vorher: 160,
      nachher: 170,
      kante: 'unterkante',
      pleulL: null,
      hubradius: null,
      deachsierungL: null,
    });
    assert.equal(noGeometry?.differenceLength, null);
  });
});
