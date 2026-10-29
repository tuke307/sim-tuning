'use client';

import React, { useCallback, useEffect, useSyncExternalStore } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { Car, CloudOff, Cog, Gauge, UserRound, Waves, Wind } from 'lucide-react';
import { Button } from '@sim-tuning/ui/components/ui/button';
import BrandLogo from '@/components/brand/BrandLogo';
import { useAuth } from '@/providers/auth/auth-context';
import { useLocale } from '@/providers/i18n/locale-context';
import { SimDataProvider } from '@/providers/sim/sim-data-context';
import { HomeView } from './home-view';
import { SettingsView } from './settings-view';
import { VehiclesView } from './vehicles/vehicles-view';
import { MotorView } from './motor/motor-view';
import { EinlassView } from './einlass/einlass-view';
import { AuslassView } from './auslass/auslass-view';
import { DynoView } from './dyno/dyno-view';
import { MODULE_META, type SimModule } from './shared/modules';
import { confirmLeave } from '@/lib/navigation/leave-guard';
import { NAV_SUBTAB_PARAM, NAV_VIEW_PARAM } from '@/lib/navigation/nav-state';
import { navViewHref, parseNavView } from './home/nav-view';

export type ModuleTab = 'vehicles' | 'motor' | 'einlass' | 'auslass' | 'dyno';
export type NavTab = 'home' | ModuleTab | 'profile';

function subscribeOnline(onChange: () => void): () => void {
  window.addEventListener('online', onChange);
  window.addEventListener('offline', onChange);
  return () => {
    window.removeEventListener('online', onChange);
    window.removeEventListener('offline', onChange);
  };
}

export function PwaShell() {
  return (
    <SimDataProvider>
      <Shell />
    </SimDataProvider>
  );
}

