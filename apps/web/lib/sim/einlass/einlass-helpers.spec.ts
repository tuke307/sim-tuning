import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import { einlass } from '@sim-tuning/common/logic';
import type { VehicleDto } from '@sim-tuning/common/schemas';
import {
  buildKanalUpdate,
  buildVergaserUpdate,
  computeKanalResonanzLaenge,
  computeVergaser,
  kanalDraftFromVehicle,
  vergaserDraftFromVehicle,
  type KanalDraft,
} from './einlass-helpers';

const vehicle = {
  id: 'veh-1',
  motor: {
    name: 'M 53',
    hubraumV: 49600,
    resonanzU: 5750,
    kurbelgehaeuseV: 148800,
    einlass: {
      flaecheA: 300,
      steuerzeitSZ: 136,
      durchmesserD: 18,
      vergaser: { durchmesserD: null },
    },
  },
} as unknown as VehicleDto;

describe('einlass helpers — Kanal', () => {
  it('prefills the draft from the vehicle (base units)', () => {
    assert.deepEqual(kanalDraftFromVehicle(vehicle), {
      flaecheA: 300,
      steuerzeitSZ: 136,
      kurbelgehaeuseV: 148800,
      resonanzU: 5750,
      durchmesserD: 18,
    });
    assert.deepEqual(kanalDraftFromVehicle(null), {
      flaecheA: null,
      steuerzeitSZ: null,
      kurbelgehaeuseV: null,
      resonanzU: null,
      durchmesserD: null,
    });
  });

  it('converts to cm², cm³ and cm before calling the v2 formula', () => {
    const draft = kanalDraftFromVehicle(vehicle);
    assert.equal(
      computeKanalResonanzLaenge(draft),
      einlass.getResonanzLaenge(3, 136, 148.8, 5750, 1.8),
    );
  });

  it('returns null for incomplete or non-finite input', () => {
    const draft: KanalDraft = { ...kanalDraftFromVehicle(vehicle), durchmesserD: null };
    assert.equal(computeKanalResonanzLaenge(draft), null);
    assert.equal(
      computeKanalResonanzLaenge({ ...kanalDraftFromVehicle(vehicle), resonanzU: 0 }),
      null,
    );
  });

  it('builds a partial nested update with the motor name', () => {
    assert.deepEqual(buildKanalUpdate('M 53', kanalDraftFromVehicle(vehicle)), {
      motor: {
        name: 'M 53',
        kurbelgehaeuseV: 148800,
        resonanzU: 5750,
        einlass: { flaecheA: 300, steuerzeitSZ: 136, durchmesserD: 18 },
      },
    });
  });
});

describe('einlass helpers — Vergaser', () => {
  it('computes bore and main jet with the v2 default factors', () => {
    const draft = vergaserDraftFromVehicle(vehicle);
    const result = computeVergaser(draft);
    const bore = einlass.getVergaserDurchmesser(49.6, 5750);
    assert.equal(result.durchmesserD, bore);
    assert.equal(result.hauptdueseD, einlass.getVergaserHauptduesenDurchmesser(bore));
  });

  it('honours custom resistance factors and falls back to defaults when cleared', () => {
    const custom = computeVergaser({
      hubraumV: 49600,
      resonanzU: 5750,
      vergaserFaktor: 0.8,
      hauptdueseFaktor: 1,
    });
    const bore = einlass.getVergaserDurchmesser(49.6, 5750, 0.8);
    assert.equal(custom.durchmesserD, bore);
    assert.equal(custom.hauptdueseD, einlass.getVergaserHauptduesenDurchmesser(bore, 1));

    const fallback = computeVergaser({
      hubraumV: 49600,
      resonanzU: 5750,
      vergaserFaktor: null,
      hauptdueseFaktor: null,
    });
    assert.deepEqual(fallback, computeVergaser(vergaserDraftFromVehicle(vehicle)));
  });

  it('returns nulls while inputs are missing and saves the bore', () => {
    const empty = computeVergaser(vergaserDraftFromVehicle(null));
    assert.deepEqual(empty, { durchmesserD: null, hauptdueseD: null });
    const draft = vergaserDraftFromVehicle(vehicle);
    const result = computeVergaser(draft);
    assert.deepEqual(buildVergaserUpdate('M 53', draft, result), {
      motor: {
        name: 'M 53',
        hubraumV: 49600,
        resonanzU: 5750,
        einlass: { vergaser: { durchmesserD: result.durchmesserD } },
      },
    });
  });
});
