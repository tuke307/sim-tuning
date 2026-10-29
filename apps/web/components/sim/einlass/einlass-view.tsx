'use client';

import React, { useState } from 'react';
import { ModulePage } from '@/components/sim/shared/module-page';
import { useLocale } from '@/providers/i18n/locale-context';
import { useSimData } from '@/providers/sim/sim-data-context';
import { EinlassKanal } from './einlass-kanal';
import { EinlassVergaser } from './einlass-vergaser';

type EinlassTab = 'kanal' | 'vergaser';

/** Intake module (v2 `EinlassMainView`): Kanal and Vergaser sub-tabs. */
export function EinlassView() {
  const { t } = useLocale();
  const { selectedVehicle } = useSimData();
  const [tab, setTab] = useState<EinlassTab>('kanal');

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
          // Keyed by vehicle so the draft is re-initialised when another vehicle is picked.
          <EinlassKanal key={selectedVehicle.id} vehicle={selectedVehicle} />
        ) : (
          <EinlassVergaser key={selectedVehicle.id} vehicle={selectedVehicle} />
        ))}
    </ModulePage>
  );
}
