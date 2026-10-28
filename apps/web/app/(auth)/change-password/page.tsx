'use client';

import React, { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { Card } from '@sim-tuning/ui/components/ui/card';
import { Input } from '@sim-tuning/ui/components/ui/input';
import { Label } from '@sim-tuning/ui/components/ui/label';
import { api } from '@/lib/api';
import { useLocale } from '@/providers/i18n/locale-context';

export default function ChangePasswordPage() {
  const router = useRouter();
  const { t } = useLocale();
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setMessage(null);
    try {
      const res = await api.auth.changePassword({ currentPassword, newPassword });
      if (res.success) {
        setMessage('Password updated successfully.');
        setTimeout(() => router.push('/'), 1000);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update password');
    }
  };

  return (
    <div className="bg-background flex min-h-dvh items-center justify-center p-4">
      <Card className="w-full max-w-sm rounded-3xl p-6">
        <form onSubmit={handleSubmit} className="space-y-4">
          <h1 className="font-display text-xl font-bold">{t.auth.changePasswordTitle}</h1>
          <div className="space-y-1.5">
            <Label htmlFor="current-password" className="text-xs font-semibold">
              {t.auth.currentPasswordLabel}
            </Label>
            <Input
              id="current-password"
              type="password"
              required
              value={currentPassword}
              onChange={(e) => setCurrentPassword(e.target.value)}
              placeholder={t.auth.currentPasswordLabel}
              className="h-10"
            />
          </div>
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
          {error && <p className="text-destructive text-xs">{error}</p>}
          {message && <p className="text-primary text-xs">{message}</p>}
          <div className="flex items-center justify-between pt-2">
            <Button type="button" variant="ghost" size="sm" asChild>
              <Link href="/">{t.common.cancel}</Link>
            </Button>
            <Button type="submit">{t.auth.updatePasswordBtn}</Button>
          </div>
        </form>
      </Card>
    </div>
  );
}
