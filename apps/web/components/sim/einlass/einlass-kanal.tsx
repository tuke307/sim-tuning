'use client';

import React from 'react';
import type { VehicleDto } from '@sim-tuning/common/schemas';
import { FIELD_BASE_UNITS } from '@sim-tuning/common/units';
import { SaveToVehicleButton, Section } from '@/components/sim/shared/module-page';
import { ResultValue, UnitNumberField } from '@/components/sim/shared/unit-number-field';
import { useWorkDraft } from '@/hooks/useWorkDraft';
import { workDraftKey } from '@/lib/sim/drafts/work-draft';
import {
  buildKanalUpdate,
  computeKanalResonanzLaenge,
  kanalDraftFromVehicle,
  type KanalDraft,
} from '@/lib/sim/einlass/einlass-helpers';
import { useLocale } from '@/providers/i18n/locale-context';

const PANEL = 'einlass.kanal';

/** v2 `EinlassKanalView`: intake resonance length. */
export function EinlassKanal({ vehicle }: { vehicle: VehicleDto }) {
  const { t } = useLocale();
  const k = t.einlass.kanal;
  const { draft, setDraft, dirty, reset, commit } = useWorkDraft<KanalDraft>(
    workDraftKey(vehicle.id, PANEL),
    () => kanalDraftFromVehicle(vehicle),
  );
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
          testId="einlass-kanal-flaecheA"
        />
        <UnitNumberField
          label={k.steuerzeitSZ}
          value={draft.steuerzeitSZ}
          suffix="°KW"
          onChange={set('steuerzeitSZ')}
          testId="einlass-kanal-steuerzeitSZ"
        />
        <UnitNumberField
          label={k.resonanzU}
          value={draft.resonanzU}
          suffix="U/min"
          onChange={set('resonanzU')}
          testId="einlass-kanal-resonanzU"
        />
        <UnitNumberField
          label={k.kurbelgehaeuseV}
          value={draft.kurbelgehaeuseV}
          baseUnit={FIELD_BASE_UNITS.motor.kurbelgehaeuseV}
          onChange={set('kurbelgehaeuseV')}
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
      <SaveToVehicleButton
        buildUpdate={(name) => buildKanalUpdate(name, draft)}
        onSaved={(saved) => commit(workDraftKey(saved.id, PANEL))}
        draft={{ dirty, onReset: reset }}
        hint={t.einlass.saveHint}
        testId="einlass-kanal-save"
      />
    </Section>
  );
}
