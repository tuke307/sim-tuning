'use client';

import { useState } from 'react';
import type { VehicleDto, VehicleInputDto } from '@sim-tuning/common/schemas';
import { toast } from '@sim-tuning/ui/components/ui/sonner';
import { workDraftStore } from '@/lib/sim/drafts/work-draft';
import { interpolate } from '@/lib/sim/vehicles/draft';
import { useLocale } from '@/providers/i18n/locale-context';
import { useSimData } from '@/providers/sim/sim-data-context';

export type SaveToVehicleStatus = 'idle' | 'saving' | 'saved' | 'error';

export interface SaveToVehicleOptions {
  /** Partial nested update for the selected vehicle; `null` while there is nothing to save. */
  buildUpdate: (motorName: string) => Partial<VehicleInputDto> | null;
  /**
   * Called after the values were stored on `saved` (the selected vehicle or, for a starter, the
   * new copy). May return a function that undoes local side effects (drafts) for "Undo".
   */
  onSaved?: (saved: VehicleDto) => (() => void) | void;
}

/**
 * Shared "save calculator values to the vehicle" logic of the Motor, Einlass and Auslass panels.
 * Own vehicles are updated in place. Starter vehicles are read-only, so saving duplicates the
 * starter, applies the update to the copy, selects it and offers "Undo" in a toast (same flow as
 * iOS).
 */
export function useSaveToVehicle({ buildUpdate, onSaved }: SaveToVehicleOptions) {
  const { t } = useLocale();
  const { repository, selectedVehicle, setVehicle, selectVehicle, refresh } = useSimData();
  const [status, setStatus] = useState<SaveToVehicleStatus>('idle');
  const [savedJson, setSavedJson] = useState<string | null>(null);
  const isStarter = !!selectedVehicle?.isSystem;
  const motorName = selectedVehicle?.motor?.name ?? selectedVehicle?.name ?? 'Motor';
  const update = selectedVehicle ? buildUpdate(motorName) : null;
  const updateJson = update ? JSON.stringify(update) : null;
  // "Saved" is only shown while the inputs still match what was stored.
  const saved = status === 'saved' && savedJson === updateJson;

  const saveOwn = async (vehicle: VehicleDto, input: Partial<VehicleInputDto>) => {
    const updated = await repository.updateVehicle(vehicle.id, input);
    setVehicle(updated);
    onSaved?.(updated);
    setSavedJson(updateJson);
    setStatus('saved');
  };

  const saveAsCopy = async (starter: VehicleDto, input: Partial<VehicleInputDto>) => {
    const copy = await repository.duplicateVehicle(
      starter.id,
      `${starter.name} ${t.vehicles.copySuffix}`.slice(0, 120),
    );
    let updated: VehicleDto;
    try {
      updated = await repository.updateVehicle(copy.id, input);
    } catch (err) {
      // Do not leave an unpatched copy behind.
      await repository.deleteVehicle(copy.id).catch(() => undefined);
      throw err;
    }
    setVehicle(updated);
    const undoDrafts = onSaved?.(updated);
    selectVehicle(updated.id);
    setStatus('idle');

    const undo = async () => {
      try {
        await repository.deleteVehicle(updated.id);
      } catch {
        toast.error(t.sim.errorGeneric);
        return;
      }
      workDraftStore.clearVehicle(updated.id);
      undoDrafts?.();
      selectVehicle(starter.id);
      await refresh();
    };

    toast(interpolate(t.sim.copyCreated, { name: updated.name }), {
      action: { label: t.sim.undo, onClick: () => void undo() },
    });
  };

  const save = async () => {
    if (!selectedVehicle || !update || status === 'saving') return;
    setStatus('saving');
    try {
      if (selectedVehicle.isSystem) await saveAsCopy(selectedVehicle, update);
      else await saveOwn(selectedVehicle, update);
    } catch {
      setStatus('error');
    }
  };

  return {
    status,
    saved,
    isStarter,
    canSave: !!update && status !== 'saving',
    save,
  };
}
