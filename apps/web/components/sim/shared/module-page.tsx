'use client';

import React, { type ReactNode } from 'react';
import { Copy, Lock } from 'lucide-react';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { NativeSelect } from '@sim-tuning/ui/components/ui/native-select';
import { Tabs, TabsList, TabsTrigger } from '@sim-tuning/ui/components/ui/tabs';
import { cn } from '@sim-tuning/ui/lib/utils';
import { useLocale } from '@/providers/i18n/locale-context';
import { useSimData } from '@/providers/sim/sim-data-context';
import { MODULE_META, type SimModule } from './modules';

/** Picker for the vehicle every module works on (v2 "Fahrzeug auswählen"). */
export function VehicleSelect({ className }: { className?: string }) {
  const { t } = useLocale();
  const { vehicles, selectedVehicle, selectVehicle } = useSimData();
  return (
    <NativeSelect
      aria-label={t.sim.selectVehicle}
      data-testid="vehicle-select"
      value={selectedVehicle?.id ?? ''}
      onChange={(e) => selectVehicle(e.target.value || null)}
      className={cn('w-auto max-w-[16rem]', className)}
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
      className="bg-card flex flex-wrap items-center justify-between gap-2 rounded-md border border-dashed px-3 py-2 text-[0.8125rem]"
    >
      <span className="text-muted-foreground flex items-center gap-2">
        <Lock className="h-4 w-4 shrink-0" aria-hidden />
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
        <Copy className="h-4 w-4" aria-hidden />
        {t.sim.duplicateToEdit}
      </Button>
    </div>
  );
}

export interface ModuleTab<K extends string> {
  key: K;
  label: string;
}

/** Small uppercase field label inside the Schriftfeld. */
function FieldLabel({ children }: { children: ReactNode }) {
  return (
    <span className="font-display text-muted-foreground block text-[0.6875rem] leading-none font-semibold tracking-[0.08em] uppercase">
      {children}
    </span>
  );
}

/**
 * Schriftfeld (title block) that opens every module: a ruled box whose top rule is the module
 * colour, with sheet number, module name, vehicle, engine and date. Collapses to one line
 * below 480px.
 */
function Schriftfeld({
  module,
  title,
  showVehicle,
}: {
  module: SimModule;
  title: string;
  showVehicle: boolean;
}) {
  const { t } = useLocale();
  const { selectedVehicle } = useSimData();
  const meta = MODULE_META[module];
  // ISO 8601 date as on a technical drawing (DIN ISO 8601), the same in every language.
  const date = new Date().toLocaleDateString('sv-SE');
  const engine = selectedVehicle?.motor?.name?.trim() || '–';

  return (
    <div
      role="group"
      aria-label={t.sim.titleBlock}
      data-testid="schriftfeld"
      data-module={module}
      className="bg-card relative flex flex-wrap items-stretch overflow-hidden rounded-md border pt-0.5"
    >
      {/* Top rule in the module colour. */}
      <span aria-hidden className={cn('absolute inset-x-0 top-0 h-0.5', meta.bg)} />
      <div className="flex min-h-14 shrink-0 items-center gap-2 px-3 py-2">
        <span className="font-data text-muted-foreground text-[0.75rem] tracking-[0.06em] whitespace-nowrap uppercase">
          {t.sim.sheet} {meta.sheet}
        </span>
        <span aria-hidden className={cn('font-data text-[0.75rem]', meta.text)}>
          ·
        </span>
        <h1 className="font-display text-[1.25rem] leading-none font-bold sm:text-[1.5rem]">
          {title}
        </h1>
      </div>
      {showVehicle && (
        <div className="flex min-w-0 flex-1 items-center border-l px-2 py-1.5 sm:px-3">
          <div className="w-full min-w-0 sm:max-w-[22rem]">
            <VehicleSelect className="w-full max-w-full" />
          </div>
        </div>
      )}
      <dl
        className={cn(
          'hidden w-full border-t min-[480px]:flex md:w-auto md:border-t-0 md:border-l',
          !showVehicle && 'md:ml-auto',
        )}
      >
        {showVehicle && (
          <div className="flex min-w-0 flex-1 flex-col justify-center gap-1 px-3 py-2 md:min-w-[8rem] md:flex-none">
            <dt>
              <FieldLabel>{t.sim.engine}</FieldLabel>
            </dt>
            <dd className="font-data truncate text-[0.875rem]" data-testid="schriftfeld-engine">
              {engine}
            </dd>
          </div>
        )}
        <div className="flex flex-col justify-center gap-1 border-l px-3 py-2 first:border-l-0">
          <dt>
            <FieldLabel>{t.sim.date}</FieldLabel>
          </dt>
          <dd className="font-data text-[0.875rem] whitespace-nowrap">{date}</dd>
        </div>
      </dl>
    </div>
  );
}

/** Common layout for a module: Schriftfeld, optional sub-tabs, content. */
export function ModulePage<K extends string>({
  module,
  title,
  subtitle,
  tabs,
  activeTab,
  onTabChange,
  showVehicle = true,
  children,
}: {
  module: SimModule;
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
    <div className="space-y-4 pb-12">
      <div className="space-y-2">
        <Schriftfeld module={module} title={title} showVehicle={showVehicle} />
        {subtitle && <p className="text-muted-foreground text-[0.8125rem]">{subtitle}</p>}
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
        <div className="bg-card rounded-md border border-dashed p-4 text-sm">{t.sim.noVehicle}</div>
      ) : (
        children
      )}
    </div>
  );
}

/** Hairline sheet with an uppercase condensed heading, used inside module views. */
export function Section({
  title,
  children,
  className,
  bodyClassName,
  action,
}: {
  title: string;
  children: ReactNode;
  className?: string;
  bodyClassName?: string;
  action?: ReactNode;
}) {
  return (
    <section className={cn('bg-card min-w-0 rounded-md border', className)}>
      <div className="flex min-h-10 items-center justify-between gap-2 border-b px-4 py-2">
        <h2 className="font-display text-[0.875rem] leading-tight font-semibold">{title}</h2>
        {action}
      </div>
      <div className={cn('space-y-4 p-4', bodyClassName)}>{children}</div>
    </section>
  );
}
