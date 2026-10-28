import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import { starterVehicles } from '@/lib/data/model';
import {
  draftToInput,
  interpolate,
  isDraftDirty,
  newVehicleInput,
  setDraftField,
  validateDraft,
  vehicleToDraft,
} from './draft';

const s51 = starterVehicles().find((v) => v.systemKey === 'veh-s51b1-4')!;

describe('vehicle draft helpers', () => {
  it('maps a stored vehicle into an editable draft', () => {
    const draft = vehicleToDraft(s51);
    assert.equal(draft.name, s51.name);
    assert.equal(draft.motorName, s51.motor?.name);
    assert.equal(draft.motor.hubL, 44);
    assert.equal(draft.einlass.steuerzeitSZ, 135);
    assert.equal(draft.ueberstroemer.anzahl, null);
    assert.equal(isDraftDirty(draft, s51), false);
  });

  it('updates nested fields immutably and builds a partial update payload', () => {
    const draft = vehicleToDraft(s51);
    const next = setDraftField(draft, 'auslass', 'steuerzeitSZ', 170);
    assert.notEqual(next, draft);
    assert.notEqual(draft.auslass.steuerzeitSZ, 170);
    assert.equal(isDraftDirty(next, s51), true);
    const input = draftToInput({ ...next, beschreibung: '   ' });
    assert.equal(input.beschreibung, null);
    assert.equal(input.motor?.auslass?.steuerzeitSZ, 170);
    assert.equal(input.motor?.hubL, 44);
    assert.equal(input.motor?.name, s51.motor?.name);
  });

  it('validates required names', () => {
    const draft = vehicleToDraft(s51);
    assert.equal(validateDraft(draft), null);
    assert.equal(validateDraft({ ...draft, name: ' ' }), 'nameRequired');
    assert.equal(validateDraft({ ...draft, motorName: '' }), 'motorNameRequired');
  });

  it('builds a create payload with the engine named after the vehicle by default', () => {
    assert.deepEqual(newVehicleInput({ name: ' My S51 ', beschreibung: '' }), {
      name: 'My S51',
      beschreibung: null,
      gewicht: null,
      motor: { name: 'My S51' },
    });
    assert.equal(newVehicleInput({ name: 'A', motorName: 'M 541' }).motor?.name, 'M 541');
  });

  it('interpolates placeholders', () => {
    assert.equal(interpolate('Delete "{name}"?', { name: 'S51' }), 'Delete "S51"?');
    assert.equal(interpolate('{missing}', {}), '{missing}');
  });
});
