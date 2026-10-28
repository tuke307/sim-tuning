'use client';

import React from 'react';
import { Car, Cog, Gauge, Waves, Wind } from 'lucide-react';
import { Badge } from '@sim-tuning/ui/components/ui/badge';
import { Card } from '@sim-tuning/ui/components/ui/card';
import { useLocale } from '@/providers/i18n/locale-context';

type ModuleKey = 'vehicles' | 'motor' | 'einlass' | 'auslass' | 'dyno';

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

export function HomeView() {
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
              <Card
                data-testid={`home-module-${key}`}
                className="flex h-full flex-col gap-2 rounded-[1.25rem] p-5"
              >
                <div className="flex items-center justify-between gap-2">
                  <span className="flex items-center gap-2.5">
                    <Icon className="text-primary h-5 w-5" />
                    <span className="font-semibold">{t.home.modules[key].title}</span>
                  </span>
                  <Badge variant="secondary" className="rounded-full text-xs">
                    {t.home.comingSoon}
                  </Badge>
                </div>
                <p className="text-muted-foreground text-[0.8125rem]">{t.home.modules[key].desc}</p>
              </Card>
            </li>
          ))}
        </ul>
      </section>
    </div>
  );
}
