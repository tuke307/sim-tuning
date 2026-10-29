'use client';

import React, { useState } from 'react';
import { Copy, Plus } from 'lucide-react';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { toast } from '@sim-tuning/ui/components/ui/sonner';
import { Spinner } from '@sim-tuning/ui/components/ui/spinner';
import { cn } from '@sim-tuning/ui/lib/utils';
import { useLocale } from '@/providers/i18n/locale-context';
import { useSimData } from '@/providers/sim/sim-data-context';
import { MODULE_META } from '../shared/modules';
import { ViewLink, type NavigateFn } from './view-link';

/** First run: no own vehicle yet, so one tap turns a starter vehicle into an editable copy. */
export function FirstRun({ onNavigate }: { onNavigate: NavigateFn }) {
  const { t, format, bcp47 } = useLocale();
  const { vehicles, repository, setVehicle, selectVehicle } = useSimData();
  const [busyId, setBusyId] = useState<string | null>(null);
  const starters = vehicles.filter((v) => v.isSystem);
  const cm3 = new Intl.NumberFormat(bcp47, { maximumFractionDigits: 1 });

  async function adoptStarter(id: string) {
    setBusyId(id);
    try {
      const copy = await repository.duplicateVehicle(id);
      setVehicle(copy);
      selectVehicle(copy.id);
      toast.success(format(t.home.starterCopied, { name: copy.name }));
    } catch {
      toast.error(t.sim.errorGeneric);
    } finally {
      setBusyId(null);
    }
  }

  return (
    <section
      aria-labelledby="home-first-run-title"
      data-testid="home-first-run"
      className="bg-card relative overflow-hidden rounded-md border pt-0.5"
    >
      <span aria-hidden className={cn('absolute inset-x-0 top-0 h-0.5', MODULE_META.vehicles.bg)} />
      <div className="space-y-1.5 border-b px-4 py-3">
        <h1
          id="home-first-run-title"
          className="font-display text-[1.5rem] leading-tight font-bold sm:text-[2rem]"
        >
          {t.home.firstRunTitle}
        </h1>
        <p className="text-muted-foreground max-w-[60ch] text-[0.9375rem]">{t.home.firstRunDesc}</p>
      </div>

      {starters.length > 0 && (
        <ul className="divide-y">
          {starters.map((vehicle) => {
            const engine = vehicle.motor?.name?.trim();
            const hubraumV = vehicle.motor?.hubraumV;
            const busy = busyId === vehicle.id;
            return (
              <li key={vehicle.id}>
                <button
                  type="button"
                  data-testid={`home-starter-${vehicle.systemKey ?? vehicle.id}`}
                  disabled={busyId !== null}
                  aria-busy={busy || undefined}
                  onClick={() => void adoptStarter(vehicle.id)}
                  className="group hover:bg-raised grid min-h-16 w-full grid-cols-[minmax(0,1fr)_auto] items-center gap-x-3 px-4 py-3 text-left outline-none focus-visible:outline-2 focus-visible:-outline-offset-2 focus-visible:outline-ring disabled:opacity-60"
                >
                  <span className="min-w-0 space-y-1">
                    <span className="font-display block truncate text-[1.0625rem] font-semibold tracking-[0.04em] uppercase">
                      {vehicle.name}
                    </span>
                    <span className="text-muted-foreground flex flex-wrap gap-x-2 text-[0.8125rem]">
                      <span>{t.home.starterBadge}</span>
                      {engine && <span className="font-data">· {engine}</span>}
                      {hubraumV !== null && hubraumV !== undefined && (
                        <span className="font-data">· {cm3.format(hubraumV / 1000)} cm³</span>
                      )}
                    </span>
                  </span>
                  {busy ? (
                    <Spinner className="h-4 w-4" />
                  ) : (
                    <Copy
                      className="text-muted-foreground group-hover:text-foreground h-4 w-4"
                      aria-hidden
                    />
                  )}
                </button>
              </li>
            );
          })}
        </ul>
      )}

      <div className="border-t p-4">
        <Button asChild variant="secondary">
          <ViewLink view="vehicles" onNavigate={onNavigate} data-testid="home-create-empty">
            <Plus aria-hidden />
            {t.home.createEmpty}
          </ViewLink>
        </Button>
      </div>
    </section>
  );
}
