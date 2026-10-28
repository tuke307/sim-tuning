'use client';

import React from 'react';
import { Car, Cog, Gauge, Waves, Wind } from 'lucide-react';
import { Card } from '@sim-tuning/ui/components/ui/card';
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
