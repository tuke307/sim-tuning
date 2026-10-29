'use client';

import React, { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { LogIn, UserPlus, MailCheck } from 'lucide-react';
import BrandLogo from '@/components/brand/BrandLogo';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { Card } from '@sim-tuning/ui/components/ui/card';
import { Input } from '@sim-tuning/ui/components/ui/input';
import { Label } from '@sim-tuning/ui/components/ui/label';
import { Tabs, TabsList, TabsTrigger } from '@sim-tuning/ui/components/ui/tabs';
import { useAuth } from '@/providers/auth/auth-context';
import { useLocale } from '@/providers/i18n/locale-context';

export default function LoginPage() {
  const router = useRouter();
  const { login, register, resendConfirmation, continueOfflineLocal, error, infoMessage, loading } =
    useAuth();
  const { t } = useLocale();
  const [mode, setMode] = useState<'login' | 'register'>('login');
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const ok =
      mode === 'login'
        ? await login(email, password)
        : await register({ email, password, name: name || undefined });
    if (ok) {
      router.push('/');
    }
  };

  return (
    <div className="bg-background flex min-h-dvh items-center justify-center p-4">
      <Card className="w-full max-w-md space-y-6 rounded-md p-6 sm:p-8">
        <div className="flex items-center justify-between">
          <Link href="/" className="flex items-center gap-2.5">
            <BrandLogo variant="mark" size={40} decorative />
            <div>
              <span className="font-display block text-[1.125rem] font-bold tracking-[0.12em] uppercase">
                SimTuning
              </span>
              <span className="text-muted-foreground block text-xs">{t.nav.tagline}</span>
            </div>
          </Link>
        </div>

        <Tabs value={mode} onValueChange={(val) => setMode(val as 'login' | 'register')}>
          <TabsList className="w-full">
            <TabsTrigger value="login">{t.auth.signInTab}</TabsTrigger>
            <TabsTrigger value="register">{t.auth.createAccountTab}</TabsTrigger>
          </TabsList>
        </Tabs>

        <form onSubmit={handleSubmit} className="space-y-4">
          {mode === 'register' && (
            <div className="space-y-1.5">
              <Label htmlFor="name" className="text-xs font-semibold">
                {t.auth.nameLabel}
              </Label>
              <Input
                id="name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="Tuner"
                className="h-10"
              />
            </div>
          )}

          <div className="space-y-1.5">
            <Label htmlFor="email" className="text-xs font-semibold">
              {t.auth.emailLabel}
            </Label>
            <Input
              id="email"
              type="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="tuner@example.com"
              className="h-10"
            />
          </div>

          <div className="space-y-1.5">
            <div className="flex items-center justify-between">
              <Label htmlFor="password" className="text-xs font-semibold">
                {t.auth.passwordLabel}
              </Label>
              {mode === 'login' && (
                <Link
                  href="/forgot-password"
                  className="text-primary text-xs font-semibold hover:underline"
                >
                  {t.auth.forgotPasswordLink}
                </Link>
              )}
            </div>
            <Input
              id="password"
              type="password"
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder={t.auth.passwordMinHint}
              className="h-10"
            />
          </div>

          {infoMessage && (
            <div className="border-primary/50 bg-primary/8 text-foreground space-y-2 rounded-md border p-3.5 text-xs">
              <div className="flex items-start gap-2">
                <MailCheck className="mt-0.5 h-4 w-4 shrink-0" />
                <span>{infoMessage}</span>
              </div>
              {email.trim() && (
                <Button
                  type="button"
                  variant="secondary"
                  size="xs"
                  disabled={loading}
                  onClick={() => void resendConfirmation(email.trim())}
                >
                  {t.profile.resendConfirmationBtn}
                </Button>
              )}
            </div>
          )}

          {error && (
            <div className="space-y-2">
              <p className="text-destructive text-xs">{error}</p>
              {error.toLowerCase().includes('confirm your email') && email.trim() && (
                <Button
                  type="button"
                  variant="secondary"
                  size="xs"
                  disabled={loading}
                  onClick={() => void resendConfirmation(email.trim())}
                >
                  <MailCheck className="h-3.5 w-3.5" /> {t.profile.resendConfirmationBtn}
                </Button>
              )}
            </div>
          )}

          <Button type="submit" size="lg" disabled={loading} className="w-full">
            {mode === 'login' ? (
              <>
                <LogIn className="h-4 w-4" /> {t.auth.signInBtn}
              </>
            ) : (
              <>
                <UserPlus className="h-4 w-4" /> {t.auth.registerBtn}
              </>
            )}
          </Button>
        </form>

        <div className="border-t pt-4 text-center">
          <Button
            type="button"
            variant="link"
            onClick={() => {
              continueOfflineLocal();
              router.push('/');
            }}
            className="text-muted-foreground hover:text-foreground text-xs font-semibold"
          >
            {t.auth.continueGuestBtn}
          </Button>
        </div>
      </Card>
    </div>
  );
}
