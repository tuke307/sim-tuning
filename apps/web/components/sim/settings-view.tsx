'use client';

import React, { useState, useSyncExternalStore } from 'react';
import Link from 'next/link';
import {
  User,
  LogOut,
  LogIn,
  UserPlus,
  MailCheck,
  KeyRound,
  Trash2,
  AlertTriangle,
  Globe,
  Ruler,
  ShieldCheck,
} from 'lucide-react';
import { useTheme } from 'next-themes';
import { Badge } from '@sim-tuning/ui/components/ui/badge';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { Card } from '@sim-tuning/ui/components/ui/card';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@sim-tuning/ui/components/ui/dialog';
import { Input } from '@sim-tuning/ui/components/ui/input';
import { Label } from '@sim-tuning/ui/components/ui/label';
import { NativeSelect } from '@sim-tuning/ui/components/ui/native-select';
import { Tabs, TabsList, TabsTrigger } from '@sim-tuning/ui/components/ui/tabs';
import { useAuth } from '@/providers/auth/auth-context';
import { useLocale } from '@/providers/i18n/locale-context';
import {
  isAnalyticsOptedOut,
  setAnalyticsOptOut,
  subscribeAnalyticsOptOut,
} from '@/lib/analytics/opt-out';
import { type LanguagePreference } from '@sim-tuning/common/enums';
import { ROUNDING_ACCURACY } from '@sim-tuning/common/schemas';

