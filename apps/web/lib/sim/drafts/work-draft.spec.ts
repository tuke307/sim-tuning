import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import {
  createWorkDraftStore,
  diffFromSeed,
  draftDiffers,
  mergeDraft,
  omitKeys,
  parseStoredDraft,
  WORK_DRAFT_STORAGE_PREFIX,
  workDraftKey,
  type DraftStorage,
} from './work-draft';

class MemoryStorage implements DraftStorage {
  readonly map = new Map<string, string>();
  get length() {
    return this.map.size;
  }
  key(index: number) {
    return [...this.map.keys()][index] ?? null;
  }
  getItem(key: string) {
    return this.map.get(key) ?? null;
  }
  setItem(key: string, value: string) {
    this.map.set(key, value);
  }
  removeItem(key: string) {
    this.map.delete(key);
  }
}

const seed = { hubL: 39.5 as number | null, hubraumV: null as number | null, kante: 'unterkante' };

describe('work drafts — parsing and merging', () => {
  it('builds keys per vehicle and panel', () => {
    assert.equal(workDraftKey('veh-1', 'motor.hubraum'), 'veh-1:motor.hubraum');
  });

  it('rejects malformed, non-object and oversized stored data', () => {
    assert.equal(parseStoredDraft(null), undefined);
    assert.equal(parseStoredDraft('{not json'), undefined);
    assert.equal(parseStoredDraft('[1,2]'), undefined);
    assert.equal(parseStoredDraft('42'), undefined);
    assert.equal(parseStoredDraft(JSON.stringify({ a: 'x'.repeat(30_000) })), undefined);
    assert.deepEqual(parseStoredDraft('{"hubL":40}'), { hubL: 40 });
  });

  it('takes only known keys with a matching type', () => {
    assert.deepEqual(mergeDraft(seed, { hubL: 41, hubraumV: 49600, extra: 1 }), {
      hubL: 41,
      hubraumV: 49600,
      kante: 'unterkante',
    });
    assert.deepEqual(mergeDraft(seed, { hubL: 'x', hubraumV: {}, kante: 3 }), seed);
    assert.deepEqual(mergeDraft(seed, { hubL: null }), { ...seed, hubL: null });
    assert.deepEqual(mergeDraft(seed, { kante: null }), seed);
    assert.deepEqual(mergeDraft(seed, { hubL: Number.NaN }), seed);
    assert.equal(mergeDraft(seed, undefined), seed);
  });

  it('merges nested objects recursively', () => {
    const nested = { timings: { einlass: 140 as number | null, auslass: null as number | null } };
    assert.deepEqual(mergeDraft(nested, { timings: { auslass: 170, einlass: 'x' } }), {
      timings: { einlass: 140, auslass: 170 },
    });
  });

  it('detects differences only on the compared keys', () => {
    assert.equal(draftDiffers({ ...seed, kante: 'oberkante' }, seed, ['hubL', 'hubraumV']), false);
    assert.equal(draftDiffers({ ...seed, hubL: 40 }, seed, ['hubL']), true);
    const loose: { hubraumV: number | null | undefined } = { hubraumV: null };
    assert.equal(draftDiffers({ hubraumV: undefined }, loose, ['hubraumV']), false);
  });

  it('keeps only changed fields and can drop applied keys', () => {
    assert.equal(diffFromSeed(seed, { ...seed }), undefined);
    assert.deepEqual(diffFromSeed({ ...seed, hubL: 40, kante: 'oberkante' }, seed), {
      hubL: 40,
      kante: 'oberkante',
    });
    assert.deepEqual(omitKeys({ hubL: 40, kante: 'oberkante' }, ['hubL']), { kante: 'oberkante' });
    assert.equal(omitKeys({ hubL: 40 }, ['hubL']), undefined);
  });
});

describe('work drafts — store', () => {
  it('persists, notifies and removes drafts', () => {
    const storage = new MemoryStorage();
    const store = createWorkDraftStore(() => storage);
    const key = workDraftKey('veh-1', 'motor.hubraum');
    let calls = 0;
    const unsubscribe = store.subscribe(key, () => (calls += 1));

    assert.equal(store.read(key), undefined);
    store.write(key, { hubL: 40 });
    assert.deepEqual(store.read(key), { hubL: 40 });
    assert.equal(storage.getItem(WORK_DRAFT_STORAGE_PREFIX + key), '{"hubL":40}');
    // Snapshots are stable between writes (required by useSyncExternalStore).
    assert.equal(store.read(key), store.read(key));

    store.write(key, undefined);
    assert.equal(store.read(key), undefined);
    assert.equal(storage.getItem(WORK_DRAFT_STORAGE_PREFIX + key), null);
    assert.equal(calls, 2);
    unsubscribe();
    store.write(key, { hubL: 1 });
    assert.equal(calls, 2);
  });

  it('reads drafts written by an earlier page load and ignores garbage', () => {
    const storage = new MemoryStorage();
    storage.setItem(WORK_DRAFT_STORAGE_PREFIX + 'veh-1:a', '{"x":1}');
    storage.setItem(WORK_DRAFT_STORAGE_PREFIX + 'veh-1:b', 'garbage');
    const store = createWorkDraftStore(() => storage);
    assert.deepEqual(store.read('veh-1:a'), { x: 1 });
    assert.equal(store.read('veh-1:b'), undefined);
  });

  it('clears all drafts of one vehicle', () => {
    const storage = new MemoryStorage();
    storage.setItem(WORK_DRAFT_STORAGE_PREFIX + 'veh-1:a', '{"x":1}');
    const store = createWorkDraftStore(() => storage);
    store.write('veh-1:b', { y: 2 });
    store.write('veh-2:a', { z: 3 });
    store.clearVehicle('veh-1');
    assert.equal(store.read('veh-1:a'), undefined);
    assert.equal(store.read('veh-1:b'), undefined);
    assert.deepEqual(store.read('veh-2:a'), { z: 3 });
    assert.deepEqual([...storage.map.keys()], [WORK_DRAFT_STORAGE_PREFIX + 'veh-2:a']);
  });

  it('falls back to memory when storage is unavailable or throws', () => {
    const store = createWorkDraftStore(() => {
      throw new Error('SecurityError');
    });
    store.write('veh-1:a', { x: 1 });
    assert.deepEqual(store.read('veh-1:a'), { x: 1 });
  });
});
