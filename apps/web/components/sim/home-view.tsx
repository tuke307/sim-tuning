'use client';

import React from 'react';
import { Car, ChevronRight, CloudUpload, Cog, Gauge, Waves, Wind } from 'lucide-react';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { Card } from '@sim-tuning/ui/components/ui/card';
import { toast } from '@sim-tuning/ui/components/ui/sonner';
import { useSimData } from '@/providers/sim/sim-data-context';
import { useLocale } from '@/providers/i18n/locale-context';

import type { ModuleTab } from './pwa-shell';
import { MODULE_META } from './shared/modules';

type ModuleKey = ModuleTab;

const MODULES: Array<{
  key: ModuleKey;
  icon: React.ComponentType<{ className?: string }>;
}> = [
  { key: 'vehicles', icon: Car },
  { key: 'motor', icon: Cog },
  { key: 'einlass', icon: Wind },
  { key: 'auslass', icon: Waves },
  { key: 'dyno', icon: Gauge },
];

export function HomeView({ onNavigate }: { onNavigate: (tab: ModuleKey) => void }) {
  const { t } = useLocale();

  return (
    <div className="space-y-6 pb-12">
      <div className="space-y-1.5">
        <h1 className="font-display text-[2rem] leading-none font-bold tracking-[0.08em] sm:text-[2.5rem]">
          {t.home.title}
        </h1>
        <p className="text-muted-foreground max-w-[60ch] text-[0.9375rem]">{t.home.subtitle}</p>
      </div>

      <GuestImportCard />

      {/* Blattverzeichnis: the module list as a drawing index. */}
      <section aria-labelledby="home-modules-title" className="bg-card rounded-md border">
        <h2
          id="home-modules-title"
          className="font-display border-b px-4 py-2.5 text-[0.875rem] font-semibold"
        >
          {t.home.modulesTitle}
        </h2>
        <ul className="divide-y">
          {MODULES.map(({ key, icon: Icon }) => {
            const meta = MODULE_META[key];
            return (
              <li key={key}>
                <button
                  type="button"
                  data-testid={`home-module-${key}`}
                  onClick={() => onNavigate(key)}
                  className="group hover:bg-raised grid min-h-16 w-full grid-cols-[3rem_minmax(0,1fr)_auto] items-center gap-x-3 py-3 pr-4 text-left outline-none focus-visible:outline-2 focus-visible:-outline-offset-2 focus-visible:outline-ring sm:grid-cols-[3.5rem_12rem_minmax(0,1fr)_auto]"
                >
                  <span className="row-span-2 flex justify-center self-center sm:row-span-1">
                    <span
                      className={`font-data text-muted-foreground border-t-2 px-1 pt-1 text-[0.8125rem] ${meta.border}`}
                    >
                      {String(meta.sheet).padStart(2, '0')}
                    </span>
                  </span>
                  <span className="flex min-w-0 items-center gap-2.5">
                    <Icon className={`h-5 w-5 shrink-0 ${meta.text}`} aria-hidden />
                    <span className="font-display text-[1rem] font-semibold tracking-[0.06em] uppercase">
                      {t.home.modules[key].title}
                    </span>
                  </span>
                  <span className="text-muted-foreground col-start-2 row-start-2 pt-0.5 text-[0.8125rem] sm:col-start-3 sm:row-start-1 sm:pt-0">
                    {t.home.modules[key].desc}
                  </span>
                  <ChevronRight
                    className="text-muted-foreground group-hover:text-foreground col-start-3 row-span-2 row-start-1 h-4 w-4 sm:col-start-4 sm:row-span-1"
                    aria-hidden
                  />
                </button>
              </li>
            );
          })}
        </ul>
      </section>
    </div>
  );
}

/** Offers to move guest-mode data into the account right after signing in (Phase 10). */
function GuestImportCard() {
  const { t, format } = useLocale();
  const { pendingGuestData, importGuestData, discardGuestData } = useSimData();
  const [busy, setBusy] = React.useState(false);
  if (!pendingGuestData) return null;
  const counts = {
    vehicles: pendingGuestData.vehicles.length,
    dynos: pendingGuestData.dynos.length,
  };
  return (
    <Card data-testid="guest-import-card" className="space-y-3 p-4">
      <h2 className="flex items-center gap-2 text-[0.9375rem] font-semibold">
        <CloudUpload className="text-primary h-5 w-5" aria-hidden />
        {t.home.guestImportTitle}
      </h2>
      <p className="text-muted-foreground text-[0.8125rem]">
        {format(t.home.guestImportDesc, counts)}
      </p>
      <div className="flex flex-wrap gap-2">
        <Button
          type="button"
          disabled={busy}
          onClick={async () => {
            setBusy(true);
            try {
              const result = await importGuestData();
              toast.success(format(t.home.guestImportDone, { vehicles: result.vehicles }));
            } catch {
              toast.error(t.sim.errorGeneric);
            } finally {
              setBusy(false);
            }
          }}
        >
          {t.home.guestImportBtn}
        </Button>
        <Button
          type="button"
          variant="secondary"
          disabled={busy}
          onClick={() => void discardGuestData()}
        >
          {t.home.guestDiscardBtn}
        </Button>
      </div>
    </Card>
  );
}
