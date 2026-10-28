'use client';

import React, { useState } from 'react';
import type { VehicleDto } from '@sim-tuning/common/schemas';
import { FIELD_BASE_UNITS } from '@sim-tuning/common/units';
import { Section } from '@/components/sim/shared/module-page';
import { ResultValue, UnitNumberField } from '@/components/sim/shared/unit-number-field';
import { FactorSlider } from './factor-slider';
import { SaveMotorButton } from './save-motor-button';
import {
  buildTheorieUpdate,
  computeTheorie,
  theorieDraftFromVehicle,
  type TheorieDraft,
} from '@/lib/sim/auslass/auslass-helpers';
import { useLocale } from '@/providers/i18n/locale-context';
import { useSimData } from '@/providers/sim/sim-data-context';

/** v2 `AuslassTheorieView`: header length/diameter, gas velocity and resonance length. */
export function AuslassTheorie({ vehicle }: { vehicle: VehicleDto }) {
  const { t } = useLocale();
  const { rounding } = useSimData();
  const th = t.auslass.theorie;
  const readOnly = vehicle.isSystem;
  const [draft, setDraft] = useState<TheorieDraft>(() => theorieDraftFromVehicle(vehicle));
  const set = (key: keyof TheorieDraft) => (value: number | null) =>
    setDraft((d) => ({ ...d, [key]: value }));
  const result = computeTheorie(draft);
  const fmt = new Intl.NumberFormat(undefined, {
    maximumFractionDigits: rounding.roundingAccuracy,
  });

  return (
    <div className="space-y-5">
      <div className="grid gap-5 lg:grid-cols-2">
        <Section title={th.sectionKruemmerL}>
          <UnitNumberField
            label={th.kruemmerD}
            value={draft.kruemmerD}
            baseUnit={FIELD_BASE_UNITS.auspuff.kruemmerD}
            onChange={set('kruemmerD')}
            readOnly={readOnly}
            testId="auslass-theorie-kruemmerD"
          />
          <FactorSlider
            label={th.kruemmerF}
            value={draft.kruemmerF}
            onChange={set('kruemmerF')}
            readOnly={readOnly}
            testId="auslass-theorie-kruemmerF"
          />
          <ResultValue
            label={th.kruemmerL}
            value={result.kruemmerL}
            unit="mm"
            testId="auslass-theorie-kruemmerL"
          />
        </Section>

        <Section title={th.sectionKruemmerD}>
          <UnitNumberField
            label={th.flaecheA}
            value={draft.flaecheA}
            baseUnit={FIELD_BASE_UNITS.auslass.flaecheA}
            onChange={set('flaecheA')}
            readOnly={readOnly}
            testId="auslass-theorie-flaecheA"
          />
          <div className="bg-secondary/50 rounded-2xl px-4 py-3">
            <p className="text-muted-foreground text-xs">{th.kruemmerDRange}</p>
            <p
              className="font-rounded text-lg font-semibold tabular-nums"
              data-testid="auslass-theorie-kruemmerDRange"
            >
              {result.kruemmerDRange
                ? `${fmt.format(result.kruemmerDRange[0])} – ${fmt.format(result.kruemmerDRange[1])}`
                : '–'}
              {result.kruemmerDRange && (
                <span className="text-muted-foreground ml-1 text-sm">mm</span>
              )}
            </p>
          </div>
        </Section>

        <Section title={th.sectionGas}>
          <UnitNumberField
            label={th.gasT}
            value={draft.gasT}
            baseUnit={FIELD_BASE_UNITS.auspuff.abgasT}
            onChange={set('gasT')}
            readOnly={readOnly}
            testId="auslass-theorie-gasT"
          />
          <ResultValue
            label={th.gasV}
            value={result.gasV}
            unit="m/s"
            testId="auslass-theorie-gasV"
          />
        </Section>

        <Section title={th.sectionResonanz}>
          <div className="grid gap-4 sm:grid-cols-2">
            <UnitNumberField
              label={th.steuerzeitSZ}
              value={draft.steuerzeitSZ}
              suffix="°KW"
              onChange={set('steuerzeitSZ')}
              readOnly={readOnly}
              testId="auslass-theorie-steuerzeitSZ"
            />
            <UnitNumberField
              label={th.abgasT}
              value={draft.abgasT}
              baseUnit={FIELD_BASE_UNITS.auspuff.abgasT}
              onChange={set('abgasT')}
              readOnly={readOnly}
              testId="auslass-theorie-abgasT"
            />
            <UnitNumberField
              label={th.resonanzU}
              value={draft.resonanzU}
              suffix="U/min"
              onChange={set('resonanzU')}
              readOnly={readOnly}
              testId="auslass-theorie-resonanzU"
            />
          </div>
          <ResultValue
            label={th.resonanzL}
            value={result.resonanzL}
            unit="mm"
            testId="auslass-theorie-resonanzL"
          />
        </Section>
      </div>
      <SaveMotorButton
        buildUpdate={(name) => buildTheorieUpdate(name, draft, result)}
        hint={th.saveHint}
        testId="auslass-theorie-save"
      />
    </div>
  );
}
