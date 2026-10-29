'use client';

import React, { useId } from 'react';
import { Label } from '@sim-tuning/ui/components/ui/label';
import { useLocale } from '@/providers/i18n/locale-context';
import { KRUEMMER_FAKTOR_MAX, KRUEMMER_FAKTOR_MIN } from '@/lib/sim/auslass/auslass-helpers';

/**
 * v2 torque factor slider (`spitz` … `flach`, 6 … 12) as an accessible native range input.
 * Local variant: the shared field kit has no slider field.
 */
export function FactorSlider({
  label,
  value,
  onChange,
  readOnly,
  testId,
  min = KRUEMMER_FAKTOR_MIN,
  max = KRUEMMER_FAKTOR_MAX,
  step = 0.1,
}: {
  label: string;
  value: number | null | undefined;
  onChange: (value: number) => void;
  readOnly?: boolean;
  testId?: string;
  min?: number;
  max?: number;
  step?: number;
}) {
  const id = useId();
  const { t } = useLocale();
  const hasValue = typeof value === 'number' && Number.isFinite(value);
  const position = hasValue ? Math.min(max, Math.max(min, value)) : min;
  return (
    <div className="space-y-1.5">
      <div className="flex items-baseline justify-between gap-2">
        <Label htmlFor={id} className="text-[0.8125rem] font-medium">
          {label}
        </Label>
        <output
          htmlFor={id}
          className="font-data text-sm"
          data-testid={testId ? `${testId}-value` : undefined}
        >
          {hasValue ? value.toFixed(1) : '–'}
        </output>
      </div>
      <input
        id={id}
        type="range"
        min={min}
        max={max}
        step={step}
        value={position}
        disabled={readOnly}
        data-testid={testId}
        aria-valuetext={hasValue ? value.toFixed(1) : undefined}
        onChange={(e) => onChange(Number(e.target.value))}
        className="accent-primary h-11 w-full cursor-pointer disabled:cursor-not-allowed disabled:opacity-50"
      />
      <div className="text-muted-foreground flex justify-between text-xs" aria-hidden>
        <span>
          {t.auslass.factor.peaky} ({min})
        </span>
        <span>
          {t.auslass.factor.flat} ({max})
        </span>
      </div>
    </div>
  );
}
