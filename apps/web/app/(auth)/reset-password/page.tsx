'use client';

import React, { useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { KeyRound, CheckCircle2, AlertTriangle } from 'lucide-react';
import BrandLogo from '@/components/brand/BrandLogo';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { Card } from '@sim-tuning/ui/components/ui/card';
import { Input } from '@sim-tuning/ui/components/ui/input';
import { Label } from '@sim-tuning/ui/components/ui/label';
import { api } from '@/lib/api';
import { useLocale } from '@/providers/i18n/locale-context';

export default function ResetPasswordPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const token = searchParams.get('token') ?? '';
  const { t } = useLocale();

  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!token) {
      setError('Missing password reset token. Please use the link from your email.');
      return;
    }

    if (newPassword !== confirmPassword) {
      setError('Passwords do not match.');
      return;
    }

    setLoading(true);
    try {
      const res = await api.auth.resetPassword({ token, newPassword });
      if (res.success && res.data) {
        setSuccessMessage(res.data.message);
        setTimeout(() => router.push('/login'), 1800);
      } else {
        setError(res.error || 'Failed to reset password');
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to reset password');
    } finally {
      setLoading(false);
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

        <div>
          <h1 className="font-display text-2xl font-bold">{t.auth.resetTitle}</h1>
          <p className="text-muted-foreground mt-1 text-xs">{t.auth.resetSub}</p>
        </div>

        {!token ? (
          <div className="space-y-4">
            <div className="border-destructive/60 bg-destructive/8 text-destructive flex items-start gap-2.5 rounded-md border p-3.5 text-xs">
              <AlertTriangle className="mt-0.5 h-4 w-4 shrink-0" />
              <span>
                No password reset token was found in the URL. Please request a new password reset
                link.
              </span>
            </div>
            <Button asChild variant="secondary" className="w-full">
              <Link href="/forgot-password">{t.auth.sendResetLinkBtn}</Link>
            </Button>
          </div>
        ) : successMessage ? (
          <div className="space-y-4 py-2 text-center">
            <div className="border-primary text-primary mx-auto flex border h-12 w-12 items-center justify-center rounded-full">
              <CheckCircle2 className="h-7 w-7" />
            </div>
            <p className="text-sm font-bold">{successMessage}</p>
            <Button asChild size="lg" className="w-full">
              <Link href="/login">{t.auth.signInBtn} →</Link>
            </Button>
          </div>
        ) : (
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-1.5">
              <Label htmlFor="new-password" className="text-xs font-semibold">
                {t.auth.newPasswordLabel}
              </Label>
              <Input
                id="new-password"
                type="password"
                required
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                placeholder={t.auth.passwordMinHint}
                className="h-10"
              />
            </div>

            <div className="space-y-1.5">
              <Label htmlFor="confirm-password" className="text-xs font-semibold">
                {t.auth.confirmPasswordLabel}
              </Label>
              <Input
                id="confirm-password"
                type="password"
                required
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                placeholder={t.auth.confirmPasswordLabel}
                className="h-10"
              />
            </div>

            {error && <p className="text-destructive text-xs">{error}</p>}

            <Button type="submit" size="lg" disabled={loading} className="w-full">
              <KeyRound className="h-4 w-4" />
              {t.auth.saveNewPasswordBtn}
            </Button>
          </form>
        )}
      </Card>
    </div>
  );
}
