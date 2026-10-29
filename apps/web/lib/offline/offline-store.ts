export type SyncState =
  'SYNCED' | 'SAVED_LOCALLY' | 'SYNCING' | 'CONFLICT' | 'AUTH_EXPIRED' | 'OFFLINE';

const DB_NAME = 'sim_tuning_offline_v1';
const DB_VERSION = 1;

const STORES = {
  KV: 'kv_store',
} as const;

const memoryKv = new Map<string, unknown>();

function hasIndexedDb(): boolean {
  return typeof window !== 'undefined' && typeof window.indexedDB !== 'undefined';
}

function openDatabase(): Promise<IDBDatabase> {
  return new Promise((resolve, reject) => {
    const req = window.indexedDB.open(DB_NAME, DB_VERSION);
    req.onupgradeneeded = () => {
      const db = req.result;
      if (!db.objectStoreNames.contains(STORES.KV)) {
        db.createObjectStore(STORES.KV);
      }
    };
    req.onsuccess = () => resolve(req.result);
    req.onerror = () => reject(req.error);
  });
}

export async function idbGet<T>(key: string): Promise<T | null> {
  if (!hasIndexedDb()) {
    return (memoryKv.get(key) as T) ?? null;
  }
  const db = await openDatabase();
  return new Promise((resolve, reject) => {
    const tx = db.transaction(STORES.KV, 'readonly');
    const req = tx.objectStore(STORES.KV).get(key);
    req.onsuccess = () => resolve((req.result as T) ?? null);
    req.onerror = () => reject(req.error);
  });
}

export async function idbSet<T>(key: string, value: T): Promise<void> {
  memoryKv.set(key, value);
  if (!hasIndexedDb()) return;
  const db = await openDatabase();
  return new Promise((resolve, reject) => {
    const tx = db.transaction(STORES.KV, 'readwrite');
    const req = tx.objectStore(STORES.KV).put(value, key);
    req.onsuccess = () => resolve();
    req.onerror = () => reject(req.error);
  });
}

/** Stable random id of this browser install, sent with every sync so the server can recognise
 * its own retried writes. */
export async function getDeviceId(): Promise<string> {
  const existing = await idbGet<string>('device_id');
  if (existing) return existing;
  const id = generateId('web');
  await idbSet('device_id', id);
  return id;
}

export async function clearAllLocalData(): Promise<void> {
  memoryKv.clear();

  if (!hasIndexedDb()) return;
  const db = await openDatabase();
  await new Promise<void>((resolve, reject) => {
    const tx = db.transaction([STORES.KV], 'readwrite');
    tx.objectStore(STORES.KV).clear();
    tx.oncomplete = () => resolve();
    tx.onerror = () => reject(tx.error);
  });
}

export function generateId(prefix = 'loc'): string {
  if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
    return `${prefix}-${crypto.randomUUID()}`;
  }
  return `${prefix}-${Date.now().toString(36)}-${Math.random().toString(36).slice(2, 9)}`;
}
