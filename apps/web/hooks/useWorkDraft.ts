'use client';

import { useCallback, useSyncExternalStore } from 'react';
import {
  diffFromSeed,
  draftDiffers,
  mergeDraft,
  omitKeys,
  workDraftStore,
} from '@/lib/sim/drafts/work-draft';

export interface WorkDraftOptions<T> {
  /**
   * Fields that are stored on the vehicle. Only these drive the "not saved to vehicle" marker and
   * are dropped from the draft once saved. Defaults to all fields.
   */
  compare?: readonly (keyof T)[];
  /** Extra check of a restored draft; when it fails the vehicle seed is used instead. */
  validate?: (draft: T) => boolean;
}

export interface WorkDraft<T> {
  draft: T;
  setDraft: (next: T | ((prev: T) => T)) => void;
  /** The draft differs from the vehicle's stored values (on the compared fields). */
  dirty: boolean;
  /** Drops the draft and re-seeds every field from the vehicle. */
  reset: () => void;
  /**
   * Marks the compared fields as saved: they follow the vehicle again, scratch fields stay. When
   * the values went to another vehicle (starter copy), scratch fields move to `targetKey`.
   * Returns a function that restores the previous draft (used by "Undo").
   */
  commit: (targetKey?: string) => () => void;
}

/**
 * Calculator inputs that survive navigation: kept per `key` (`workDraftKey(vehicleId, panel)`) in
 * `sessionStorage`, seeded from `init()` (the vehicle values). Only fields that differ from the
 * seed are stored, so untouched fields keep following the vehicle.
 */
export function useWorkDraft<T extends object>(
  key: string,
  init: () => T,
  options: WorkDraftOptions<T> = {},
): WorkDraft<T> {
  const { compare, validate } = options;
  const subscribe = useCallback(
    (listener: () => void) => workDraftStore.subscribe(key, listener),
    [key],
  );
  const stored = useSyncExternalStore(
    subscribe,
    () => workDraftStore.read(key),
    () => undefined,
  );

  const seed = init();
  const resolve = (raw: Record<string, unknown> | undefined): T => {
    if (!raw) return seed;
    const merged = mergeDraft(seed, raw);
    return validate && !validate(merged) ? seed : merged;
  };
  const draft = resolve(stored);
  const compareKeys = compare ?? (Object.keys(seed) as (keyof T)[]);

  const setDraft = (next: T | ((prev: T) => T)) => {
    const prev = resolve(workDraftStore.read(key));
    const value = typeof next === 'function' ? (next as (prev: T) => T)(prev) : next;
    workDraftStore.write(key, diffFromSeed(value, seed));
  };

  const reset = () => workDraftStore.write(key, undefined);

  const commit = (targetKey?: string) => {
    const before = workDraftStore.read(key);
    const changed: Partial<T> = diffFromSeed(resolve(before), seed) ?? {};
    const scratch = omitKeys<Partial<T>>(changed, compareKeys);
    workDraftStore.write(key, scratch);
    if (targetKey && targetKey !== key) {
      const beforeTarget = workDraftStore.read(targetKey);
      workDraftStore.write(targetKey, scratch);
      return () => {
        workDraftStore.write(targetKey, beforeTarget);
        workDraftStore.write(key, before);
      };
    }
    return () => workDraftStore.write(key, before);
  };

  return {
    draft,
    setDraft,
    dirty: draftDiffers(draft, seed, compareKeys),
    reset,
    commit,
  };
}
