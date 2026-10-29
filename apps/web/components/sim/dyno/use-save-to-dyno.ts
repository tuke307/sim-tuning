'use client';

import { useCallback } from 'react';
import type { DynoDto, VehicleDto } from '@sim-tuning/common/schemas';
import { useSimData } from '@/providers/sim/sim-data-context';

/** Vehicle names are at most 120 characters, the dyno name has the same limit. */
const MAX_DYNO_NAME = 120;
const DYNO_NAME_SUFFIX = ' Dyno';

/** Default name of a dyno that is created automatically for a vehicle. */
export function defaultDynoName(vehicleName: string): string {
  const base = vehicleName
    .trim()
    .slice(0, MAX_DYNO_NAME - DYNO_NAME_SUFFIX.length)
    .trim();
  return base ? `${base}${DYNO_NAME_SUFFIX}` : DYNO_NAME_SUFFIX.trim();
}

/**
 * Every own vehicle has at most one dyno. Saving a curve, run or diagnosis on a vehicle without
 * one creates it on the fly (as "Daten" would), so the user never has to set it up first.
 * Returns a function that runs `save` against the (possibly new) dyno, reports the result via
 * `onDynoChange` and resolves `null` for starter vehicles, which stay read-only.
 */
export function useSaveToDyno(
  vehicle: VehicleDto | null,
  dyno: DynoDto | null,
  onDynoChange: (dyno: DynoDto) => void,
) {
  const { repository, setVehicle } = useSimData();
  return useCallback(
    async (save: (target: DynoDto) => Promise<DynoDto>): Promise<DynoDto | null> => {
      if (dyno) {
        const saved = await save(dyno);
        onDynoChange(saved);
        return saved;
      }
      if (!vehicle || vehicle.isSystem) return null;
      const created = await repository.saveDyno(vehicle.id, {
        name: defaultDynoName(vehicle.name),
      });
      // Refresh the vehicle only after saving so its new dynoId does not trigger a reload of the
      // dyno while the data is still being written.
      const refreshVehicle = async () => {
        const fresh = await repository.getVehicle(vehicle.id).catch(() => null);
        if (fresh) setVehicle(fresh);
      };
      let saved: DynoDto;
      try {
        saved = await save(created);
      } catch (err) {
        onDynoChange(created);
        await refreshVehicle();
        throw err;
      }
      onDynoChange(saved);
      await refreshVehicle();
      return saved;
    },
    [dyno, vehicle, repository, setVehicle, onDynoChange],
  );
}
