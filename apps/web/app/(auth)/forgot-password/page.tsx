'use client';

import React, { useState } from 'react';
import Link from 'next/link';
import { Mail, CheckCircle2, ArrowLeft } from 'lucide-react';
import BrandLogo from '@/components/brand/BrandLogo';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { Card } from '@sim-tuning/ui/components/ui/card';
import { Input } from '@sim-tuning/ui/components/ui/input';
import { Label } from '@sim-tuning/ui/components/ui/label';
import { api } from '@/lib/api';
import { trackEvent } from '@/lib/analytics/track';
import { useLocale } from '@/providers/i18n/locale-context';

export default function ForgotPasswordPage() {
  const { t } = useLocale();
  const [email, setEmail] = useState('');
  const [submittedMessage, setSubmittedMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSubmittedMessage(null);
    setLoading(true);
    try {
      const res = await api.auth.forgotPassword({ email: email.trim() });
      if (res.success && res.data) {
        trackEvent('password_reset_request');
        setSubmittedMessage(res.data.message);
      } else {
        setError(res.error || 'Unable to process request');
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unable to send reset email');
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
          <h1 className="font-display text-2xl font-bold">{t.auth.forgotTitle}</h1>
          <p className="text-muted-foreground mt-1 text-xs leading-relaxed">{t.auth.forgotSub}</p>
        </div>

        {submittedMessage ? (
          <div className="space-y-4">
            <div className="border-primary/50 bg-primary/8 text-foreground flex items-start gap-2.5 rounded-md border p-4 text-xs leading-relaxed">
              <CheckCircle2 className="mt-0.5 h-4 w-4 shrink-0" />
              <span>{submittedMessage}</span>
            </div>
            <Button asChild variant="secondary" className="w-full">
              <Link href="/login">
                <ArrowLeft className="h-4 w-4" /> {t.auth.returnToSignIn}
              </Link>
            </Button>
          </div>
        ) : (
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-1.5">
              <Label htmlFor="reset-email" className="text-xs font-semibold">
                {t.auth.emailLabel}
              </Label>
              <Input
                id="reset-email"
                type="email"
                required
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="tuner@example.com"
                className="h-10"
              />
            </div>

            {error && <p className="text-destructive text-xs">{error}</p>}

            <Button type="submit" size="lg" disabled={loading} className="w-full">
              <Mail className="h-4 w-4" />
              {loading ? t.auth.sendingResetLinkBtn : t.auth.sendResetLinkBtn}
            </Button>

            <div className="border-t pt-3 text-center">
              <Button variant="link" asChild className="text-muted-foreground text-xs">
                <Link href="/login">
                  <ArrowLeft className="h-3.5 w-3.5" /> {t.auth.backToSignIn}
                </Link>
              </Button>
            </div>
          </form>
        )}
      </Card>
    </div>
  );
}
