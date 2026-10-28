'use client';

import React, { useState } from 'react';
import { Input } from '@sim-tuning/ui/components/ui/input';
import { cn } from '@sim-tuning/ui/lib/utils';

interface BaseNumberInputProps extends Omit<
  React.ComponentProps<'input'>,
  'type' | 'value' | 'onChange' | 'min' | 'max' | 'step'
> {
  min?: number;
  max?: number;
  step?: number | string;
  variant?: 'input' | 'bare';
}

interface RequiredNumberInputProps extends BaseNumberInputProps {
  allowNull?: false;
  value: number;
  fallbackValue?: number;
  onValueChange: (val: number) => void;
}

interface NullableNumberInputProps extends BaseNumberInputProps {
  allowNull: true;
  value: number | null | undefined;
  fallbackValue?: number | null;
  onValueChange: (val: number | null) => void;
}

export type NumberInputProps = RequiredNumberInputProps | NullableNumberInputProps;

function clampNumber(val: number, min?: number, max?: number): number {
  let res = val;
  if (min !== undefined && res < min) res = min;
  if (max !== undefined && res > max) res = max;
  return res;
}

export function NumberInput(props: NumberInputProps) {
  const {
    value,
    min,
    max,
    step,
    variant = 'input',
    allowNull,
    fallbackValue,
    onValueChange,
    onFocus,
    onBlur,
    ...rest
  } = props;

  const formatExternal = (v: number | null | undefined): string =>
    v === null || v === undefined || Number.isNaN(v) ? '' : String(v);

  const [isFocused, setIsFocused] = useState(false);
  const [draft, setDraft] = useState<string>(() => formatExternal(value));

  const isDecimal = step !== undefined && (String(step).includes('.') || String(step) === 'any');

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const raw = e.target.value.replace(',', '.');
    setDraft(e.target.value);

    if (raw.trim() === '' || raw === '-') {
      if (allowNull) {
        onValueChange(null);
      } else {
        const fb = clampNumber(
          typeof fallbackValue === 'number' ? fallbackValue : (min ?? 0),
          min,
          max,
        );
        onValueChange(fb);
      }
      return;
    }

    const parsed = Number(raw);
    if (!Number.isNaN(parsed)) {
      if (allowNull) {
        onValueChange(parsed);
      } else {
        onValueChange(parsed);
      }
    }
  };

  const handleFocus = (e: React.FocusEvent<HTMLInputElement>) => {
    setIsFocused(true);
    setDraft(formatExternal(value));
    const target = e.currentTarget;
    window.requestAnimationFrame(() => {
      try {
        target.select();
      } catch {
        // Ignore if unsupported by input type
      }
    });
    onFocus?.(e);
  };

  const handleBlur = (e: React.FocusEvent<HTMLInputElement>) => {
    setIsFocused(false);
    const normalized = draft.trim().replace(',', '.');

    if (normalized === '' || Number.isNaN(Number(normalized))) {
      if (allowNull) {
        const fb = typeof fallbackValue === 'number' ? clampNumber(fallbackValue, min, max) : null;
        onValueChange(fb);
        setDraft(formatExternal(fb));
      } else {
        const fb = clampNumber(
          typeof fallbackValue === 'number' ? fallbackValue : (min ?? 0),
          min,
          max,
        );
        onValueChange(fb);
        setDraft(String(fb));
      }
    } else {
      const clamped = clampNumber(Number(normalized), min, max);
      if (allowNull) {
        onValueChange(clamped);
      } else {
        onValueChange(clamped);
      }
      setDraft(String(clamped));
    }

    onBlur?.(e);
  };

  const sharedProps = {
    ...rest,
    type: 'number' as const,
    inputMode: (isDecimal ? 'decimal' : 'numeric') as 'decimal' | 'numeric',
    min,
    max,
    step,
    value: isFocused ? draft : formatExternal(value),
    onChange: handleChange,
    onFocus: handleFocus,
    onBlur: handleBlur,
  };

  if (variant === 'bare') {
    return <input {...sharedProps} className={cn(sharedProps.className, 'max-sm:text-base')} />;
  }

  return <Input {...sharedProps} />;
}
