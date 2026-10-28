'use client';

import React, { type ReactNode } from 'react';
import { Copy, Lock } from 'lucide-react';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { Card } from '@sim-tuning/ui/components/ui/card';
import { NativeSelect } from '@sim-tuning/ui/components/ui/native-select';
import { Tabs, TabsList, TabsTrigger } from '@sim-tuning/ui/components/ui/tabs';
import { useLocale } from '@/providers/i18n/locale-context';
import { useSimData } from '@/providers/sim/sim-data-context';

/** Picker for the vehicle every module works on (v2 "Fahrzeug auswählen"). */
export function VehicleSelect() {
  const { t } = useLocale();
  const { vehicles, selectedVehicle, selectVehicle } = useSimData();
  return (
    <NativeSelect
      aria-label={t.sim.selectVehicle}
      data-testid="vehicle-select"
      value={selectedVehicle?.id ?? ''}
      onChange={(e) => selectVehicle(e.target.value || null)}
      className="w-auto max-w-[16rem]"
    >
      {vehicles.map((v) => (
        <option key={v.id} value={v.id}>
          {v.isSystem ? `${v.name} · ${t.sim.starterBadge}` : v.name}
        </option>
      ))}
    </NativeSelect>
  );
}

/** Banner shown on starter (read-only) vehicles with a one-tap duplicate. */
export function ReadOnlyBanner() {
  const { t } = useLocale();
  const { selectedVehicle, repository, setVehicle, selectVehicle } = useSimData();
  if (!selectedVehicle?.isSystem) return null;
  return (
    <div
      data-testid="read-only-banner"
      className="bg-secondary flex flex-wrap items-center justify-between gap-2 rounded-2xl px-4 py-3 text-[0.8125rem]"
    >
      <span className="flex items-center gap-2">
        <Lock className="h-4 w-4" />
        {t.sim.readOnlyHint}
      </span>
      <Button
        type="button"
        size="sm"
        variant="secondary"
        onClick={async () => {
          const copy = await repository.duplicateVehicle(selectedVehicle.id);
          setVehicle(copy);
          selectVehicle(copy.id);
        }}
      >
        <Copy className="h-4 w-4" />
        {t.sim.duplicateToEdit}
      </Button>
    </div>
  );
}

export interface ModuleTab<K extends string> {
  key: K;
  label: string;
}

/** Common layout for a module: title, vehicle picker, optional sub-tabs, content. */
export function ModulePage<K extends string>({
  title,
  subtitle,
  tabs,
  activeTab,
  onTabChange,
  showVehicle = true,
  children,
}: {
  title: string;
  subtitle?: string;
  tabs?: ModuleTab<K>[];
  activeTab?: K;
  onTabChange?: (key: K) => void;
  showVehicle?: boolean;
  children: ReactNode;
}) {
  const { t } = useLocale();
  const { selectedVehicle, loading } = useSimData();
  return (
    <div className="space-y-5 pb-12">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="font-display text-2xl font-bold tracking-tight sm:text-3xl">{title}</h1>
          {subtitle && <p className="text-muted-foreground text-sm">{subtitle}</p>}
        </div>
        {showVehicle && <VehicleSelect />}
      </div>
      {tabs && activeTab && onTabChange && (
        <Tabs value={activeTab} onValueChange={(v) => onTabChange(v as K)}>
          <TabsList>
            {tabs.map((tab) => (
              <TabsTrigger key={tab.key} value={tab.key} data-testid={`module-tab-${tab.key}`}>
                {tab.label}
              </TabsTrigger>
            ))}
          </TabsList>
        </Tabs>
      )}
      {showVehicle && <ReadOnlyBanner />}
      {showVehicle && !loading && !selectedVehicle ? (
        <Card className="rounded-[1.25rem] p-5 text-sm">{t.sim.noVehicle}</Card>
      ) : (
        children
      )}
    </div>
  );
}

/** Titled card section used inside module views. */
export function Section({ title, children }: { title: string; children: ReactNode }) {
  return (
    <Card className="space-y-4 rounded-[1.25rem] p-5">
      <h2 className="text-[0.9375rem] font-semibold">{title}</h2>
      {children}
    </Card>
  );
}
