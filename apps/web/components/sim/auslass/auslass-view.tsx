'use client';

import React, { useState } from 'react';
import { ModulePage } from '@/components/sim/shared/module-page';
import { useLocale } from '@/providers/i18n/locale-context';
import { useSimData } from '@/providers/sim/sim-data-context';
import { AuslassAnwendung } from './auslass-anwendung';
import { AuslassTheorie } from './auslass-theorie';

type AuslassTab = 'theorie' | 'anwendung';

/** Exhaust module (v2 `AuslassMainView`): Theorie and Anwendung sub-tabs. */
export function AuslassView() {
  const { t } = useLocale();
  const { selectedVehicle } = useSimData();
  const [tab, setTab] = useState<AuslassTab>('theorie');

  return (
    <ModulePage
      module="auslass"
      title={t.auslass.title}
      subtitle={t.auslass.subtitle}
      tabs={[
        { key: 'theorie', label: t.auslass.tabs.theorie },
        { key: 'anwendung', label: t.auslass.tabs.anwendung },
      ]}
      activeTab={tab}
      onTabChange={setTab}
    >
      {selectedVehicle &&
        (tab === 'theorie' ? (
          // Keyed by vehicle so the draft is re-initialised when another vehicle is picked.
          <AuslassTheorie key={selectedVehicle.id} vehicle={selectedVehicle} />
        ) : (
          <AuslassAnwendung key={selectedVehicle.id} vehicle={selectedVehicle} />
        ))}
    </ModulePage>
  );
}
