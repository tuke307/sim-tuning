'use client';

import React, { useId, useState } from 'react';
import type { VehicleDto } from '@sim-tuning/common/schemas';
import { FIELD_BASE_UNITS, UNITS } from '@sim-tuning/common/units';
import { Label } from '@sim-tuning/ui/components/ui/label';
import { NativeSelect } from '@sim-tuning/ui/components/ui/native-select';
import { Section } from '@/components/sim/shared/module-page';
import { ResultValue, UnitNumberField } from '@/components/sim/shared/unit-number-field';
import { computeVerdichtung, ZIELVERDICHTUNGEN } from '@/lib/sim/motor/calculations';
import { useLocale } from '@/providers/i18n/locale-context';
import { ApplyToVehicle } from './apply-to-vehicle';

/** v2 MotorVerdichtungView: current compression and how much to machine off for a target. */
export function VerdichtungPanel({ vehicle }: { vehicle: VehicleDto }) {
  const { t } = useLocale();
  const targetId = useId();
  const [hubraumV, setHubraumV] = useState<number | null>(vehicle.motor?.hubraumV ?? null);
  const [brennraumV, setBrennraumV] = useState<number | null>(vehicle.motor?.brennraumV ?? null);
  const [bohrungD, setBohrungD] = useState<number | null>(vehicle.motor?.bohrungD ?? null);
  const [ziel, setZiel] = useState<number | null>(null);
  const { current, abdrehen } = computeVerdichtung({
    hubraumV,
    brennraumV,
    bohrungD,
    zielVerdichtung: ziel,
  });

  return (
    <div className="grid items-start gap-4 lg:grid-cols-2">
      <Section title={t.motor.verdichtung.inputsTitle}>
        <div className="grid gap-4 sm:grid-cols-2">
          <UnitNumberField
            label={t.motor.verdichtung.hubraum}
            value={hubraumV}
            baseUnit={FIELD_BASE_UNITS.motor.hubraumV}
            onChange={setHubraumV}
            testId="motor-verdichtung-hubraum"
          />
          <UnitNumberField
            label={t.motor.verdichtung.brennraum}
            value={brennraumV}
            baseUnit={FIELD_BASE_UNITS.motor.brennraumV}
            onChange={setBrennraumV}
            testId="motor-verdichtung-brennraum"
          />
          <UnitNumberField
            label={t.motor.verdichtung.bohrung}
            value={bohrungD}
            baseUnit={FIELD_BASE_UNITS.motor.bohrungD}
            onChange={setBohrungD}
            testId="motor-verdichtung-bohrung"
          />
        </div>
      </Section>
      <div className="space-y-4">
        <Section title={t.motor.verdichtung.currentTitle}>
          <ResultValue
            label={t.motor.verdichtung.current}
            value={current}
            unit=": 1"
            testId="motor-verdichtung-current"
          />
          <ApplyToVehicle
            testId="motor-verdichtung-apply"
            patch={current !== null ? { hubraumV, brennraumV, verdichtungV: current } : null}
          />
        </Section>
        <Section title={t.motor.verdichtung.targetTitle}>
          <div className="space-y-1.5">
            <Label htmlFor={targetId} className="text-[0.8125rem] font-medium">
              {t.motor.verdichtung.target}
            </Label>
            <NativeSelect
              id={targetId}
              value={ziel === null ? '' : String(ziel)}
              onChange={(e) => setZiel(e.target.value === '' ? null : Number(e.target.value))}
              data-testid="motor-verdichtung-ziel"
            >
              <option value="">{t.motor.verdichtung.targetPlaceholder}</option>
              {ZIELVERDICHTUNGEN.map((value) => (
                <option key={value} value={value}>
                  {value} : 1
                </option>
              ))}
            </NativeSelect>
          </div>
          <ResultValue
            label={t.motor.verdichtung.abdrehen}
            value={abdrehen}
            unit={UNITS.millimeter.abbreviation}
            testId="motor-verdichtung-abdrehen"
          />
          <p className="text-muted-foreground text-xs">{t.motor.verdichtung.abdrehenHint}</p>
        </Section>
      </div>
    </div>
  );
}
