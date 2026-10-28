'use client';

import React, { useState } from 'react';
import { Check, Loader2, Save } from 'lucide-react';
import type { VehicleInputDto } from '@sim-tuning/common/schemas';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { useLocale } from '@/providers/i18n/locale-context';
import { useSimData } from '@/providers/sim/sim-data-context';

type Status = 'idle' | 'saving' | 'saved' | 'error';

/**
 * Saves a partial nested vehicle update built by the caller and pushes the result into the
 * shared vehicle cache. Disabled for starter (read-only) vehicles.
 */
export function SaveMotorButton({
  buildUpdate,
  hint,
  disabled,
  testId,
}: {
  buildUpdate: (motorName: string) => Partial<VehicleInputDto> | null;
  hint?: string;
  disabled?: boolean;
  testId?: string;
}) {
  const { t } = useLocale();
  const { repository, selectedVehicle, setVehicle } = useSimData();
  const [status, setStatus] = useState<Status>('idle');
  const [savedJson, setSavedJson] = useState<string | null>(null);
  const readOnly = !selectedVehicle || selectedVehicle.isSystem;
  const motorName = selectedVehicle?.motor?.name ?? 'Motor';
  const current = buildUpdate(motorName);
  const currentJson = current ? JSON.stringify(current) : null;
  // "Saved" is only shown while the inputs still match what was stored.
  const showSaved = status === 'saved' && savedJson === currentJson;

  const onSave = async () => {
    if (!selectedVehicle || readOnly || !current) return;
    setStatus('saving');
    try {
      const updated = await repository.updateVehicle(selectedVehicle.id, current);
      setVehicle(updated);
      setSavedJson(currentJson);
      setStatus('saved');
    } catch {
      setStatus('error');
    }
  };

  return (
    <div className="flex flex-wrap items-center gap-3">
      <Button
        type="button"
        onClick={onSave}
        disabled={readOnly || disabled || !current || status === 'saving'}
        data-testid={testId}
      >
        {status === 'saving' ? (
          <Loader2 className="h-4 w-4 animate-spin" aria-hidden />
        ) : (
          <Save className="h-4 w-4" aria-hidden />
        )}
        {t.sim.save}
      </Button>
      <span role="status" aria-live="polite" className="text-[0.8125rem]">
        {showSaved && (
          <span className="text-muted-foreground flex items-center gap-1">
            <Check className="h-4 w-4" aria-hidden />
            {t.sim.saved}
          </span>
        )}
        {status === 'error' && <span className="text-destructive">{t.sim.errorGeneric}</span>}
      </span>
      {hint && <p className="text-muted-foreground w-full text-xs">{hint}</p>}
    </div>
  );
}
