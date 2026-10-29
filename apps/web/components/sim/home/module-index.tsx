'use client';

import React from 'react';
import { Car, ChevronRight, Cog, Gauge, Waves, Wind } from 'lucide-react';
import { useLocale } from '@/providers/i18n/locale-context';
import type { ModuleTab } from '../pwa-shell';
import { MODULE_META } from '../shared/modules';
import { Section } from '../shared/module-page';
import { ViewLink, type NavigateFn } from './view-link';

const MODULES: Array<{
  key: ModuleTab;
  icon: React.ComponentType<{ className?: string }>;
}> = [
  { key: 'vehicles', icon: Car },
  { key: 'motor', icon: Cog },
  { key: 'einlass', icon: Wind },
  { key: 'auslass', icon: Waves },
  { key: 'dyno', icon: Gauge },
];

/** Compact Blattverzeichnis: one row per module sheet. */
export function ModuleIndex({ onNavigate }: { onNavigate: NavigateFn }) {
  const { t } = useLocale();
  return (
    <Section title={t.home.modulesTitle} bodyClassName="p-0 space-y-0">
      <ul className="divide-y">
        {MODULES.map(({ key, icon: Icon }) => {
          const meta = MODULE_META[key];
          return (
            <li key={key}>
              <ViewLink
                view={key}
                onNavigate={onNavigate}
                data-testid={`home-module-${key}`}
                className="group hover:bg-raised grid min-h-12 w-full grid-cols-[2.75rem_minmax(0,1fr)_auto] items-center gap-x-3 py-2 pr-4 text-left outline-none focus-visible:outline-2 focus-visible:-outline-offset-2 focus-visible:outline-ring sm:grid-cols-[2.75rem_11rem_minmax(0,1fr)_auto]"
              >
                <span className="flex justify-center">
                  <span
                    className={`font-data text-muted-foreground border-t-2 px-1 pt-0.5 text-[0.75rem] ${meta.border}`}
                  >
                    {String(meta.sheet).padStart(2, '0')}
                  </span>
                </span>
                <span className="flex min-w-0 items-center gap-2.5">
                  <Icon className={`h-4 w-4 shrink-0 ${meta.text}`} aria-hidden />
                  <span className="font-display truncate text-[0.9375rem] font-semibold tracking-[0.06em] uppercase">
                    {t.home.modules[key].title}
                  </span>
                </span>
                <span className="text-muted-foreground hidden truncate text-[0.8125rem] sm:block">
                  {t.home.modules[key].desc}
                </span>
                <ChevronRight
                  className="text-muted-foreground group-hover:text-foreground h-4 w-4"
                  aria-hidden
                />
              </ViewLink>
            </li>
          );
        })}
      </ul>
    </Section>
  );
}
