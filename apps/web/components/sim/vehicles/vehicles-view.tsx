'use client';

import React, { useState } from 'react';
import { Card } from '@sim-tuning/ui/components/ui/card';
import { ModulePage } from '@/components/sim/shared/module-page';
import { confirmLeave } from '@/lib/navigation/leave-guard';
import { useSubTab } from '@/lib/navigation/nav-state';
import { useLocale } from '@/providers/i18n/locale-context';
import { useSimData } from '@/providers/sim/sim-data-context';
import { CreateVehicleDialog } from './create-vehicle-dialog';
import { EnvironmentsPanel } from './environments-panel';
import { VehicleEditor } from './vehicle-editor';
import { VehicleList } from './vehicle-list';

const VEHICLES_TABS = ['vehicles', 'environments'] as const;

export function VehiclesView() {
  const { t } = useLocale();
  const { vehicles, selectedVehicle, selectVehicle, loading, error } = useSimData();
  const [tab, setTab] = useSubTab(VEHICLES_TABS, 'vehicles');
  const [createOpen, setCreateOpen] = useState(false);

  return (
    <ModulePage
      module="vehicles"
      title={t.vehicles.title}
      subtitle={t.vehicles.subtitle}
      showVehicle={false}
      tabs={[
        { key: 'vehicles', label: t.vehicles.tabVehicles },
        { key: 'environments', label: t.vehicles.tabEnvironments },
      ]}
      activeTab={tab}
      onTabChange={setTab}
    >
      {error && (
        <Card role="alert" className="text-destructive rounded-md p-4 text-sm">
          {t.sim.errorGeneric}
        </Card>
      )}
      {tab === 'environments' ? (
        <EnvironmentsPanel />
      ) : (
        <div className="grid items-start gap-4 lg:grid-cols-[20rem_minmax(0,1fr)]">
          <div className="lg:sticky lg:top-4">
            <VehicleList
              vehicles={vehicles}
              selectedId={selectedVehicle?.id ?? null}
              onSelect={(id) => {
                if (id === selectedVehicle?.id) return;
                // The open editor may hold unsaved changes: let it save, discard or cancel first.
                void confirmLeave().then((ok) => {
                  if (ok) selectVehicle(id);
                });
              }}
              onCreate={() => {
                // Creating selects the new vehicle, which replaces the open editor.
                void confirmLeave().then((ok) => {
                  if (ok) setCreateOpen(true);
                });
              }}
            />
          </div>
          {selectedVehicle ? (
            <VehicleEditor key={selectedVehicle.id} vehicle={selectedVehicle} />
          ) : (
            !loading && (
              <Card className="text-muted-foreground rounded-md p-5 text-sm">
                {t.vehicles.editorEmpty}
              </Card>
            )
          )}
        </div>
      )}
      <CreateVehicleDialog open={createOpen} onOpenChange={setCreateOpen} />
    </ModulePage>
  );
}
