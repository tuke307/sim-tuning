import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import { clearAllLocalData, generateId, getDeviceId, idbGet, idbSet } from './offline-store';

describe('offline store (in-memory fallback without IndexedDB)', () => {
  it('keeps a stable device id until local data is cleared', async () => {
    const first = await getDeviceId();
    assert.match(first, /^web-/);
    assert.equal(await getDeviceId(), first);

    await clearAllLocalData();
    assert.notEqual(await getDeviceId(), first);
  });

  it('round-trips key/value entries and wipes them on clear', async () => {
    await idbSet('profile', { name: 'Tuner' });
    assert.deepEqual(await idbGet('profile'), { name: 'Tuner' });

    await clearAllLocalData();
    assert.equal(await idbGet('profile'), null);
  });

  it('prefixes generated ids', () => {
    assert.match(generateId('veh'), /^veh-/);
  });
});
