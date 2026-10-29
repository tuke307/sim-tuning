'use client';

import React from 'react';
import type { VehicleDto } from '@sim-tuning/common/schemas';
import { FIELD_BASE_UNITS } from '@sim-tuning/common/units';
import { SaveToVehicleButton, Section } from '@/components/sim/shared/module-page';
import { ResultValue, UnitNumberField } from '@/components/sim/shared/unit-number-field';
import { useWorkDraft } from '@/hooks/useWorkDraft';
import { workDraftKey } from '@/lib/sim/drafts/work-draft';
import { FactorSlider } from './factor-slider';
import {
  buildTheorieUpdate,
  computeTheorie,
  theorieDraftFromVehicle,
  type TheorieDraft,
} from '@/lib/sim/auslass/auslass-helpers';
import { useLocale } from '@/providers/i18n/locale-context';
import { useSimData } from '@/providers/sim/sim-data-context';

const PANEL = 'auslass.theorie';

/** Fields stored on the vehicle; the gas temperature of the velocity calculator is scratch. */
const VEHICLE_FIELDS = [
  'kruemmerD',
  'kruemmerF',
  'flaecheA',
  'steuerzeitSZ',
  'abgasT',
  'resonanzU',
] as const;

/** v2 `AuslassTheorieView`: header length/diameter, gas velocity and resonance length. */
export function AuslassTheorie({ vehicle }: { vehicle: VehicleDto }) {
  const { t } = useLocale();
  const { rounding } = useSimData();
  const th = t.auslass.theorie;
  const { draft, setDraft, dirty, reset, commit } = useWorkDraft<TheorieDraft>(
    workDraftKey(vehicle.id, PANEL),
    () => theorieDraftFromVehicle(vehicle),
    { compare: VEHICLE_FIELDS },
  );
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
            testId="auslass-theorie-kruemmerD"
          />
          <FactorSlider
            label={th.kruemmerF}
            value={draft.kruemmerF}
            onChange={set('kruemmerF')}
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
            testId="auslass-theorie-flaecheA"
          />
          <div className="bg-card rounded-md border px-3 py-2.5">
            <p className="text-muted-foreground text-[0.75rem]">{th.kruemmerDRange}</p>
            <p
              className="font-data text-[1.25rem] leading-tight"
              data-testid="auslass-theorie-kruemmerDRange"
            >
              {result.kruemmerDRange
                ? `${fmt.format(result.kruemmerDRange[0])} – ${fmt.format(result.kruemmerDRange[1])}`
                : '–'}
              {result.kruemmerDRange && (
                <span className="unit-caps text-muted-foreground ml-1.5 text-[0.875rem]">mm</span>
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
              testId="auslass-theorie-steuerzeitSZ"
            />
            <UnitNumberField
              label={th.abgasT}
              value={draft.abgasT}
              baseUnit={FIELD_BASE_UNITS.auspuff.abgasT}
              onChange={set('abgasT')}
              testId="auslass-theorie-abgasT"
            />
            <UnitNumberField
              label={th.resonanzU}
              value={draft.resonanzU}
              suffix="U/min"
              onChange={set('resonanzU')}
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
      <SaveToVehicleButton
        buildUpdate={(name) => buildTheorieUpdate(name, draft, result)}
        onSaved={(saved) => commit(workDraftKey(saved.id, PANEL))}
        draft={{ dirty, onReset: reset }}
        hint={th.saveHint}
        testId="auslass-theorie-save"
      />
    </div>
  );
}
