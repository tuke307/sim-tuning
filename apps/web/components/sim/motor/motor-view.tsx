'use client';

import React from 'react';
import { ModulePage } from '@/components/sim/shared/module-page';
import { useSubTab } from '@/lib/navigation/nav-state';
import { useLocale } from '@/providers/i18n/locale-context';
import { useSimData } from '@/providers/sim/sim-data-context';
import { HubraumPanel } from './hubraum-panel';
import { SteuerdiagrammPanel } from './steuerdiagramm-panel';
import { UmrechnungPanel } from './umrechnung-panel';
import { VerdichtungPanel } from './verdichtung-panel';

const MOTOR_TABS = ['hubraum', 'verdichtung', 'steuerdiagramm', 'umrechnung'] as const;

/** v2 MotorMainView: Hubraum, Verdichtung, Steuerdiagramm and Umrechnung calculators. */
export function MotorView() {
  const { t } = useLocale();
  const { selectedVehicle } = useSimData();
  const [tab, setTab] = useSubTab(MOTOR_TABS, 'hubraum');

  return (
    <ModulePage
      module="motor"
      title={t.motor.title}
      subtitle={t.motor.subtitle}
      tabs={[
        { key: 'hubraum', label: t.motor.tabHubraum },
        { key: 'verdichtung', label: t.motor.tabVerdichtung },
        { key: 'steuerdiagramm', label: t.motor.tabSteuerdiagramm },
        { key: 'umrechnung', label: t.motor.tabUmrechnung },
      ]}
      activeTab={tab}
      onTabChange={setTab}
    >
      {selectedVehicle && (
        // Inputs are per-vehicle work drafts seeded from the selected vehicle.
        <div key={selectedVehicle.id} data-testid={`motor-panel-${tab}`}>
          {tab === 'hubraum' && <HubraumPanel vehicle={selectedVehicle} />}
          {tab === 'verdichtung' && <VerdichtungPanel vehicle={selectedVehicle} />}
          {tab === 'steuerdiagramm' && <SteuerdiagrammPanel vehicle={selectedVehicle} />}
          {tab === 'umrechnung' && <UmrechnungPanel vehicle={selectedVehicle} />}
        </div>
      )}
    </ModulePage>
  );
}
