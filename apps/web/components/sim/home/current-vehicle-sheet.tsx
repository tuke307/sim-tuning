'use client';

import React, { useEffect, useState } from 'react';
import { ChevronRight } from 'lucide-react';
import type { DynoDto, VehicleDto } from '@sim-tuning/common/schemas';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { cn } from '@sim-tuning/ui/lib/utils';
import { useLocale } from '@/providers/i18n/locale-context';
import { useSimData } from '@/providers/sim/sim-data-context';
import type { ModuleTab } from '../pwa-shell';
import { MODULE_META } from '../shared/modules';
import { ReadOnlyBanner, Section, VehicleSelect } from '../shared/module-page';
import { ResultValue } from '../shared/unit-number-field';
import { ViewLink, type NavigateFn } from './view-link';

const KW = '°KW';

/** Small uppercase label above a group of values, as in the Schriftfeld. */
function GroupLabel({ children }: { children: React.ReactNode }) {
  return (
    <h2 className="font-display text-muted-foreground text-[0.6875rem] leading-none font-semibold tracking-[0.08em] uppercase">
      {children}
    </h2>
  );
}

export type NextStepModule = Extract<ModuleTab, 'vehicles' | 'dyno' | 'motor'>;

/** Module that moves this vehicle forward: engine data first, then a dyno run, then tuning. */
export function pickNextStep(vehicle: VehicleDto, dyno: DynoDto | null): NextStepModule {
  const motor = vehicle.motor;
  const hasGeometry =
    !!motor && (motor.hubraumV !== null || (motor.hubL !== null && motor.bohrungD !== null));
  if (!hasGeometry) return 'vehicles';
  if (!dyno || dyno.dynoPs.length === 0) return 'dyno';
  return 'motor';
}

function peakPower(dyno: DynoDto): { ps: number; drehzahl: number } | null {
  let peak: { ps: number; drehzahl: number } | null = null;
  for (const point of dyno.dynoPs) {
    if (Number.isFinite(point.ps) && (!peak || point.ps > peak.ps)) peak = point;
  }
  return peak;
}

/** Loads the vehicle's dyno run; `undefined` while loading, `null` when there is none. */
function useVehicleDyno(vehicle: VehicleDto): DynoDto | null | undefined {
  const { repository } = useSimData();
  const key = `${vehicle.id}:${vehicle.dynoId ?? ''}:${vehicle.updatedAt}`;
  const [result, setResult] = useState<{ key: string; dyno: DynoDto | null } | null>(null);

  useEffect(() => {
    let cancelled = false;
    repository
      .getDyno(vehicle.id)
      .then((dyno) => {
        if (!cancelled) setResult({ key, dyno });
      })
      .catch(() => {
        if (!cancelled) setResult({ key, dyno: null });
      });
    return () => {
      cancelled = true;
    };
  }, [repository, vehicle.id, key]);

  return result?.key === key ? result.dyno : undefined;
}

