'use client';

import React from 'react';
import type { VehicleDto } from '@sim-tuning/common/schemas';
import { FIELD_BASE_UNITS, UNITS } from '@sim-tuning/common/units';
import { Section } from '@/components/sim/shared/module-page';
import { useWorkDraft } from '@/hooks/useWorkDraft';
import { workDraftKey } from '@/lib/sim/drafts/work-draft';
import { ResultValue, UnitNumberField } from '@/components/sim/shared/unit-number-field';
import { computeHubraum, DEFAULT_EINBAUSPIEL_MM } from '@/lib/sim/motor/calculations';
import { interpolate } from '@/lib/sim/vehicles/draft';
import { useLocale } from '@/providers/i18n/locale-context';
import { ApplyToVehicle } from './apply-to-vehicle';

const MM = UNITS.millimeter.abbreviation;
const PANEL = 'motor.hubraum';

interface HubraumDraft {
  hubL: number | null;
  hubraumV: number | null;
  einbauspiel: number | null;
}

/** Fields stored on the vehicle; the piston clearance is a scratch value. */
const VEHICLE_FIELDS = ['hubL', 'hubraumV'] as const;

/** v2 MotorHubraumView: bore and piston diameter from stroke + displacement, oversize bores. */
export function HubraumPanel({ vehicle }: { vehicle: VehicleDto }) {
  const { t } = useLocale();
  const { draft, setDraft, dirty, reset, commit } = useWorkDraft<HubraumDraft>(
    workDraftKey(vehicle.id, PANEL),
    () => ({
      hubL: vehicle.motor?.hubL ?? null,
      hubraumV: vehicle.motor?.hubraumV ?? null,
      einbauspiel: DEFAULT_EINBAUSPIEL_MM,
    }),
    { compare: VEHICLE_FIELDS },
  );
  const { hubL, hubraumV, einbauspiel } = draft;
  const set = (key: keyof HubraumDraft) => (value: number | null) =>
    setDraft((d) => ({ ...d, [key]: value }));
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
            onChange={set('hubL')}
            testId="motor-hubraum-hub"
          />
          <UnitNumberField
            label={t.motor.hubraum.hubraum}
            value={hubraumV}
            baseUnit={FIELD_BASE_UNITS.motor.hubraumV}
            onChange={set('hubraumV')}
            testId="motor-hubraum-hubraum"
          />
          <UnitNumberField
            label={t.motor.hubraum.einbauspiel}
            value={einbauspiel}
            baseUnit="millimeter"
            onChange={set('einbauspiel')}
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
          onSaved={(saved) => commit(workDraftKey(saved.id, PANEL))}
          draft={{ dirty, onReset: reset }}
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
