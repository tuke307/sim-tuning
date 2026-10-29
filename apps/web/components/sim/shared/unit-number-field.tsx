'use client';

import React, { useId } from 'react';
import {
  convertUnit,
  convertValueForUnit,
  QUANTITY_UNITS,
  UNITS,
  type UnitId,
} from '@sim-tuning/common/units';
import { roundTo } from '@sim-tuning/common/utils/client';
import { Label } from '@sim-tuning/ui/components/ui/label';
import { NativeSelect } from '@sim-tuning/ui/components/ui/native-select';
import { cn } from '@sim-tuning/ui/lib/utils';
import { NumberInput } from '@/components/sim/number-input';
import { useUnitPreference } from '@/hooks/useUnitPreference';
import { useSimData } from '@/providers/sim/sim-data-context';

export interface UnitNumberFieldProps {
  label: string;
  /** Value in `baseUnit` (as stored). */
  value: number | null | undefined;
  /** Storage unit of the field (`FIELD_BASE_UNITS`). Omit for unit-less values (°KW, U/min, …). */
  baseUnit?: UnitId;
  /** Fixed suffix for unit-less values, e.g. `°KW`, `U/min`, `%`. */
  suffix?: string;
  onChange?: (baseValue: number | null) => void;
  readOnly?: boolean;
  hint?: string;
  testId?: string;
  className?: string;
}

/**
 * Number input that stores values in the field's base unit but lets the user pick the displayed
 * unit (v2 unit pickers). Unit switches round according to the user's preferences.
 */
export function UnitNumberField({
  label,
  value,
  baseUnit,
  suffix,
  onChange,
  readOnly,
  hint,
  testId,
  className,
}: UnitNumberFieldProps) {
  const id = useId();
  const { rounding } = useSimData();
  const [unit, setUnit] = useUnitPreference(baseUnit);
  const units = baseUnit ? QUANTITY_UNITS[UNITS[baseUnit].quantity] : [];

  const displayed =
    baseUnit && unit && value !== null && value !== undefined
      ? (convertValueForUnit(value, baseUnit, unit, rounding) ?? null)
      : (value ?? null);

  return (
    <div className={cn('space-y-1.5', className)}>
      <Label htmlFor={id} className="text-[0.8125rem] font-medium">
        {label}
      </Label>
      <div className="flex items-center gap-2">
        <NumberInput
          id={id}
          data-testid={testId}
          allowNull
          value={displayed === null ? null : roundTo(displayed, 10)}
          readOnly={readOnly}
          onValueChange={(next) => {
            if (!onChange) return;
            onChange(
              next === null || !baseUnit || !unit ? next : convertUnit(next, unit, baseUnit),
            );
          }}
          className="font-data"
        />
        {baseUnit && units.length > 1 ? (
          <div className="shrink-0">
            <NativeSelect
              aria-label={`${label} unit`}
              value={unit}
              onChange={(e) => setUnit(e.target.value as UnitId)}
              className="font-data w-auto"
            >
              {units.map((u) => (
                <option key={u} value={u}>
                  {UNITS[u].abbreviation}
                </option>
              ))}
            </NativeSelect>
          </div>
        ) : (
          (suffix ?? (baseUnit ? UNITS[baseUnit].abbreviation : null)) && (
            <span className="unit-caps text-muted-foreground font-data min-w-8 shrink-0 text-[0.8125rem]">
              {suffix ?? (baseUnit ? UNITS[baseUnit].abbreviation : '')}
            </span>
          )
        )}
      </div>
      {hint && <p className="text-muted-foreground text-xs">{hint}</p>}
    </div>
  );
}

/** Read-only computed value with label and unit, formatted with the user's rounding. */
export function ResultValue({
  label,
  value,
  unit,
  testId,
}: {
  label: string;
  value: number | null | undefined;
  unit?: string;
  testId?: string;
}) {
  const { rounding } = useSimData();
  const { format } = new Intl.NumberFormat(undefined, {
    maximumFractionDigits: rounding.roundingAccuracy,
  });
  const ok = value !== null && value !== undefined && Number.isFinite(value);
  return (
    <div className="bg-card min-w-0 rounded-md border px-3 py-2.5">
      <p className="text-muted-foreground text-[0.75rem] leading-snug">{label}</p>
      <p data-testid={testId} className="font-data mt-1 text-[1.25rem] leading-tight">
        {ok ? format(value) : '–'}
        {ok && unit ? (
          <span className="unit-caps text-muted-foreground ml-1.5 text-[0.875rem]">{unit}</span>
        ) : null}
      </p>
    </div>
  );
}
