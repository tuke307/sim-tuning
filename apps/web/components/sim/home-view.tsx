'use client';

import React from 'react';
import { useSimData } from '@/providers/sim/sim-data-context';
import { useLocale } from '@/providers/i18n/locale-context';
import { CurrentVehicleSheet } from './home/current-vehicle-sheet';
import { FirstRun } from './home/first-run';
import { GuestImportCard } from './home/guest-import-card';
import { ModuleIndex } from './home/module-index';
import type { NavigateFn } from './home/view-link';

/**
 * Home: the sheet of the vehicle you are working on (or the first-run model picker), followed by
 * a compact module index.
 */
export function HomeView({ onNavigate }: { onNavigate: NavigateFn }) {
  const { t } = useLocale();
  const { selectedVehicle, vehicles, loading } = useSimData();
  const hasOwnVehicle = vehicles.some((v) => !v.isSystem);

  return (
    <div className="space-y-4 pb-12">
      <GuestImportCard />

      {loading ? (
        <div aria-busy className="bg-card rounded-md border border-dashed px-4 py-6">
          <h1 className="sr-only">{t.home.title}</h1>
          <p className="text-muted-foreground text-[0.8125rem]">{t.home.loading}</p>
        </div>
      ) : hasOwnVehicle && selectedVehicle ? (
        <CurrentVehicleSheet vehicle={selectedVehicle} onNavigate={onNavigate} />
      ) : (
        <FirstRun onNavigate={onNavigate} />
      )}

      <ModuleIndex onNavigate={onNavigate} />
    </div>
  );
}