function Shell() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const activeTab = parseNavView(searchParams.get(NAV_VIEW_PARAM));
  const { isAuthenticated, user } = useAuth();
  const { t, format } = useLocale();

  // Every top-level navigation is a history entry (back/forward, reload and links restore it) and
  // waits for the unsaved-changes guard. Re-selecting the current view only resets its sub-tab.
  const setActiveTab = useCallback(
    (next: NavTab) => {
      const params = new URLSearchParams(window.location.search);
      const sameView = parseNavView(params.get(NAV_VIEW_PARAM)) === next;
      if (sameView && !params.has(NAV_SUBTAB_PARAM)) return;
      void confirmLeave().then((ok) => {
        if (!ok) return;
        if (sameView) router.replace(navViewHref(next), { scroll: false });
        else router.push(navViewHref(next), { scroll: false });
      });
    },
    [router],
  );

  useEffect(() => {
    window.scrollTo(0, 0);
  }, [activeTab]);

  const isOnline = useSyncExternalStore(
    subscribeOnline,
    () => navigator.onLine,
    () => true,
  );

  const navItems: Array<{
    id: NavTab;
    label: string;
    icon: React.ComponentType<{ className?: string; strokeWidth?: number }>;
  }> = [
    { id: 'vehicles', label: t.vehicles.title, icon: Car },
    { id: 'motor', label: t.motor.title, icon: Cog },
    { id: 'einlass', label: t.einlass.title, icon: Wind },
    { id: 'auslass', label: t.auslass.title, icon: Waves },
    { id: 'dyno', label: t.dyno.title, icon: Gauge },
  ];

  const viewLabel =
    activeTab === 'home'
      ? null
      : activeTab === 'profile'
        ? t.nav.profile
        : navItems.find((item) => item.id === activeTab)?.label;

  useEffect(() => {
    document.title = viewLabel
      ? format(t.nav.documentTitle, { view: viewLabel })
      : `SimTuning · ${t.nav.tagline}`;
  }, [viewLabel, format, t.nav.documentTitle, t.nav.tagline]);

  const syncStatusText = !isOnline
    ? `${t.shell.offline}. ${t.shell.offlineReady}`
    : !isAuthenticated
      ? `${t.shell.guest}. ${t.shell.guestLocalOnly}`
      : t.shell.synced;

  const initials = (user?.name ?? '')
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase())
    .join('');

  return (
    <div className="bg-background text-foreground flex min-h-dvh w-full max-w-full flex-col overflow-x-hidden">
      <header className="bg-background pt-safe sticky top-0 z-40 border-b">
        <div className="mx-auto flex h-14 max-w-5xl items-stretch justify-between gap-3 px-4">
          <Button
            type="button"
            variant="ghost"
            onClick={() => setActiveTab('home')}
            aria-label={t.a11y.homeButton}
            className="font-display h-auto shrink-0 gap-2 self-center px-1 text-[1.125rem] font-bold tracking-[0.12em] uppercase hover:bg-transparent"
          >
            <BrandLogo variant="mark" size={28} decorative />
            SimTuning
          </Button>

          <nav aria-label={t.nav.primaryNavAria} className="hidden items-stretch md:flex">
            {navItems.map((item) => {
              const isCurrent = activeTab === item.id;
              const meta = MODULE_META[item.id as SimModule];
              return (
                <button
                  key={item.id}
                  type="button"
                  data-testid={`desktop-nav-${item.id}`}
                  aria-current={isCurrent ? 'page' : undefined}
                  onClick={() => setActiveTab(item.id)}
                  className={`font-display relative flex items-center gap-2 px-3.5 text-[0.8125rem] font-semibold tracking-[0.08em] uppercase outline-none focus-visible:outline-2 focus-visible:-outline-offset-2 focus-visible:outline-ring lg:px-4 ${
                    isCurrent ? 'text-foreground' : 'text-muted-foreground hover:text-foreground'
                  }`}
                >
                  <span className="font-data text-[0.6875rem] font-normal tracking-normal opacity-80">
                    {meta.sheet}
                  </span>
                  {item.label}
                  <span
                    aria-hidden
                    className={`absolute inset-x-2 bottom-0 h-0.5 ${isCurrent ? meta.bg : 'bg-transparent'}`}
                  />
                </button>
              );
            })}
          </nav>

          <div className="flex min-w-0 items-center gap-2">
            <span role="status" data-testid="header-sync-status" className="sr-only">
              {syncStatusText}
            </span>

            {!isOnline && (
              <span
                data-testid="header-sync-badge"
                title={syncStatusText}
                className="text-warning flex h-11 min-w-11 items-center justify-center gap-1.5 px-2 text-[0.75rem] font-semibold tracking-[0.06em] uppercase"
              >
                <CloudOff className="h-4 w-4" aria-hidden />
                <span className="hidden sm:inline">{t.shell.offline}</span>
              </span>
            )}

            <button
              type="button"
              data-testid="nav-profile"
              aria-label={t.nav.profileAria}
              aria-current={activeTab === 'profile' ? 'page' : undefined}
              onClick={() => setActiveTab('profile')}
              className="flex size-11 shrink-0 items-center justify-center rounded-md outline-none focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring"
            >
              <span
                className={`font-data flex size-9 items-center justify-center rounded-full border text-[0.8125rem] font-semibold ${
                  activeTab === 'profile'
                    ? 'border-primary text-primary'
                    : isAuthenticated && initials
                      ? 'border-foreground/60 text-foreground'
                      : 'border-border text-muted-foreground'
                }`}
              >
                {isAuthenticated && initials ? (
                  initials
                ) : (
                  <UserRound className="h-[18px] w-[18px]" strokeWidth={1.75} aria-hidden />
                )}
              </span>
            </button>
          </div>
        </div>
      </header>

      <main className="mx-auto w-full max-w-5xl min-w-0 flex-1 overflow-x-hidden px-4 pt-5 pb-28 sm:pt-8 md:pb-12">
        {activeTab === 'home' && <HomeView onNavigate={setActiveTab} />}
        {activeTab === 'vehicles' && <VehiclesView />}
        {activeTab === 'motor' && <MotorView />}
        {activeTab === 'einlass' && <EinlassView />}
        {activeTab === 'auslass' && <AuslassView />}
        {activeTab === 'dyno' && <DynoView />}
        {activeTab === 'profile' && <SettingsView />}
      </main>

      <nav
        aria-label={t.nav.mobileNavAria}
        className="bg-card fixed inset-x-0 bottom-0 z-40 border-t pb-[env(safe-area-inset-bottom)] md:hidden"
      >
        <div className="grid" style={{ gridTemplateColumns: `repeat(${navItems.length}, 1fr)` }}>
          {navItems.map((item) => {
            const Icon = item.icon;
            const isCurrent = activeTab === item.id;
            const meta = MODULE_META[item.id as SimModule];
            return (
              <button
                key={item.id}
                type="button"
                data-testid={`mobile-nav-${item.id}`}
                aria-current={isCurrent ? 'page' : undefined}
                onClick={() => setActiveTab(item.id)}
                className={`relative flex h-14 min-w-0 flex-col items-center justify-center gap-1 text-[0.6875rem] font-semibold tracking-[0.04em] uppercase outline-none focus-visible:outline-2 focus-visible:-outline-offset-2 focus-visible:outline-ring ${
                  isCurrent ? 'text-foreground' : 'text-muted-foreground'
                }`}
              >
                <span
                  aria-hidden
                  className={`absolute inset-x-3 top-0 h-0.5 ${isCurrent ? meta.bg : 'bg-transparent'}`}
                />
                <Icon className="h-5 w-5" strokeWidth={isCurrent ? 2 : 1.5} />
                <span className="max-w-full truncate px-0.5">{item.label}</span>
              </button>
            );
          })}
        </div>
      </nav>
    </div>
  );
}
