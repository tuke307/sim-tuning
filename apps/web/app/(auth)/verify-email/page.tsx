'use client';

import React, { useEffect, useRef, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { CheckCircle2, AlertTriangle, MailCheck, RefreshCw } from 'lucide-react';
import BrandLogo from '@/components/brand/BrandLogo';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { Card } from '@sim-tuning/ui/components/ui/card';
import { Input } from '@sim-tuning/ui/components/ui/input';
import { useAuth } from '@/providers/auth/auth-context';

export default function VerifyEmailPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const token = searchParams.get('token') ?? '';
  const { verifyEmail, resendConfirmation, error, infoMessage, loading } = useAuth();

  const [verifiedSuccess, setVerifiedSuccess] = useState(false);
  const [resendEmail, setResendEmail] = useState('');
  const attemptedRef = useRef(false);

  useEffect(() => {
    if (!token || attemptedRef.current) return;
    attemptedRef.current = true;
    void verifyEmail(token).then((ok) => {
      if (ok) {
        setVerifiedSuccess(true);
      }
    });
  }, [token, verifyEmail]);

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
              <span className="text-muted-foreground block text-[0.8125rem]">
                Account Email Confirmation
              </span>
            </div>
          </Link>
        </div>

        {loading && !verifiedSuccess ? (
          <div className="space-y-3 py-6 text-center">
            <RefreshCw className="text-primary mx-auto h-10 w-10 animate-spin" />
            <h1 className="font-display text-xl font-bold">Verifying Your Account...</h1>
            <p className="text-muted-foreground text-[0.8125rem]">
              Activating your SimTuning account.
            </p>
          </div>
        ) : verifiedSuccess ? (
          <div className="space-y-4 py-4 text-center">
            <div className="border-primary text-primary mx-auto flex border h-14 w-14 items-center justify-center rounded-full">
              <CheckCircle2 className="h-8 w-8" />
            </div>
            <h1 className="font-display text-2xl font-bold">Email Confirmed!</h1>
            <p className="text-muted-foreground text-[0.8125rem] leading-relaxed">
              Your SimTuning account is now active and signed in. Your vehicles and dyno runs will
              synchronize automatically across all your devices.
            </p>
            <Button type="button" size="lg" onClick={() => router.push('/')} className="w-full">
              Open SimTuning Dashboard →
            </Button>
          </div>
        ) : (
          <div className="space-y-4">
            <div className="border-destructive/60 bg-destructive/8 text-destructive flex items-start gap-2.5 rounded-md border p-3.5 text-[0.8125rem]">
              <AlertTriangle className="mt-0.5 h-4 w-4 shrink-0" />
              <div>
                <p className="font-bold">Confirmation Link Invalid or Expired</p>
                <p className="mt-0.5 opacity-90">
                  {error || 'No valid confirmation token was found in the URL.'}
                </p>
              </div>
            </div>

            {infoMessage && (
              <div className="border-primary/50 bg-primary/8 text-foreground flex items-center gap-2 rounded-md border p-3 text-[0.8125rem] font-medium">
                <MailCheck className="h-4 w-4 shrink-0" />
                <span>{infoMessage}</span>
              </div>
            )}

            <form
              onSubmit={async (e) => {
                e.preventDefault();
                if (!resendEmail.trim()) return;
                await resendConfirmation(resendEmail.trim());
              }}
              className="space-y-3 border-t pt-4"
            >
              <label
                htmlFor="resend-email"
                className="text-muted-foreground block text-[0.8125rem]"
              >
                Enter your email address below to receive a fresh 24-hour confirmation link:
              </label>
              <Input
                id="resend-email"
                name="email"
                autoComplete="email"
                type="email"
                required
                value={resendEmail}
                onChange={(e) => setResendEmail(e.target.value)}
                placeholder="tuner@example.com"
                className="h-10"
              />
              <Button type="submit" variant="secondary" disabled={loading} className="w-full">
                <MailCheck className="h-4 w-4" /> Resend Confirmation Email
              </Button>
            </form>

            <div className="border-t pt-3 text-center">
              <Button variant="link" asChild className="text-[0.8125rem]">
                <Link href="/login">Back to Sign In</Link>
              </Button>
            </div>
          </div>
        )}
      </Card>
    </div>
  );
}
