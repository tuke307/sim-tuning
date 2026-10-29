'use client';

import React from 'react';
import type { MotorInputDto, VehicleDto } from '@sim-tuning/common/schemas';
import { SaveToVehicleButton } from '@/components/sim/shared/module-page';

export type MotorPatch = Partial<Omit<MotorInputDto, 'name'>>;

/**
 * "Save to vehicle" action shared by the motor calculators: writes the given engine fields back
 * to the selected vehicle (or into a new copy of a starter vehicle).
 */
export function ApplyToVehicle({
  patch,
  testId,
  onSaved,
  draft,
}: {
  /** Null while there is nothing (valid) to apply. */
  patch: MotorPatch | null;
  testId: string;
  onSaved?: (saved: VehicleDto) => (() => void) | void;
  draft?: { dirty: boolean; onReset: () => void };
}) {
  return (
    <SaveToVehicleButton
      size="sm"
      variant="accent"
      testId={testId}
      buildUpdate={(name) => (patch ? { motor: { name, ...patch } } : null)}
      onSaved={onSaved}
      draft={draft}
    />
  );
}
