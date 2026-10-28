'use client';

import React, { useState, useSyncExternalStore } from 'react';
import { CloudOff, House, UserRound } from 'lucide-react';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { useAuth } from '@/providers/auth/auth-context';
import { useLocale } from '@/providers/i18n/locale-context';
import { HomeView } from './home-view';
import { SettingsView } from './settings-view';

export type NavTab = 'home' | 'profile';

function subscribeOnline(onChange: () => void): () => void {
  window.addEventListener('online', onChange);
  window.addEventListener('offline', onChange);
  return () => {
    window.removeEventListener('online', onChange);
    window.removeEventListener('offline', onChange);
  };
}

export function PwaShell() {
  const [activeTab, setActiveTab] = useState<NavTab>('home');
  const { isAuthenticated, user } = useAuth();
  const { t } = useLocale();
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
    { id: 'home', label: t.nav.home, icon: House },
    { id: 'profile', label: t.nav.profile, icon: UserRound },
  ];

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
      <header className="material-bar pt-safe sticky top-0 z-40 border-b border-border/60">
        <div className="mx-auto flex h-14 max-w-5xl items-center justify-between gap-3 px-4">
          <Button
            type="button"
            variant="ghost"
            onClick={() => setActiveTab('home')}
            aria-label={t.a11y.homeButton}
            className="h-auto shrink-0 rounded-md px-0 text-[1.0625rem] font-semibold tracking-tight hover:bg-transparent"
          >
            SimTuning
          </Button>

          <nav aria-label={t.nav.primaryNavAria} className="hidden items-center gap-1 md:flex">
            {navItems.map((item) => {
              const isCurrent = activeTab === item.id;
              return (
                <button
                  key={item.id}
                  type="button"
                  data-testid={`desktop-nav-${item.id}`}
                  aria-current={isCurrent ? 'page' : undefined}
                  onClick={() => setActiveTab(item.id)}
                  className={`h-9 rounded-full px-4 text-[0.9375rem] transition-colors duration-150 ${
                    isCurrent
                      ? 'bg-secondary text-foreground font-semibold'
                      : 'text-muted-foreground hover:text-foreground font-medium'
                  }`}
                >
                  {item.label}
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
                className="text-muted-foreground flex h-11 min-w-11 items-center justify-center gap-1.5 rounded-full px-2.5 text-[0.8125rem] font-medium"
              >
                <CloudOff className="h-4 w-4" />
                <span className="hidden sm:inline">{t.shell.offline}</span>
              </span>
            )}

            <button
              type="button"
              data-testid="nav-profile"
              aria-label={t.nav.profileAria}
              aria-current={activeTab === 'profile' ? 'page' : undefined}
              onClick={() => setActiveTab('profile')}
              className="flex size-11 shrink-0 items-center justify-center rounded-full"
            >
              <span
                className={`flex size-9 items-center justify-center rounded-full text-[0.8125rem] font-semibold transition-shadow ${
                  isAuthenticated && initials
                    ? 'bg-primary/12 text-primary'
                    : 'bg-secondary text-muted-foreground'
                } ${activeTab === 'profile' ? 'ring-primary ring-offset-background ring-2 ring-offset-2' : ''}`}
              >
                {isAuthenticated && initials ? (
                  initials
                ) : (
                  <UserRound className="h-[18px] w-[18px]" />
                )}
              </span>
            </button>
          </div>
        </div>
      </header>

      <main className="mx-auto w-full max-w-5xl min-w-0 flex-1 overflow-x-hidden px-4 pt-5 pb-28 sm:pt-8 md:pb-12">
        {activeTab === 'home' && <HomeView />}
        {activeTab === 'profile' && <SettingsView />}
      </main>

      <div className="pointer-events-none fixed inset-x-0 bottom-0 z-40 flex flex-col items-center gap-2 px-3 pb-[max(env(safe-area-inset-bottom),0.5rem)] md:pb-6">
        <nav
          aria-label={t.nav.mobileNavAria}
          className="material-bar pointer-events-auto w-full max-w-md rounded-full border border-border/60 p-1 shadow-[0_8px_30px_rgb(0_0_0/0.12)] md:hidden"
        >
          <div className="grid" style={{ gridTemplateColumns: `repeat(${navItems.length}, 1fr)` }}>
            {navItems.map((item) => {
              const Icon = item.icon;
              const isCurrent = activeTab === item.id;
              return (
                <button
                  key={item.id}
                  type="button"
                  data-testid={`mobile-nav-${item.id}`}
                  aria-current={isCurrent ? 'page' : undefined}
                  onClick={() => setActiveTab(item.id)}
                  className={`flex h-14 flex-col items-center justify-center gap-0.5 rounded-full text-[0.6875rem] font-medium transition-colors duration-150 ${
                    isCurrent ? 'bg-secondary text-primary' : 'text-muted-foreground'
                  }`}
                >
                  <Icon className="h-[22px] w-[22px]" strokeWidth={isCurrent ? 2.2 : 1.8} />
                  <span>{item.label}</span>
                </button>
              );
            })}
          </div>
        </nav>
      </div>
    </div>
  );
}
