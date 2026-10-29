'use client';

import React, { useState } from 'react';
import type { VehicleDto } from '@sim-tuning/common/schemas';
import { FIELD_BASE_UNITS, UNITS } from '@sim-tuning/common/units';
import { Section } from '@/components/sim/shared/module-page';
import { ResultValue, UnitNumberField } from '@/components/sim/shared/unit-number-field';
import { computeHubraum, DEFAULT_EINBAUSPIEL_MM } from '@/lib/sim/motor/calculations';
import { interpolate } from '@/lib/sim/vehicles/draft';
import { useLocale } from '@/providers/i18n/locale-context';
import { ApplyToVehicle } from './apply-to-vehicle';

const MM = UNITS.millimeter.abbreviation;

/** v2 MotorHubraumView: bore and piston diameter from stroke + displacement, oversize bores. */
export function HubraumPanel({ vehicle }: { vehicle: VehicleDto }) {
  const { t } = useLocale();
  const [hubL, setHubL] = useState<number | null>(vehicle.motor?.hubL ?? null);
  const [hubraumV, setHubraumV] = useState<number | null>(vehicle.motor?.hubraumV ?? null);
  const [einbauspiel, setEinbauspiel] = useState<number | null>(DEFAULT_EINBAUSPIEL_MM);
  const result = computeHubraum({ hubL, hubraumV, einbauspielMm: einbauspiel });
  const grinding = result
    ? [
        result.grinding.diameter1,
        result.grinding.diameter2,
        result.grinding.diameter3,
        result.grinding.diameter4,
      ]
    : [null, null, null, null];

  return (
    <div className="grid items-start gap-4 lg:grid-cols-2">
      <Section title={t.motor.hubraum.inputsTitle}>
        <div className="grid gap-4 sm:grid-cols-2">
          <UnitNumberField
            label={t.motor.hubraum.hub}
            value={hubL}
            baseUnit={FIELD_BASE_UNITS.motor.hubL}
            onChange={setHubL}
            testId="motor-hubraum-hub"
          />
          <UnitNumberField
            label={t.motor.hubraum.hubraum}
            value={hubraumV}
            baseUnit={FIELD_BASE_UNITS.motor.hubraumV}
            onChange={setHubraumV}
            testId="motor-hubraum-hubraum"
          />
          <UnitNumberField
            label={t.motor.hubraum.einbauspiel}
            value={einbauspiel}
            baseUnit="millimeter"
            onChange={setEinbauspiel}
            hint={t.motor.hubraum.einbauspielHint}
            testId="motor-hubraum-einbauspiel"
          />
        </div>
      </Section>
      <Section title={t.motor.hubraum.resultsTitle}>
        <div className="grid gap-3 sm:grid-cols-2">
          <ResultValue
            label={t.motor.hubraum.bohrung}
            value={result?.bohrungD}
            unit={MM}
            testId="motor-hubraum-bohrung"
          />
          <ResultValue
            label={t.motor.hubraum.kolben}
            value={result?.kolbenD}
            unit={MM}
            testId="motor-hubraum-kolben"
          />
        </div>
        <p className="text-muted-foreground min-h-4 text-xs">
          {result ? '' : t.motor.missingInputs}
        </p>
        <ApplyToVehicle
          testId="motor-hubraum-apply"
          patch={result ? { hubL, hubraumV, bohrungD: result.bohrungD } : null}
        />
      </Section>
      <div className="lg:col-span-2">
        <Section title={t.motor.hubraum.grindingTitle}>
          <p className="text-muted-foreground text-xs">{t.motor.hubraum.grindingHint}</p>
          <div className="grid grid-cols-2 gap-3 md:grid-cols-4">
            {grinding.map((value, index) => (
              <ResultValue
                key={index}
                label={interpolate(t.motor.hubraum.grindingStep, { n: index + 1 })}
                value={value}
                unit={MM}
                testId={`motor-hubraum-grinding-${index + 1}`}
              />
            ))}
          </div>
        </Section>
      </div>
    </div>
  );
}
