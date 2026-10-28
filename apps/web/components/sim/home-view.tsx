'use client';

import React from 'react';
import { Car, CloudUpload, Cog, Gauge, Waves, Wind } from 'lucide-react';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { Card } from '@sim-tuning/ui/components/ui/card';
import { toast } from '@sim-tuning/ui/components/ui/sonner';
import { useSimData } from '@/providers/sim/sim-data-context';
import { useLocale } from '@/providers/i18n/locale-context';

import type { ModuleTab } from './pwa-shell';

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
      <div>
        <h1 className="font-display text-2xl font-bold tracking-tight sm:text-3xl">
          {t.home.title}
        </h1>
        <p className="text-muted-foreground text-sm">{t.home.subtitle}</p>
      </div>

      <GuestImportCard />

      <section aria-labelledby="home-modules-title" className="space-y-3">
        <h2 id="home-modules-title" className="text-[0.9375rem] font-semibold">
          {t.home.modulesTitle}
        </h2>
        <ul className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          {MODULES.map(({ key, icon: Icon }) => (
            <li key={key}>
              <button
                type="button"
                data-testid={`home-module-${key}`}
                onClick={() => onNavigate(key)}
                className="h-full w-full text-left"
              >
                <Card className="hover:bg-secondary/40 flex h-full flex-col gap-2 rounded-[1.25rem] p-5 transition-colors">
                  <span className="flex items-center gap-2.5">
                    <Icon className="text-primary h-5 w-5" />
                    <span className="font-semibold">{t.home.modules[key].title}</span>
                  </span>
                  <p className="text-muted-foreground text-[0.8125rem]">
                    {t.home.modules[key].desc}
                  </p>
                </Card>
              </button>
            </li>
          ))}
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
    <Card data-testid="guest-import-card" className="space-y-3 rounded-[1.25rem] p-5">
      <h2 className="flex items-center gap-2 text-[0.9375rem] font-semibold">
        <CloudUpload className="text-primary h-5 w-5" />
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
