'use client';

import React, {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import type { UnitRoundingOptions } from '@sim-tuning/common/units';
import type { EnvironmentDto, VehicleDto } from '@sim-tuning/common/schemas';
import { ROUNDING_ACCURACY } from '@sim-tuning/common/schemas';
import { useAuth } from '@/providers/auth/auth-context';
import { ApiRepository } from '@/lib/data/api-repository';
import { LocalRepository } from '@/lib/data/local-repository';
import type { SimRepository } from '@/lib/data/repository';

const SELECTED_VEHICLE_KEY = 'sim_tuning_selected_vehicle';

interface SimDataContextValue {
  repository: SimRepository;
  vehicles: VehicleDto[];
  environments: EnvironmentDto[];
  loading: boolean;
  error: string | null;
  /** Vehicle all module views work on (v2 "aktives Fahrzeug"). */
  selectedVehicle: VehicleDto | null;
  selectVehicle: (id: string | null) => void;
  /** Re-reads vehicles and environments, e.g. after a mutation. */
  refresh: () => Promise<void>;
  /** Replaces one vehicle in the cached list after it was saved. */
  setVehicle: (vehicle: VehicleDto) => void;
  /** Unit rounding from the user's preferences (v2 UnitSettings). */
  rounding: UnitRoundingOptions;
}

const SimDataContext = createContext<SimDataContextValue | null>(null);

export function SimDataProvider({ children }: { children: ReactNode }) {
  const { authMode, user } = useAuth();
  const repository = useMemo<SimRepository>(
    () => (authMode === 'AUTHENTICATED' ? new ApiRepository() : new LocalRepository()),
    [authMode],
  );
  const [vehicles, setVehicles] = useState<VehicleDto[]>([]);
  const [environments, setEnvironments] = useState<EnvironmentDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedId, setSelectedId] = useState<string | null>(() =>
    typeof window === 'undefined' ? null : window.localStorage.getItem(SELECTED_VEHICLE_KEY),
  );

  const refresh = useCallback(async () => {
    try {
      const [nextVehicles, nextEnvironments] = await Promise.all([
        repository.listVehicles(),
        repository.listEnvironments(),
      ]);
      setVehicles(nextVehicles);
      setEnvironments(nextEnvironments);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load data');
    } finally {
      setLoading(false);
    }
  }, [repository]);

  useEffect(() => {
    let cancelled = false;
    Promise.all([repository.listVehicles(), repository.listEnvironments()])
      .then(([nextVehicles, nextEnvironments]) => {
        if (cancelled) return;
        setVehicles(nextVehicles);
        setEnvironments(nextEnvironments);
        setError(null);
      })
      .catch((err: unknown) => {
        if (!cancelled) setError(err instanceof Error ? err.message : 'Failed to load data');
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [repository]);

  const selectVehicle = useCallback((id: string | null) => {
    setSelectedId(id);
    if (id) window.localStorage.setItem(SELECTED_VEHICLE_KEY, id);
    else window.localStorage.removeItem(SELECTED_VEHICLE_KEY);
  }, []);

  const setVehicle = useCallback((vehicle: VehicleDto) => {
    setVehicles((list) =>
      list.some((v) => v.id === vehicle.id)
        ? list.map((v) => (v.id === vehicle.id ? vehicle : v))
        : [...list, vehicle],
    );
  }, []);

  const selectedVehicle =
    vehicles.find((v) => v.id === selectedId) ??
    vehicles.find((v) => !v.isSystem) ??
    vehicles[0] ??
    null;

  const rounding = useMemo<UnitRoundingOptions>(
    () => ({
      roundingAccuracy: user?.preferences?.roundingAccuracy ?? ROUNDING_ACCURACY.default,
      roundOnUnitChange: user?.preferences?.roundOnUnitChange ?? true,
    }),
    [user?.preferences?.roundingAccuracy, user?.preferences?.roundOnUnitChange],
  );

  const value = useMemo<SimDataContextValue>(
    () => ({
      repository,
      vehicles,
      environments,
      loading,
      error,
      selectedVehicle,
      selectVehicle,
      refresh,
      setVehicle,
      rounding,
    }),
    [
      repository,
      vehicles,
      environments,
      loading,
      error,
      selectedVehicle,
      selectVehicle,
      refresh,
      setVehicle,
      rounding,
    ],
  );

  return <SimDataContext.Provider value={value}>{children}</SimDataContext.Provider>;
}

export function useSimData(): SimDataContextValue {
  const ctx = useContext(SimDataContext);
  if (!ctx) throw new Error('useSimData must be used within a SimDataProvider');
  return ctx;
}
