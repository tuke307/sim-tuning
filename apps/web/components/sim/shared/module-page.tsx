'use client';

import React, { type ReactNode } from 'react';
import { Check, Copy, Loader2, Lock, PencilLine, RotateCcw, Save } from 'lucide-react';
import type { VehicleDto, VehicleInputDto } from '@sim-tuning/common/schemas';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { NativeSelect } from '@sim-tuning/ui/components/ui/native-select';
import { Tabs, TabsList, TabsTrigger } from '@sim-tuning/ui/components/ui/tabs';
import { cn } from '@sim-tuning/ui/lib/utils';
import { useSaveToVehicle } from '@/hooks/useSaveToVehicle';
import { confirmLeave } from '@/lib/navigation/leave-guard';
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
      onChange={(e) => {
        const id = e.target.value || null;
        // A dirty vehicle editor may veto or save first; the controlled value snaps back on cancel.
        void confirmLeave().then((ok) => {
          if (ok) selectVehicle(id);
        });
      }}
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

/**
 * Quiet marker for calculator inputs that differ from the vehicle's stored values, with an action
 * that re-seeds them from the vehicle.
 */
export function DraftNotice({
  dirty,
  onReset,
  testId,
}: {
  dirty: boolean;
  onReset: () => void;
  testId?: string;
}) {
  const { t } = useLocale();
  if (!dirty) return null;
  return (
    <div
      data-testid={testId}
      className="text-muted-foreground flex flex-wrap items-center gap-x-3 gap-y-1 text-xs"
    >
      <span className="flex items-center gap-1.5">
        <PencilLine className="h-3.5 w-3.5 shrink-0" aria-hidden />
        {t.sim.draftUnsaved}
      </span>
      <Button
        type="button"
        size="xs"
        variant="ghost"
        onClick={onReset}
        data-testid={testId ? `${testId}-reset` : undefined}
      >
        <RotateCcw className="h-3.5 w-3.5" aria-hidden />
        {t.sim.draftReset}
      </Button>
    </div>
  );
}

/**
 * "Im Fahrzeug speichern" for calculator panels. On a starter vehicle it reads "Als eigenes
 * Fahrzeug speichern" and saves into a new copy (see `useSaveToVehicle`).
 */
export function SaveToVehicleButton({
  buildUpdate,
  onSaved,
  draft,
  hint,
  disabled,
  testId,
  size,
  variant,
}: {
  buildUpdate: (motorName: string) => Partial<VehicleInputDto> | null;
  onSaved?: (saved: VehicleDto) => (() => void) | void;
  /** Draft state of the panel for the "not saved to vehicle" marker. */
  draft?: { dirty: boolean; onReset: () => void };
  hint?: string;
  disabled?: boolean;
  testId?: string;
  size?: 'default' | 'sm';
  variant?: 'default' | 'accent';
}) {
  const { t } = useLocale();
  const { status, saved, isStarter, canSave, save } = useSaveToVehicle({ buildUpdate, onSaved });
  return (
    <div className="space-y-2 pt-1">
      {draft && (
        <DraftNotice
          dirty={draft.dirty}
          onReset={draft.onReset}
          testId={testId ? `${testId}-draft` : undefined}
        />
      )}
      <div className="flex flex-wrap items-center gap-3">
        <Button
          type="button"
          size={size}
          variant={variant}
          onClick={() => void save()}
          disabled={disabled || !canSave}
          data-testid={testId}
        >
          {status === 'saving' ? (
            <Loader2 className="h-4 w-4 animate-spin" aria-hidden />
          ) : saved ? (
            <Check className="h-4 w-4" aria-hidden />
          ) : isStarter ? (
            <Copy className="h-4 w-4" aria-hidden />
          ) : (
            <Save className="h-4 w-4" aria-hidden />
          )}
          {isStarter ? t.sim.saveAsCopy : t.sim.saveToVehicle}
        </Button>
        <span role="status" aria-live="polite" className="text-[0.8125rem]">
          {saved && (
            <span className="text-muted-foreground flex items-center gap-1">
              <Check className="h-4 w-4" aria-hidden />
              {t.sim.saved}
            </span>
          )}
          {status === 'error' && <span className="text-destructive">{t.sim.errorGeneric}</span>}
        </span>
      </div>
      {hint && <p className="text-muted-foreground text-xs">{hint}</p>}
      {isStarter && <p className="text-muted-foreground text-xs">{t.sim.saveAsCopyHint}</p>}
    </div>
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
