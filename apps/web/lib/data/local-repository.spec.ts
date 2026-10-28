import { describe, it, beforeEach } from 'node:test';
import assert from 'node:assert/strict';
import { STARTER_VEHICLES } from '@sim-tuning/common/catalog';
import { DynoExportSchema } from '@sim-tuning/common/schemas';
import { clearAllLocalData } from '@/lib/offline/offline-store';
import { LocalRepository } from './local-repository';
import { ReadOnlyVehicleError } from './repository';

describe('LocalRepository (guest mode, in-memory fallback)', () => {
  beforeEach(async () => {
    await clearAllLocalData();
  });

  it('lists the v2 starter vehicles first, fully nested and read-only', async () => {
    const repo = new LocalRepository();
    const vehicles = await repo.listVehicles();
    assert.equal(vehicles.length, STARTER_VEHICLES.length);
    const s51 = vehicles.find((v) => v.systemKey === 'veh-s51b1-4')!;
    assert.equal(s51.isSystem, true);
    assert.equal(s51.motor?.hubL, 44);
    assert.equal(s51.motor?.einlass?.steuerzeitSZ, 135);
    assert.ok(s51.motor?.einlass?.vergaser);
    assert.equal(s51.motor?.auslass?.auspuff?.diffusorStage, 0);
    await assert.rejects(repo.updateVehicle(s51.id, { gewicht: 1 }), ReadOnlyVehicleError);
    assert.equal((await repo.listEnvironments()).length, 4);
  });

  it('duplicates, partially updates and deletes own vehicles', async () => {
    const repo = new LocalRepository();
    const [starter] = await repo.listVehicles();
    const copy = await repo.duplicateVehicle(starter!.id);
    assert.equal(copy.isSystem, false);
    assert.equal(copy.motor?.hubL, starter!.motor?.hubL);

    const updated = await repo.updateVehicle(copy.id, {
      motor: { name: 'Tuned', auslass: { auspuff: { abgasT: 550 } } },
    });
    assert.equal(updated.motor?.name, 'Tuned');
    assert.equal(updated.motor?.auslass?.auspuff?.abgasT, 550);
    assert.equal(updated.motor?.hubL, starter!.motor?.hubL, 'untouched fields stay');

    await repo.deleteVehicle(copy.id);
    assert.equal(await repo.getVehicle(copy.id), null);
  });

  it('stores dyno series and round-trips the JSON export', async () => {
    const repo = new LocalRepository();
    const vehicle = await repo.createVehicle({ name: 'Mein S51', motor: { name: 'M 541' } });
    const dyno = await repo.saveDyno(vehicle.id, { name: 'Lauf 1' });
    await repo.saveDynoData(dyno.id, {
      drehzahl: [
        { zeit: 0, drehzahl: 3000 },
        { zeit: 5, drehzahl: 3100 },
      ],
    });
    const exported = DynoExportSchema.parse(await repo.exportDyno(dyno.id));
    assert.equal(exported.dyno.drehzahl?.length, 2);

    const imported = await repo.importDyno(exported);
    assert.notEqual(imported.id, vehicle.id);
    const importedDyno = await repo.getDyno(imported.id);
    assert.deepEqual(importedDyno?.drehzahl, exported.dyno.drehzahl);
    assert.equal((await repo.getVehicle(vehicle.id))?.dynoId, dyno.id);
  });
});
