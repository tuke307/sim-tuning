'use client';

import React, { useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { Gauge, Trash2, CheckCircle2, AlertTriangle } from 'lucide-react';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { Card } from '@sim-tuning/ui/components/ui/card';
import { useAuth } from '@/providers/auth/auth-context';

export default function DeleteAccountPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const token = searchParams.get('token') ?? '';
  const { confirmAccountDeletion, error, loading } = useAuth();

  const [deletedSuccess, setDeletedSuccess] = useState(false);

  const handleConfirmDelete = async () => {
    if (!token) return;
    const ok = await confirmAccountDeletion(token);
    if (ok) {
      setDeletedSuccess(true);
    }
  };

  return (
    <div className="bg-background flex min-h-dvh items-center justify-center p-4">
      <Card className="w-full max-w-md space-y-6 rounded-md p-6 sm:p-8">
        <div className="flex items-center justify-between">
          <Link href="/" className="flex items-center gap-2.5">
            <div className="border-primary text-primary flex h-10 w-10 items-center justify-center rounded-md border">
              <Gauge className="h-5 w-5" />
            </div>
            <div>
              <span className="font-display block text-[1.125rem] font-bold tracking-[0.12em] uppercase">
                SimTuning
              </span>
              <span className="text-muted-foreground block text-xs">
                Account Deletion Confirmation
              </span>
            </div>
          </Link>
        </div>

        {!token ? (
          <div className="space-y-4">
            <div className="border-destructive/60 bg-destructive/8 text-destructive flex items-start gap-2.5 rounded-md border p-3.5 text-xs">
              <AlertTriangle className="mt-0.5 h-4 w-4 shrink-0" />
              <div>
                <p className="font-bold">Missing Account Deletion Token</p>
                <p className="mt-0.5 opacity-90">
                  No account deletion token was found in the URL. Please request a new deletion
                  confirmation email from your Profile settings.
                </p>
              </div>
            </div>
            <Button asChild variant="secondary" className="w-full">
              <Link href="/">Back to SimTuning</Link>
            </Button>
          </div>
        ) : deletedSuccess ? (
          <div className="space-y-4 py-4 text-center">
            <div className="border-primary text-primary mx-auto flex border h-14 w-14 items-center justify-center rounded-full">
              <CheckCircle2 className="h-8 w-8" />
            </div>
            <h1 className="font-display text-2xl font-bold">Account Permanently Deleted</h1>
            <p className="text-muted-foreground text-xs leading-relaxed">
              Your SimTuning account and all associated vehicles, engine setups, and dyno runs have
              been permanently deleted.
            </p>
            <Button type="button" size="lg" onClick={() => router.push('/')} className="w-full">
              Return to SimTuning (Guest Mode) →
            </Button>
          </div>
        ) : (
          <div className="space-y-5">
            <div className="space-y-2">
              <h1 className="font-display text-2xl font-bold">
                Confirm Permanent Account Deletion
              </h1>
              <p className="text-muted-foreground text-xs leading-relaxed">
                Please confirm in your browser that you want to permanently delete your SimTuning
                account. This action is irreversible and will immediately erase:
              </p>
              <ul className="text-muted-foreground list-disc space-y-1 pl-5 text-xs">
                <li>Your account profile, credentials, and preferences</li>
                <li>All vehicles with their engine, intake, and exhaust setups</li>
                <li>All dyno runs, recordings, and measurement history</li>
              </ul>
            </div>

            {error && (
              <div className="border-destructive/60 bg-destructive/8 text-destructive flex items-start gap-2.5 rounded-md border p-3.5 text-xs">
                <AlertTriangle className="mt-0.5 h-4 w-4 shrink-0" />
                <span>{error}</span>
              </div>
            )}

            <div className="flex flex-col gap-2.5 pt-1">
              <Button
                type="button"
                variant="destructive"
                size="lg"
                disabled={loading}
                onClick={() => void handleConfirmDelete()}
                className="w-full"
              >
                <Trash2 className="h-4 w-4" />
                {loading ? 'Deleting Account...' : 'Yes, Permanently Delete My Account'}
              </Button>
              <Button type="button" variant="outline" asChild disabled={loading} className="w-full">
                <Link href="/">Cancel & Keep My Account</Link>
              </Button>
            </div>
          </div>
        )}
      </Card>
    </div>
  );
}
