'use client';

import React, { useState } from 'react';
import type { VehicleDto } from '@sim-tuning/common/schemas';
import { FIELD_BASE_UNITS } from '@sim-tuning/common/units';
import { Section } from '@/components/sim/shared/module-page';
import { ResultValue, UnitNumberField } from '@/components/sim/shared/unit-number-field';
import { SaveMotorButton } from './save-motor-button';
import {
  buildKanalUpdate,
  computeKanalResonanzLaenge,
  kanalDraftFromVehicle,
  type KanalDraft,
} from '@/lib/sim/einlass/einlass-helpers';
import { useLocale } from '@/providers/i18n/locale-context';

/** v2 `EinlassKanalView`: intake resonance length. */
export function EinlassKanal({ vehicle }: { vehicle: VehicleDto }) {
  const { t } = useLocale();
  const k = t.einlass.kanal;
  const readOnly = vehicle.isSystem;
  const [draft, setDraft] = useState<KanalDraft>(() => kanalDraftFromVehicle(vehicle));
  const set = (key: keyof KanalDraft) => (value: number | null) =>
    setDraft((d) => ({ ...d, [key]: value }));
  const resonanzL = computeKanalResonanzLaenge(draft);

  return (
    <Section title={k.section}>
      <div className="grid gap-4 sm:grid-cols-2">
        <UnitNumberField
          label={k.flaecheA}
          value={draft.flaecheA}
          baseUnit={FIELD_BASE_UNITS.einlass.flaecheA}
          onChange={set('flaecheA')}
          readOnly={readOnly}
          testId="einlass-kanal-flaecheA"
        />
        <UnitNumberField
          label={k.steuerzeitSZ}
          value={draft.steuerzeitSZ}
          suffix="°KW"
          onChange={set('steuerzeitSZ')}
          readOnly={readOnly}
          testId="einlass-kanal-steuerzeitSZ"
        />
        <UnitNumberField
          label={k.resonanzU}
          value={draft.resonanzU}
          suffix="U/min"
          onChange={set('resonanzU')}
          readOnly={readOnly}
          testId="einlass-kanal-resonanzU"
        />
        <UnitNumberField
          label={k.kurbelgehaeuseV}
          value={draft.kurbelgehaeuseV}
          baseUnit={FIELD_BASE_UNITS.motor.kurbelgehaeuseV}
          onChange={set('kurbelgehaeuseV')}
          readOnly={readOnly}
          testId="einlass-kanal-kurbelgehaeuseV"
        />
      </div>
      <ResultValue
        label={k.resonanzL}
        value={resonanzL}
        unit="mm"
        testId="einlass-kanal-resonanzL"
      />
      {resonanzL === null && <p className="text-muted-foreground text-xs">{k.hint}</p>}
      <SaveMotorButton
        buildUpdate={(name) => buildKanalUpdate(name, draft)}
        hint={t.einlass.saveHint}
        testId="einlass-kanal-save"
      />
    </Section>
  );
}
