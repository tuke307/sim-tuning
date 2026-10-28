'use client';

import React, { useState } from 'react';
import { Card } from '@sim-tuning/ui/components/ui/card';
import { ModulePage } from '@/components/sim/shared/module-page';
import { useLocale } from '@/providers/i18n/locale-context';
import { useSimData } from '@/providers/sim/sim-data-context';
import { CreateVehicleDialog } from './create-vehicle-dialog';
import { EnvironmentsPanel } from './environments-panel';
import { VehicleEditor } from './vehicle-editor';
import { VehicleList } from './vehicle-list';

type VehiclesTab = 'vehicles' | 'environments';

export function VehiclesView() {
  const { t } = useLocale();
  const { vehicles, selectedVehicle, selectVehicle, loading, error } = useSimData();
  const [tab, setTab] = useState<VehiclesTab>('vehicles');
  const [createOpen, setCreateOpen] = useState(false);

  return (
    <ModulePage
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
        <Card role="alert" className="text-destructive rounded-[1.25rem] p-4 text-sm">
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
              onSelect={selectVehicle}
              onCreate={() => setCreateOpen(true)}
            />
          </div>
          {selectedVehicle ? (
            <VehicleEditor key={selectedVehicle.id} vehicle={selectedVehicle} />
          ) : (
            !loading && (
              <Card className="text-muted-foreground rounded-[1.25rem] p-5 text-sm">
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