/** Home for a returning user: the active vehicle as a title-block sheet plus the next step. */
export function CurrentVehicleSheet({
  vehicle,
  onNavigate,
}: {
  vehicle: VehicleDto;
  onNavigate: NavigateFn;
}) {
  const { t, bcp47 } = useLocale();
  const { vehicles } = useSimData();
  const dyno = useVehicleDyno(vehicle);
  const motor = vehicle.motor;
  const engine = motor?.name?.trim();
  const hubraumCm3 =
    motor?.hubraumV !== null && motor?.hubraumV !== undefined ? motor.hubraumV / 1000 : null;
  const peak = dyno ? peakPower(dyno) : null;
  const peakRpm = peak
    ? new Intl.NumberFormat(bcp47, { maximumFractionDigits: 0 }).format(peak.drehzahl)
    : null;
  const next = dyno === undefined ? null : pickNextStep(vehicle, dyno);
  const nextMeta = next ? MODULE_META[next] : null;

  return (
    <div className="space-y-4">
      <section
        aria-labelledby="home-vehicle-name"
        data-testid="home-vehicle-sheet"
        className="bg-card relative overflow-hidden rounded-md border pt-0.5"
      >
        <span
          aria-hidden
          className={cn('absolute inset-x-0 top-0 h-0.5', MODULE_META.vehicles.bg)}
        />
        <div className="flex flex-wrap items-end justify-between gap-3 border-b px-4 py-3">
          <div className="min-w-0 space-y-1.5">
            <p className="font-display text-muted-foreground text-[0.6875rem] leading-none font-semibold tracking-[0.08em] uppercase">
              {t.home.currentVehicle}
            </p>
            <h1
              id="home-vehicle-name"
              data-testid="home-vehicle-name"
              className="font-display text-[1.5rem] leading-tight font-bold break-words sm:text-[2rem]"
            >
              {vehicle.name}
            </h1>
            <p
              data-testid="home-vehicle-engine"
              className={cn('text-[0.875rem]', engine ? 'font-data' : 'text-muted-foreground')}
            >
              {engine || t.home.noEngine}
            </p>
          </div>
          {vehicles.length > 1 && <VehicleSelect className="w-full sm:w-auto" />}
        </div>

        <div className="space-y-4 p-4">
          <ReadOnlyBanner />
          <div className="space-y-2">
            <GroupLabel>{t.home.figuresTitle}</GroupLabel>
            <div className="grid grid-cols-2 gap-2 sm:grid-cols-4">
              <ResultValue
                label={t.home.hubraum}
                value={hubraumCm3}
                unit="cm³"
                testId="home-figure-hubraum"
              />
              <ResultValue
                label={t.home.verdichtung}
                value={motor?.verdichtungV}
                testId="home-figure-verdichtung"
              />
              <ResultValue
                label={t.home.hub}
                value={motor?.hubL}
                unit="mm"
                testId="home-figure-hub"
              />
              <ResultValue
                label={t.home.bohrung}
                value={motor?.bohrungD}
                unit="mm"
                testId="home-figure-bohrung"
              />
            </div>
          </div>
          <div className="space-y-2">
            <GroupLabel>{t.home.timingsTitle}</GroupLabel>
            <div className="grid grid-cols-3 gap-2">
              <ResultValue
                label={t.home.timingEinlass}
                value={motor?.einlass?.steuerzeitSZ}
                unit={KW}
                testId="home-timing-einlass"
              />
              <ResultValue
                label={t.home.timingAuslass}
                value={motor?.auslass?.steuerzeitSZ}
                unit={KW}
                testId="home-timing-auslass"
              />
              <ResultValue
                label={t.home.timingUeberstroemer}
                value={motor?.ueberstroemer?.steuerzeitSZ}
                unit={KW}
                testId="home-timing-ueberstroemer"
              />
            </div>
          </div>
        </div>
      </section>

      <Section title={t.home.dynoTitle}>
        <div data-testid="home-dyno-summary" aria-busy={dyno === undefined}>
          {dyno === undefined ? (
            <p className="text-muted-foreground text-[0.8125rem]">{t.home.dynoLoading}</p>
          ) : dyno === null ? (
            <p className="text-muted-foreground text-[0.8125rem]">{t.home.dynoNone}</p>
          ) : (
            <div className="space-y-2">
              <p className="font-data truncate text-[0.875rem]">{dyno.name}</p>
              <div className="grid grid-cols-2 gap-2">
                <ResultValue
                  label={t.home.dynoRpmPoints}
                  value={dyno.drehzahl.length}
                  testId="home-dyno-points"
                />
                <ResultValue
                  label={
                    peakRpm
                      ? `${t.home.dynoPeakPower} ${t.home.dynoPeakAt} ${peakRpm} U/min`
                      : t.home.dynoPeakPower
                  }
                  value={peak?.ps}
                  unit="PS"
                  testId="home-dyno-peak"
                />
              </div>
              {!peak && (
                <p className="text-muted-foreground text-[0.8125rem]">{t.home.dynoNoCurve}</p>
              )}
            </div>
          )}
        </div>
      </Section>

      {next && nextMeta && (
        <section
          aria-labelledby="home-next-step-title"
          data-testid="home-next-step"
          data-module={next}
          className="bg-card relative flex flex-wrap items-center justify-between gap-3 overflow-hidden rounded-md border py-3 pr-4 pl-5"
        >
          <span aria-hidden className={cn('absolute inset-y-0 left-0 w-0.5', nextMeta.bg)} />
          <div className="min-w-0 flex-1 space-y-1">
            <p className="font-display text-muted-foreground text-[0.6875rem] leading-none font-semibold tracking-[0.08em] uppercase">
              {t.home.nextStepTitle}
            </p>
            <h2 id="home-next-step-title" className="font-display text-[1.125rem] font-semibold">
              {t.home.nextStep[next].title}
            </h2>
            <p className="text-muted-foreground text-[0.8125rem]">{t.home.nextStep[next].desc}</p>
          </div>
          <Button asChild>
            <ViewLink view={next} onNavigate={onNavigate} data-testid="home-next-step-link">
              {t.home.nextStep[next].cta}
              <ChevronRight aria-hidden />
            </ViewLink>
          </Button>
        </section>
      )}
    </div>
  );
}