export function SettingsView() {
  const {
    user,
    authMode,
    login,
    register,
    resendConfirmation,
    requestAccountDeletion,
    logout,
    updatePreferences,
    error: authError,
    infoMessage: authInfoMessage,
    loading: authLoading,
  } = useAuth();
  const { theme, setTheme } = useTheme();
  const { t, languagePreference, setLanguagePreference } = useLocale();

  const [authTab, setAuthTab] = useState<'login' | 'register'>('login');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [name, setName] = useState('');
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [deletePassword, setDeletePassword] = useState('');
  const [deleteError, setDeleteError] = useState<string | null>(null);
  const analyticsOptedOut = useSyncExternalStore(
    subscribeAnalyticsOptOut,
    isAnalyticsOptedOut,
    () => false,
  );

  return (
    <div className="space-y-6 pb-12">
      <div>
        <h1 className="font-display text-2xl font-bold tracking-tight sm:text-3xl">
          {t.profile.title}
        </h1>
        <p className="text-muted-foreground text-sm">{t.profile.subtitle}</p>
      </div>

      {/* Account & Preferences Grid */}
      <div className="grid gap-4 lg:grid-cols-2">
        {/* Account & Session Security Card */}
        <Card className="space-y-4 rounded-[1.25rem] p-5">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2.5">
              <User className="text-primary h-5 w-5" />
              <h2 className="font-display text-lg font-bold">{t.profile.accountSectionTitle}</h2>
            </div>
            <Badge
              variant={authMode === 'AUTHENTICATED' ? 'success' : 'warning'}
              className="rounded-full px-3 py-1 text-xs"
            >
              {authMode === 'AUTHENTICATED'
                ? t.profile.authenticatedBadge
                : t.profile.offlineLocalBadge}
            </Badge>
          </div>

          {user && (
            <div className="bg-secondary/40 rounded-2xl p-3.5 text-xs">
              <p className="text-sm font-bold">{user.name || 'Tuner'}</p>
              <p className="text-muted-foreground">{user.email}</p>
              <p className="text-muted-foreground mt-1 text-xs">
                {authMode === 'AUTHENTICATED' ? t.profile.authenticatedDesc : t.profile.guestDesc}
              </p>
            </div>
          )}

          {authMode === 'AUTHENTICATED' ? (
            <div className="space-y-3">
              {authInfoMessage && (
                <div className="border-primary/40 bg-primary/12 text-primary flex items-start gap-2 rounded-2xl border p-3 text-xs">
                  <MailCheck className="mt-0.5 h-4 w-4 shrink-0" />
                  <span>{authInfoMessage}</span>
                </div>
              )}
              <div className="flex flex-wrap items-center gap-2">
                <Button type="button" variant="secondary" asChild>
                  <Link href="/change-password">
                    <KeyRound className="h-4 w-4" /> {t.profile.changePasswordBtn}
                  </Link>
                </Button>
                <Button
                  type="button"
                  variant="secondary"
                  onClick={() => void logout()}
                  className="hover:bg-destructive/20 hover:text-destructive w-fit"
                >
                  <LogOut className="h-4 w-4" /> {t.profile.signOutBtn}
                </Button>
                <Button
                  type="button"
                  variant="destructive"
                  onClick={() => {
                    setDeletePassword('');
                    setDeleteError(null);
                    setDeleteDialogOpen(true);
                  }}
                  className="w-fit"
                >
                  <Trash2 className="h-4 w-4" /> {t.profile.deleteAccountBtn}
                </Button>
              </div>
            </div>
          ) : (
            <form
              onSubmit={async (e) => {
                e.preventDefault();
                if (authTab === 'login') {
                  await login(email, password);
                } else {
                  await register({ email, password, name: name || undefined });
                }
              }}
              className="space-y-3 border-t pt-3"
            >
              <Tabs
                value={authTab}
                onValueChange={(val) => setAuthTab(val as 'login' | 'register')}
              >
                <TabsList className="w-full">
                  <TabsTrigger value="login">{t.profile.signInTab}</TabsTrigger>
                  <TabsTrigger value="register">{t.profile.registerTab}</TabsTrigger>
                </TabsList>
              </Tabs>

              {authTab === 'register' && (
                <Input
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  placeholder={t.profile.yourNamePlaceholder}
                  className="text-xs"
                />
              )}
              <Input
                type="email"
                required
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder={t.profile.emailPlaceholder}
                className="text-xs"
              />
              <Input
                type="password"
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder={t.profile.passwordPlaceholder}
                className="text-xs"
              />

              {authTab === 'login' && (
                <div className="flex justify-end">
                  <Link
                    href="/forgot-password"
                    className="text-primary text-xs font-semibold hover:underline"
                  >
                    {t.profile.forgotPasswordLink}
                  </Link>
                </div>
              )}

              {authInfoMessage && (
                <div className="border-primary/40 bg-primary/12 text-primary space-y-2 rounded-2xl border p-3 text-xs">
                  <div className="flex items-start gap-2">
                    <MailCheck className="mt-0.5 h-4 w-4 shrink-0" />
                    <span>{authInfoMessage}</span>
                  </div>
                  {email.trim() && (
                    <Button
                      type="button"
                      variant="secondary"
                      size="xs"
                      disabled={authLoading}
                      onClick={() => void resendConfirmation(email.trim())}
                    >
                      {t.profile.resendConfirmationBtn}
                    </Button>
                  )}
                </div>
              )}

              {authError && (
                <div className="space-y-2">
                  <p className="text-destructive text-xs">{authError}</p>
                  {authError.toLowerCase().includes('confirm your email') && email.trim() && (
                    <Button
                      type="button"
                      variant="secondary"
                      size="xs"
                      disabled={authLoading}
                      onClick={() => void resendConfirmation(email.trim())}
                    >
                      <MailCheck className="h-3.5 w-3.5" /> {t.profile.resendConfirmationBtn}
                    </Button>
                  )}
                </div>
              )}

              <Button type="submit" disabled={authLoading} className="w-full">
                {authTab === 'login' ? (
                  <>
                    <LogIn className="h-4 w-4" /> {t.profile.signInSyncBtn}
                  </>
                ) : (
                  <>
                    <UserPlus className="h-4 w-4" /> {t.profile.createAccountBtn}
                  </>
                )}
              </Button>
            </form>
          )}
        </Card>

        {/* Units & App Preferences Card */}
        <Card className="space-y-4 rounded-[1.25rem] p-5">
          <h2 className="font-display text-lg font-bold">{t.profile.preferencesTitle}</h2>

          <div className="space-y-3 text-xs">
            {/* Language Switcher */}
            <div className="flex flex-wrap items-center justify-between gap-2">
              <div>
                <p className="flex items-center gap-1.5 font-semibold">
                  <Globe className="text-primary h-3.5 w-3.5" />
                  {t.profile.languageLabel}
                </p>
              </div>
              <Tabs
                value={languagePreference}
                onValueChange={(val) => {
                  if (val === 'system' || val === 'en' || val === 'de') {
                    void setLanguagePreference(val as LanguagePreference);
                  }
                }}
              >
                <TabsList>
                  <TabsTrigger value="system">{t.profile.langSystem}</TabsTrigger>
                  <TabsTrigger value="en">{t.profile.langEn}</TabsTrigger>
                  <TabsTrigger value="de">{t.profile.langDe}</TabsTrigger>
                </TabsList>
              </Tabs>
            </div>

            <div className="flex flex-wrap items-center justify-between gap-2">
              <div>
                <p className="font-semibold">{t.profile.themeLabel}</p>
              </div>
              <Tabs
                value={theme ?? 'system'}
                onValueChange={(val) => {
                  setTheme(val);
                  if (val === 'dark' || val === 'light' || val === 'system') {
                    void updatePreferences({ theme: val });
                  }
                }}
              >
                <TabsList>
                  <TabsTrigger value="system">{t.profile.themeSystem}</TabsTrigger>
                  <TabsTrigger value="light">{t.profile.themeLight}</TabsTrigger>
                  <TabsTrigger value="dark">{t.profile.themeDark}</TabsTrigger>
                </TabsList>
              </Tabs>
            </div>

            <div className="flex flex-wrap items-center justify-between gap-2 border-t pt-3">
              <div>
                <p className="flex items-center gap-1.5 font-semibold">
                  <Ruler className="text-primary h-3.5 w-3.5" />
                  {t.profile.roundingAccuracyLabel}
                </p>
                <p className="text-muted-foreground text-xs">{t.profile.roundingAccuracySub}</p>
              </div>
              <NativeSelect
                aria-label={t.profile.roundingAccuracyLabel}
                data-testid="profile-rounding-accuracy"
                value={user?.preferences?.roundingAccuracy ?? ROUNDING_ACCURACY.default}
                onChange={(e) =>
                  void updatePreferences({ roundingAccuracy: Number(e.target.value) })
                }
                className="w-auto tabular-nums"
              >
                {Array.from(
                  { length: ROUNDING_ACCURACY.max - ROUNDING_ACCURACY.min + 1 },
                  (_, i) => ROUNDING_ACCURACY.min + i,
                ).map((digits) => (
                  <option key={digits} value={digits}>
                    {digits}
                  </option>
                ))}
              </NativeSelect>
            </div>

            <div className="flex flex-wrap items-center justify-between gap-2">
              <div>
                <p className="font-semibold">{t.profile.roundOnUnitChangeLabel}</p>
                <p className="text-muted-foreground text-xs">{t.profile.roundOnUnitChangeSub}</p>
              </div>
              <Tabs
                data-testid="profile-round-on-unit-change-tabs"
                value={(user?.preferences?.roundOnUnitChange ?? true) ? 'on' : 'off'}
                onValueChange={(val) => void updatePreferences({ roundOnUnitChange: val === 'on' })}
              >
                <TabsList>
                  <TabsTrigger value="on">{t.profile.enabledOption}</TabsTrigger>
                  <TabsTrigger value="off">{t.profile.disabledOption}</TabsTrigger>
                </TabsList>
              </Tabs>
            </div>

            {/* Anonymous usage statistics opt-out (device-local, stored in localStorage) */}
            <div className="flex flex-wrap items-center justify-between gap-2 border-t pt-3">
              <div>
                <p className="flex items-center gap-1.5 font-semibold">
                  <ShieldCheck className="text-primary h-3.5 w-3.5" />
                  {t.profile.usageStatsLabel}
                </p>
                <p className="text-muted-foreground text-xs">{t.profile.usageStatsSub}</p>
              </div>
              <Tabs
                data-testid="profile-usage-stats-tabs"
                value={analyticsOptedOut ? 'off' : 'on'}
                onValueChange={(val) => setAnalyticsOptOut(val === 'off')}
              >
                <TabsList>
                  <TabsTrigger value="on" data-testid="usage-stats-on">
                    {t.profile.enabledOption}
                  </TabsTrigger>
                  <TabsTrigger value="off" data-testid="usage-stats-off">
                    {t.profile.disabledOption}
                  </TabsTrigger>
                </TabsList>
              </Tabs>
            </div>
          </div>
        </Card>
      </div>

      <Dialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle className="text-destructive flex items-center gap-2">
              <AlertTriangle className="h-5 w-5 shrink-0" />
              {t.profile.deleteDialogTitle}
            </DialogTitle>
            <DialogDescription>{t.profile.deleteDialogDesc}</DialogDescription>
          </DialogHeader>

          <form
            onSubmit={async (e) => {
              e.preventDefault();
              setDeleteError(null);
              if (!deletePassword.trim()) {
                setDeleteError(t.profile.currentPasswordPlaceholder);
                return;
              }
              const ok = await requestAccountDeletion(deletePassword);
              if (ok) {
                setDeletePassword('');
                setDeleteDialogOpen(false);
              }
            }}
            className="space-y-4"
          >
            <div className="space-y-1.5">
              <Label htmlFor="delete-account-password" className="text-xs font-semibold">
                {t.profile.currentPasswordLabel}
              </Label>
              <Input
                id="delete-account-password"
                type="password"
                required
                value={deletePassword}
                onChange={(e) => setDeletePassword(e.target.value)}
                placeholder={t.profile.currentPasswordPlaceholder}
                className="h-10 text-xs"
              />
            </div>

            {(deleteError || authError) && (
              <p className="text-destructive text-xs">{deleteError || authError}</p>
            )}

            <DialogFooter>
              <Button
                type="button"
                variant="outline"
                onClick={() => setDeleteDialogOpen(false)}
                disabled={authLoading}
              >
                {t.common.cancel}
              </Button>
              <Button type="submit" variant="destructive" disabled={authLoading}>
                <MailCheck className="h-4 w-4" />
                {authLoading ? t.profile.sendingBtn : t.profile.sendDeletionLinkBtn}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
