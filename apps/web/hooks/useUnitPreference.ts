'use client';

import { useCallback, useSyncExternalStore } from 'react';
import { QUANTITY_UNITS, UNITS, type QuantityKind, type UnitId } from '@sim-tuning/common/units';

/** `localStorage` prefix of the chosen display unit per quantity (length, volume, …). */
const STORAGE_PREFIX = 'sim_tuning_unit:';

const listeners = new Set<() => void>();
const cache = new Map<QuantityKind, string | null>();

function read(quantity: QuantityKind): string | null {
  if (cache.has(quantity)) return cache.get(quantity) ?? null;
  let value: string | null;
  try {
    value = window.localStorage.getItem(STORAGE_PREFIX + quantity);
  } catch {
    value = null;
  }
  cache.set(quantity, value);
  return value;
}

function write(quantity: QuantityKind, unit: UnitId): void {
  cache.set(quantity, unit);
  try {
    window.localStorage.setItem(STORAGE_PREFIX + quantity, unit);
  } catch {
    // Storage blocked: the choice still applies for this page.
  }
  listeners.forEach((listener) => listener());
}

function subscribe(listener: () => void): () => void {
  listeners.add(listener);
  // Keep tabs in sync: another tab changed a unit choice.
  const onStorage = (event: StorageEvent) => {
    if (event.key === null || event.key.startsWith(STORAGE_PREFIX)) {
      cache.clear();
      listener();
    }
  };
  window.addEventListener('storage', onStorage);
  return () => {
    listeners.delete(listener);
    window.removeEventListener('storage', onStorage);
  };
}

/**
 * Display unit for fields stored in `baseUnit`, shared by all fields of the same quantity and
 * remembered in `localStorage`. The server snapshot is the base unit, so hydration matches.
 */
export function useUnitPreference(
  baseUnit: UnitId | undefined,
): [UnitId | undefined, (unit: UnitId) => void] {
  const quantity = baseUnit ? UNITS[baseUnit].quantity : null;
  const stored = useSyncExternalStore(
    subscribe,
    () => (quantity ? read(quantity) : null),
    () => null,
  );
  const allowed: readonly string[] = quantity ? QUANTITY_UNITS[quantity] : [];
  // Stored values are untrusted: only units of this quantity are accepted.
  const unit = stored !== null && allowed.includes(stored) ? (stored as UnitId) : baseUnit;
  const setUnit = useCallback(
    (next: UnitId) => {
      if (quantity && (QUANTITY_UNITS[quantity] as readonly string[]).includes(next)) {
        write(quantity, next);
      }
    },
    [quantity],
  );
  return [unit, setUnit];
}
