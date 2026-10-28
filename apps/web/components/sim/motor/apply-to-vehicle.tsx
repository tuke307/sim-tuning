'use client';

import React, { useState } from 'react';
import { Check, Save } from 'lucide-react';
import type { MotorInputDto } from '@sim-tuning/common/schemas';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { useLocale } from '@/providers/i18n/locale-context';
import { useSimData } from '@/providers/sim/sim-data-context';

export type MotorPatch = Partial<Omit<MotorInputDto, 'name'>>;

/**
 * "Apply to vehicle" action shared by the motor calculators: writes the given engine fields back
 * to the selected vehicle. Starter vehicles are read-only, so the button is disabled for them.
 */
export function ApplyToVehicle({
  patch,
  testId,
}: {
  /** Null while there is nothing (valid) to apply. */
  patch: MotorPatch | null;
  testId: string;
}) {
  const { t } = useLocale();
  const { selectedVehicle, repository, setVehicle } = useSimData();
  const [saving, setSaving] = useState(false);
  const [failed, setFailed] = useState(false);
  const [savedKey, setSavedKey] = useState<string | null>(null);
  const readOnly = !selectedVehicle || selectedVehicle.isSystem;
  const patchKey = patch ? JSON.stringify(patch) : null;
  const saved = patchKey !== null && patchKey === savedKey;

  const apply = async () => {
    if (!selectedVehicle || readOnly || !patch) return;
    setSaving(true);
    setFailed(false);
    try {
      const updated = await repository.updateVehicle(selectedVehicle.id, {
        motor: { name: selectedVehicle.motor?.name ?? selectedVehicle.name, ...patch },
      });
      setVehicle(updated);
      setSavedKey(patchKey);
    } catch {
      setFailed(true);
    } finally {
      setSaving(false);
    }
  };

  const message = failed
    ? t.sim.errorGeneric
    : saved
      ? t.motor.applied
      : readOnly
        ? t.motor.applyReadOnlyHint
        : '';

  return (
    <div className="flex flex-wrap items-center gap-3 pt-1">
      <Button
        type="button"
        size="sm"
        variant="accent"
        disabled={readOnly || !patch || saving}
        onClick={() => void apply()}
        data-testid={testId}
      >
        {saved ? <Check className="h-4 w-4" /> : <Save className="h-4 w-4" />}
        {t.motor.applyToVehicle}
      </Button>
      <p
        role="status"
        aria-live="polite"
        className={
          failed ? 'text-destructive min-h-5 text-xs' : 'text-muted-foreground min-h-5 text-xs'
        }
      >
        {message}
      </p>
    </div>
  );
}
