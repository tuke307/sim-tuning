/**
 * Per-vehicle work drafts of the calculator panels (Motor, Einlass, Auslass).
 *
 * Values typed into a calculator survive sub-tab, module and vehicle switches and reloads within
 * the browser session. Drafts live in `sessionStorage` (calculator inputs only, never secrets) and
 * are keyed by `${vehicleId}:${panel}`. Stored data is untrusted: it is parsed defensively and
 * merged onto a fresh seed from the vehicle, so unknown or mistyped fields fall back to the seed.
 */

/** Prefix of all draft entries in `sessionStorage`. */
export const WORK_DRAFT_STORAGE_PREFIX = 'sim_tuning_draft:';

/** Upper bound for one stored draft; anything larger is not a calculator draft. */
const MAX_RAW_LENGTH = 20_000;

/** Minimal `Storage` subset, so the store can run against a fake in unit tests. */
export interface DraftStorage {
  getItem(key: string): string | null;
  setItem(key: string, value: string): void;
  removeItem(key: string): void;
  readonly length: number;
  key(index: number): string | null;
}

export function workDraftKey(vehicleId: string, panel: string): string {
  return `${vehicleId}:${panel}`;
}

function isPlainObject(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}

/** Parses a stored draft; returns `undefined` for missing, oversized or malformed data. */
export function parseStoredDraft(raw: string | null): Record<string, unknown> | undefined {
  if (raw === null || raw.length > MAX_RAW_LENGTH) return undefined;
  try {
    const parsed: unknown = JSON.parse(raw);
    return isPlainObject(parsed) ? parsed : undefined;
  } catch {
    return undefined;
  }
}

/** Whether `value` may replace `seedValue` (same primitive kind; numbers must be finite). */
function acceptsValue(seedValue: unknown, value: unknown): boolean {
  if (value === null) return seedValue === null || typeof seedValue !== 'string';
  if (typeof value === 'number') {
    return Number.isFinite(value) && (seedValue === null || typeof seedValue === 'number');
  }
  if (typeof value === 'string' || typeof value === 'boolean') {
    return typeof seedValue === typeof value;
  }
  return false;
}

/**
 * Merges a stored draft onto the seed: only keys known to the seed are taken, nested plain
 * objects are merged recursively, everything with an unexpected type keeps the seed value.
 */
export function mergeDraft<T extends object>(seed: T, stored: unknown): T {
  if (!isPlainObject(stored)) return seed;
  const result: Record<string, unknown> = { ...(seed as Record<string, unknown>) };
  for (const [key, seedValue] of Object.entries(seed as Record<string, unknown>)) {
    if (!Object.prototype.hasOwnProperty.call(stored, key)) continue;
    const value = stored[key];
    if (isPlainObject(seedValue)) {
      result[key] = mergeDraft(seedValue, value);
    } else if (acceptsValue(seedValue === undefined ? null : seedValue, value)) {
      result[key] = value;
    }
  }
  return result as T;
}

const same = (a: unknown, b: unknown): boolean =>
  JSON.stringify(a ?? null) === JSON.stringify(b ?? null);

/** Whether any of `keys` differs between draft and seed (`null` and `undefined` are equal). */
export function draftDiffers<T extends object>(
  draft: T,
  seed: T,
  keys: readonly (keyof T)[],
): boolean {
  return keys.some((key) => !same(draft[key], seed[key]));
}

/**
 * Only the top-level fields that differ from the seed (or `undefined` when none do). Storing the
 * difference keeps untouched fields following the vehicle when it changes elsewhere.
 */
export function diffFromSeed<T extends object>(draft: T, seed: T): Partial<T> | undefined {
  const diff: Partial<T> = {};
  for (const key of Object.keys(draft) as (keyof T)[]) {
    if (!same(draft[key], seed[key])) diff[key] = draft[key];
  }
  return Object.keys(diff).length > 0 ? diff : undefined;
}

/** Copy of `draft` without `keys` (or `undefined` when nothing is left). */
export function omitKeys<T extends object>(
  draft: T,
  keys: readonly (keyof T)[],
): Partial<T> | undefined {
  const rest: Partial<T> = { ...draft };
  for (const key of keys) delete rest[key];
  return Object.keys(rest).length > 0 ? rest : undefined;
}

/** Reactive key → draft store backed by a `DraftStorage` (or memory only when unavailable). */
export function createWorkDraftStore(getStorage: () => DraftStorage | null) {
  const cache = new Map<string, Record<string, unknown> | undefined>();
  const listeners = new Map<string, Set<() => void>>();

  const storage = (): DraftStorage | null => {
    try {
      return getStorage();
    } catch {
      return null;
    }
  };

  const notify = (key: string) => listeners.get(key)?.forEach((listener) => listener());

  function read(key: string): Record<string, unknown> | undefined {
    if (cache.has(key)) return cache.get(key);
    let value: Record<string, unknown> | undefined;
    try {
      value = parseStoredDraft(storage()?.getItem(WORK_DRAFT_STORAGE_PREFIX + key) ?? null);
    } catch {
      value = undefined;
    }
    cache.set(key, value);
    return value;
  }

  function write(key: string, value: object | undefined): void {
    const next = value === undefined ? undefined : { ...(value as Record<string, unknown>) };
    cache.set(key, next);
    try {
      const s = storage();
      if (next === undefined) s?.removeItem(WORK_DRAFT_STORAGE_PREFIX + key);
      else s?.setItem(WORK_DRAFT_STORAGE_PREFIX + key, JSON.stringify(next));
    } catch {
      // Storage full or blocked: the draft still lives in memory for this page.
    }
    notify(key);
  }

  function subscribe(key: string, listener: () => void): () => void {
    let set = listeners.get(key);
    if (!set) {
      set = new Set();
      listeners.set(key, set);
    }
    set.add(listener);
    return () => {
      set.delete(listener);
    };
  }

  /** Removes every draft of one vehicle, e.g. after the vehicle was deleted. */
  function clearVehicle(vehicleId: string): void {
    const prefix = workDraftKey(vehicleId, '');
    const keys = new Set([...cache.keys()].filter((key) => key.startsWith(prefix)));
    const s = storage();
    try {
      if (s) {
        for (let i = 0; i < s.length; i += 1) {
          const full = s.key(i);
          if (full?.startsWith(WORK_DRAFT_STORAGE_PREFIX + prefix)) {
            keys.add(full.slice(WORK_DRAFT_STORAGE_PREFIX.length));
          }
        }
      }
    } catch {
      // Ignore unreadable storage; cached keys are still cleared.
    }
    keys.forEach((key) => write(key, undefined));
  }

  return { read, write, subscribe, clearVehicle };
}

export type WorkDraftStore = ReturnType<typeof createWorkDraftStore>;

/** Process-wide store on `sessionStorage` (memory only during SSR or when storage is blocked). */
export const workDraftStore: WorkDraftStore = createWorkDraftStore(() =>
  typeof window === 'undefined' ? null : window.sessionStorage,
);
