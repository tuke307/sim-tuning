import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import { auslass } from '@sim-tuning/common/logic';
import type { VehicleDto } from '@sim-tuning/common/schemas';
import {
  anwendungDraftFromVehicle,
  buildAnwendungUpdate,
  buildTheorieUpdate,
  computeTheorie,
  getAuspuffViewBox,
  getGesamtLaenge,
  missingAnwendungFields,
  runAnwendung,
  theorieDraftFromVehicle,
  toAuspuffInput,
  type AnwendungDraft,
} from './auslass-helpers';

/** Same inputs as the v2 golden fixture (`v2-parity.spec.ts`). */
const draft: AnwendungDraft = {
  resonanzU: 8000,
  auslassDurchmesserD: 45.83,
  auslassLaengeL: 100,
  auslassSteuerzeitSZ: 190,
  auslassFlaecheA: 20,
  abgasT: 548.69,
  kruemmerF: 10,
  kruemmerW: 2,
  mittelteilF: 2.9,
  diffusorStage: 1,
  diffusorW1: 7.5,
  gegenKonusW: 8,
  endrohrL: 295,
  endrohrD: 27,
};

const vehicle = {
  id: 'veh-1',
  motor: {
    name: 'M 53',
    resonanzU: 8000,
    auslass: {
      durchmesserD: 45.83,
      laengeL: 100,
      steuerzeitSZ: 190,
      flaecheA: 20,
      auspuff: {
        abgasT: 548.69,
        kruemmerD: 30,
        kruemmerF: 10,
        kruemmerW: 2,
        mittelteilF: 2.9,
        diffusorStage: 0,
        diffusorW1: 7.5,
        gegenKonusW: 8,
        endrohrL: 295,
        endrohrD: 27,
      },
    },
  },
} as unknown as VehicleDto;

describe('auslass helpers — Theorie', () => {
  it('prefills from the vehicle and computes every v2 value', () => {
    const theorie = theorieDraftFromVehicle(vehicle);
    assert.equal(theorie.flaecheA, 20);
    assert.equal(theorie.gasT, 548.69);
    const result = computeTheorie(theorie);
    assert.equal(result.kruemmerL, auslass.getKruemmerLaenge(30, 10, 0));
    assert.equal(result.gasV, auslass.getGasGeschwindigkeit(548.69));
    assert.equal(result.resonanzL, auslass.getResonanzLaenge(190, 548.69, 8000));
    // 20 mm² = 0.2 cm² → formula gives cm → displayed in mm.
    const cm = auslass.getKruemmerDurchmesser(0.2, 10);
    assert.ok(result.kruemmerDRange);
    assert.ok(Math.abs(result.kruemmerDRange[0] - cm * 10) < 1e-9);
    assert.ok(
      Math.abs(result.kruemmerDRange[1] - auslass.getKruemmerDurchmesser(0.2, 20) * 10) < 1e-9,
    );
  });

  it('returns nulls for missing inputs and builds the update', () => {
    const empty = computeTheorie(theorieDraftFromVehicle(null));
    assert.deepEqual(empty, { kruemmerL: null, kruemmerDRange: null, gasV: null, resonanzL: null });
    const theorie = theorieDraftFromVehicle(vehicle);
    const result = computeTheorie(theorie);
    const update = buildTheorieUpdate('M 53', theorie, result);
    assert.equal(update.motor?.name, 'M 53');
    assert.equal(update.motor?.auslass?.auspuff?.kruemmerL, result.kruemmerL);
    assert.equal(update.motor?.auslass?.auspuff?.resonanzL, result.resonanzL);
    assert.equal(update.motor?.auslass?.flaecheA, 20);
  });
});

describe('auslass helpers — Anwendung', () => {
  it('maps a stored diffusor stage 0 to stage 1', () => {
    assert.deepEqual(anwendungDraftFromVehicle(vehicle), draft);
    assert.equal(anwendungDraftFromVehicle(null).diffusorStage, 1);
  });

  it('lists missing required fields (angles only for stage 1)', () => {
    const empty = anwendungDraftFromVehicle(null);
    const missing = missingAnwendungFields(empty);
    assert.ok(missing.includes('abgasT'));
    assert.ok(missing.includes('diffusorW1'));
    assert.ok(!missingAnwendungFields({ ...empty, diffusorStage: 2 }).includes('diffusorW1'));
    assert.deepEqual(missingAnwendungFields(draft), []);
    assert.deepEqual(runAnwendung(empty).status, 'incomplete');
  });

  for (const stage of [1, 2, 3] as const) {
    it(`matches calculateAuspuff + geometry for stage ${stage}`, () => {
      const outcome = runAnwendung({ ...draft, diffusorStage: stage });
      assert.equal(outcome.status, 'ok');
      if (outcome.status !== 'ok') return;
      const input = toAuspuffInput({ ...draft, diffusorStage: stage });
      const expected = auslass.calculateAuspuff(input);
      assert.deepEqual(outcome.result, expected);
      assert.deepEqual(outcome.geometry, auslass.getAuspuffGeometry(expected, input));
      if (stage !== 1) {
        assert.equal(outcome.result.diffusorL1, 0);
        assert.equal(outcome.result.diffusorD1, 0);
      }
    });
  }

  it('reports non-finite results as invalid (e.g. 0° angles)', () => {
    assert.equal(runAnwendung({ ...draft, gegenKonusW: 0 }).status, 'invalid');
  });

  it('sums the total length and stores it in cm', () => {
    const outcome = runAnwendung(draft);
    assert.equal(outcome.status, 'ok');
    if (outcome.status !== 'ok') return;
    const r = outcome.result;
    const sum = r.kruemmerL + r.diffusorL1 + r.mittelteilL + r.gegenkonusL + 295;
    assert.ok(Math.abs(getGesamtLaenge(r, 295) - sum) < 0.01);
    const update = buildAnwendungUpdate('M 53', draft, r, outcome.gesamtL);
    const auspuff = update.motor?.auslass?.auspuff;
    assert.equal(auspuff?.mittelteilD, r.mittelteilD);
    assert.equal(auspuff?.diffusorStage, 1);
    assert.ok(Math.abs((auspuff?.gesamtL ?? 0) - outcome.gesamtL / 10) < 1e-3);
  });
});

describe('auslass helpers — drawing', () => {
  it('keeps the v2 canvas when everything fits and grows otherwise', () => {
    const base = {
      width: 2000,
      height: 1000,
      segments: [{ kind: 'frame' as const, x1: 100, y1: 700, x2: 500, y2: 700 }],
      rects: [],
      valueLabels: [],
      partLabels: [],
    };
    assert.deepEqual(getAuspuffViewBox(base), { x: 0, y: 0, width: 2000, height: 1000 });
    const wide = getAuspuffViewBox({
      ...base,
      rects: [{ x: 1800, y: 600, width: 600, height: 50 }],
    });
    assert.equal(wide.x, 0);
    assert.equal(wide.width, 2440);
    assert.equal(wide.height, 1000);
  });
});
