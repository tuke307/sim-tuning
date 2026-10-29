'use client';

import React from 'react';
import { ModulePage } from '@/components/sim/shared/module-page';
import { useSubTab } from '@/lib/navigation/nav-state';
import { useLocale } from '@/providers/i18n/locale-context';
import { useSimData } from '@/providers/sim/sim-data-context';
import { EinlassKanal } from './einlass-kanal';
import { EinlassVergaser } from './einlass-vergaser';

const EINLASS_TABS = ['kanal', 'vergaser'] as const;

/** Intake module (v2 `EinlassMainView`): Kanal and Vergaser sub-tabs. */
export function EinlassView() {
  const { t } = useLocale();
  const { selectedVehicle } = useSimData();
  const [tab, setTab] = useSubTab(EINLASS_TABS, 'kanal');

  return (
    <ModulePage
      module="einlass"
      title={t.einlass.title}
      subtitle={t.einlass.subtitle}
      tabs={[
        { key: 'kanal', label: t.einlass.tabs.kanal },
        { key: 'vergaser', label: t.einlass.tabs.vergaser },
      ]}
      activeTab={tab}
      onTabChange={setTab}
    >
      {selectedVehicle &&
        (tab === 'kanal' ? (
          // Keyed by vehicle so each vehicle gets its own work draft.
          <EinlassKanal key={selectedVehicle.id} vehicle={selectedVehicle} />
        ) : (
          <EinlassVergaser key={selectedVehicle.id} vehicle={selectedVehicle} />
        ))}
    </ModulePage>
  );
}
