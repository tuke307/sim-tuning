'use client';

import React, { useState } from 'react';
import type { VehicleDto } from '@sim-tuning/common/schemas';
import { FIELD_BASE_UNITS } from '@sim-tuning/common/units';
import { Section } from '@/components/sim/shared/module-page';
import { ResultValue, UnitNumberField } from '@/components/sim/shared/unit-number-field';
import { SaveMotorButton } from './save-motor-button';
import {
  buildVergaserUpdate,
  computeVergaser,
  HAUPTDUESE_WIDERSTAND_DEFAULT,
  VERGASER_WIDERSTAND_DEFAULT,
  vergaserDraftFromVehicle,
  type VergaserDraft,
} from '@/lib/sim/einlass/einlass-helpers';
import { useLocale } from '@/providers/i18n/locale-context';

/** v2 `EinlassVergaserView`: carburettor bore and main jet. */
export function EinlassVergaser({ vehicle }: { vehicle: VehicleDto }) {
  const { t } = useLocale();
  const v = t.einlass.vergaser;
  const readOnly = vehicle.isSystem;
  const [draft, setDraft] = useState<VergaserDraft>(() => vergaserDraftFromVehicle(vehicle));
  const set = (key: keyof VergaserDraft) => (value: number | null) =>
    setDraft((d) => ({ ...d, [key]: value }));
  const result = computeVergaser(draft);
  const faktorHint = (value: number) => v.faktorHint.replace('{{value}}', String(value));

  return (
    <div className="grid gap-5 lg:grid-cols-2">
      <Section title={v.sectionDurchmesser}>
        <div className="grid gap-4 sm:grid-cols-2">
          <UnitNumberField
            label={v.hubraumV}
            value={draft.hubraumV}
            baseUnit={FIELD_BASE_UNITS.motor.hubraumV}
            onChange={set('hubraumV')}
            readOnly={readOnly}
            testId="einlass-vergaser-hubraumV"
          />
          <UnitNumberField
            label={v.resonanzU}
            value={draft.resonanzU}
            suffix="U/min"
            onChange={set('resonanzU')}
            readOnly={readOnly}
            testId="einlass-vergaser-resonanzU"
          />
          <UnitNumberField
            label={v.vergaserFaktor}
            value={draft.vergaserFaktor}
            onChange={set('vergaserFaktor')}
            readOnly={readOnly}
            hint={faktorHint(VERGASER_WIDERSTAND_DEFAULT)}
            testId="einlass-vergaser-faktor"
          />
        </div>
        <ResultValue
          label={v.durchmesserD}
          value={result.durchmesserD}
          unit="mm"
          testId="einlass-vergaser-durchmesserD"
        />
        {result.durchmesserD === null && <p className="text-muted-foreground text-xs">{v.hint}</p>}
      </Section>
      <Section title={v.sectionHauptduese}>
        <UnitNumberField
          label={v.hauptdueseFaktor}
          value={draft.hauptdueseFaktor}
          onChange={set('hauptdueseFaktor')}
          readOnly={readOnly}
          hint={faktorHint(HAUPTDUESE_WIDERSTAND_DEFAULT)}
          testId="einlass-vergaser-hauptduese-faktor"
        />
        <div className="grid gap-3 sm:grid-cols-2">
          <ResultValue label={v.durchmesserD} value={result.durchmesserD} unit="mm" />
          {/* v2 labels the main jet result in µm (`UnitHauptdueseD = Micrometer`). */}
          <ResultValue
            label={v.hauptdueseD}
            value={result.hauptdueseD}
            unit="µm"
            testId="einlass-vergaser-hauptdueseD"
          />
        </div>
      </Section>
      <div className="lg:col-span-2">
        <SaveMotorButton
          buildUpdate={(name) => buildVergaserUpdate(name, draft, result)}
          hint={t.einlass.saveHint}
          testId="einlass-vergaser-save"
        />
      </div>
    </div>
  );
}
