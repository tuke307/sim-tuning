import { describe, it, beforeEach } from 'node:test';
import assert from 'node:assert/strict';
import { clearAllLocalData } from '@/lib/offline/offline-store';
import { LocalRepository } from './local-repository';
import { isGuestDataEmpty, migrateGuestData } from './guest-migration';

describe('guest data migration', () => {
  beforeEach(async () => {
    await clearAllLocalData();
  });

  it('copies own vehicles, environments and dyno series into the target account', async () => {
    const guest = new LocalRepository();
    const env = await guest.createEnvironment({
      name: 'Rennstrecke',
      temperaturT: 18,
      luftdruckP: 990,
    });
    const vehicle = await guest.createVehicle({
      name: 'S51 Tuning',
      motor: { name: 'M 541', hubL: 44 },
    });
    const dyno = await guest.saveDyno(vehicle.id, { name: 'Lauf', environmentId: env.id });
    await guest.saveDynoData(dyno.id, {
      drehzahl: [{ zeit: 0, drehzahl: 4000 }],
      ausrollen: [{ latitude: 51, longitude: 12, speed: 10 }],
    });
    const snapshot = await guest.snapshotOwnData();
    assert.equal(isGuestDataEmpty(snapshot), false);

    // A second, empty local repository stands in for the account (same contract as the API).
    await clearAllLocalData();
    const account = new LocalRepository();
    const result = await migrateGuestData(snapshot, account);
    assert.deepEqual(result, { vehicles: 1, environments: 1, dynos: 1 });

    const migrated = (await account.listVehicles()).find((v) => v.name === 'S51 Tuning')!;
    assert.equal(migrated.motor?.hubL, 44);
    const migratedDyno = await account.getDyno(migrated.id);
    assert.equal(migratedDyno?.drehzahl[0]?.drehzahl, 4000);
    assert.equal(migratedDyno?.ausrollen[0]?.speed, 10);
    const migratedEnv = (await account.listEnvironments()).find((e) => e.name === 'Rennstrecke');
    assert.equal(migratedDyno?.environmentId, migratedEnv?.id);

    await account.clearOwnData();
    assert.equal(isGuestDataEmpty(await account.snapshotOwnData()), true);
  });
});
