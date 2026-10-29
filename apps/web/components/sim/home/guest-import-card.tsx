'use client';

import React from 'react';
import { CloudUpload } from 'lucide-react';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { Card } from '@sim-tuning/ui/components/ui/card';
import { toast } from '@sim-tuning/ui/components/ui/sonner';
import { useSimData } from '@/providers/sim/sim-data-context';
import { useLocale } from '@/providers/i18n/locale-context';
import { ConfirmDestructiveDialog } from '../confirm-destructive-dialog';

/** Offers to move guest-mode data into the account right after signing in (Phase 10). */
export function GuestImportCard() {
  const { t, format } = useLocale();
  const { pendingGuestData, importGuestData, discardGuestData } = useSimData();
  const [busy, setBusy] = React.useState(false);
  const [confirmOpen, setConfirmOpen] = React.useState(false);
  if (!pendingGuestData) return null;
  const counts = {
    vehicles: pendingGuestData.vehicles.length,
    dynos: pendingGuestData.dynos.length,
  };
  return (
    <Card data-testid="guest-import-card" className="space-y-3 p-4">
      <h2 className="flex items-center gap-2 text-[0.9375rem] font-semibold">
        <CloudUpload className="text-primary h-5 w-5" aria-hidden />
        {t.home.guestImportTitle}
      </h2>
      <p className="text-muted-foreground text-[0.8125rem]">
        {format(t.home.guestImportDesc, counts)}
      </p>
      <div className="flex flex-wrap gap-2">
        <Button
          type="button"
          disabled={busy}
          onClick={async () => {
            setBusy(true);
            try {
              const result = await importGuestData();
              toast.success(format(t.home.guestImportDone, { vehicles: result.vehicles }));
            } catch {
              toast.error(t.sim.errorGeneric);
            } finally {
              setBusy(false);
            }
          }}
        >
          {t.home.guestImportBtn}
        </Button>
        <Button
          type="button"
          variant="secondary"
          disabled={busy}
          onClick={() => setConfirmOpen(true)}
        >
          {t.home.guestDiscardBtn}
        </Button>
      </div>
      <ConfirmDestructiveDialog
        open={confirmOpen}
        onOpenChange={setConfirmOpen}
        title={t.home.guestDiscardConfirmTitle}
        description={format(t.home.guestDiscardConfirmDesc, counts)}
        confirmLabel={t.home.guestDiscardConfirmBtn}
        testId="guest-discard-dialog"
        onConfirm={async () => {
          setBusy(true);
          try {
            await discardGuestData();
            toast.success(t.home.guestDiscardDone);
          } catch {
            toast.error(t.sim.errorGeneric);
          } finally {
            setBusy(false);
          }
        }}
      />
    </Card>
  );
}
