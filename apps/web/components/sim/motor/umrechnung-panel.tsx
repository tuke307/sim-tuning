'use client';

import React, { useId } from 'react';
import type { SteuerdiagrammPort } from '@sim-tuning/common/logic';
import type { VehicleDto } from '@sim-tuning/common/schemas';
import { FIELD_BASE_UNITS, UNITS } from '@sim-tuning/common/units';
import { Label } from '@sim-tuning/ui/components/ui/label';
import { NativeSelect } from '@sim-tuning/ui/components/ui/native-select';
import { Tabs, TabsList, TabsTrigger } from '@sim-tuning/ui/components/ui/tabs';
import { DraftNotice, Section } from '@/components/sim/shared/module-page';
import { useWorkDraft } from '@/hooks/useWorkDraft';
import { workDraftKey } from '@/lib/sim/drafts/work-draft';
import { ResultValue, UnitNumberField } from '@/components/sim/shared/unit-number-field';
import {
  computeDistanceToOT,
  computeHubradius,
  computePortTimingChange,
  STEUERDIAGRAMM_PORTS,
  steuerkanteForPort,
  type Steuerkante,
} from '@/lib/sim/motor/calculations';
import { useLocale } from '@/providers/i18n/locale-context';

const KW = '°KW';
const MM = UNITS.millimeter.abbreviation;
const PANEL = 'motor.umrechnung';

interface UmrechnungDraft {
  hubL: number | null;
  pleulL: number | null;
  deachsierungL: number | null;
  degree: number | null;
  kante: Steuerkante;
  vorher: number | null;
  nachher: number | null;
}

/** Geometry comes from the vehicle; the conversion inputs are scratch values. */
const VEHICLE_FIELDS = ['hubL', 'pleulL', 'deachsierungL'] as const;
const isKante = (draft: UmrechnungDraft) =>
  draft.kante === 'unterkante' || draft.kante === 'oberkante';

/** v2 MotorUmrechnungView: crank radius, distance to TDC and port timing change in ° and mm. */
export function UmrechnungPanel({ vehicle }: { vehicle: VehicleDto }) {
  const { t } = useLocale();
  const u = t.motor.umrechnung;
  const portSelectId = useId();
  const edgeLabelId = useId();
  const motor = vehicle.motor;
  const { draft, setDraft, dirty, reset } = useWorkDraft<UmrechnungDraft>(
    workDraftKey(vehicle.id, PANEL),
    () => ({
      hubL: motor?.hubL ?? null,
      pleulL: motor?.pleulL ?? null,
      deachsierungL: motor?.deachsierungL ?? 0,
      degree: null,
      kante: 'unterkante',
      vorher: null,
      nachher: null,
    }),
    { compare: VEHICLE_FIELDS, validate: isKante },
  );
  const { hubL, pleulL, deachsierungL, degree, kante, vorher, nachher } = draft;
  const set = (key: Exclude<keyof UmrechnungDraft, 'kante'>) => (value: number | null) =>
    setDraft((d) => ({ ...d, [key]: value }));
  const setKante = (value: Steuerkante) => setDraft((d) => ({ ...d, kante: value }));

  const hubradius = computeHubradius({ hubL, pleulL, deachsierungL });
  const distance = computeDistanceToOT({ pleulL, hubradius, deachsierungL, degree });
  const change = computePortTimingChange({
    vorher,
    nachher,
    kante,
    pleulL,
    hubradius,
    deachsierungL,
  });

  const vehicleTimings: Record<SteuerdiagrammPort, number | null | undefined> = {
    einlass: motor?.einlass?.steuerzeitSZ,
    auslass: motor?.auslass?.steuerzeitSZ,
    ueberstroemer: motor?.ueberstroemer?.steuerzeitSZ,
  };
  const availablePorts = STEUERDIAGRAMM_PORTS.filter((p) => typeof vehicleTimings[p] === 'number');

  return (
    <div className="grid items-start gap-4 lg:grid-cols-2">
      <div className="space-y-4">
        <Section title={u.geometryTitle}>
          <div className="grid gap-4 sm:grid-cols-2">
            <UnitNumberField
              label={u.hub}
              value={hubL}
              baseUnit={FIELD_BASE_UNITS.motor.hubL}
              onChange={set('hubL')}
              testId="motor-umrechnung-hub"
            />
            <UnitNumberField
              label={u.pleuel}
              value={pleulL}
              baseUnit={FIELD_BASE_UNITS.motor.pleulL}
              onChange={set('pleulL')}
              testId="motor-umrechnung-pleuel"
            />
            <UnitNumberField
              label={u.deachsierung}
              value={deachsierungL}
              baseUnit={FIELD_BASE_UNITS.motor.deachsierungL}
              onChange={set('deachsierungL')}
              testId="motor-umrechnung-deachsierung"
            />
            <ResultValue
              label={u.hubradius}
              value={hubradius}
              unit={MM}
              testId="motor-umrechnung-hubradius"
            />
          </div>
          <p className="text-muted-foreground min-h-4 text-xs">
            {hubradius === null ? t.motor.missingInputs : ''}
          </p>
          <DraftNotice dirty={dirty} onReset={reset} testId="motor-umrechnung-draft" />
        </Section>
        <Section title={u.distanceTitle}>
          <div className="grid gap-4 sm:grid-cols-2">
            <UnitNumberField
              label={u.distanceDegree}
              value={degree}
              suffix={KW}
              onChange={set('degree')}
              testId="motor-umrechnung-degree"
            />
            <ResultValue
              label={u.distanceResult}
              value={distance}
              unit={MM}
              testId="motor-umrechnung-distance"
            />
          </div>
        </Section>
      </div>
      <Section title={u.changeTitle}>
        <div className="space-y-1.5">
          <Label htmlFor={portSelectId} className="text-[0.8125rem] font-medium">
            {u.fromVehicle}
          </Label>
          <NativeSelect
            id={portSelectId}
            value=""
            disabled={availablePorts.length === 0}
            onChange={(e) => {
              const port = e.target.value as SteuerdiagrammPort;
              const value = vehicleTimings[port];
              if (typeof value !== 'number') return;
              setDraft((d) => ({ ...d, vorher: value, kante: steuerkanteForPort(port) }));
            }}
            data-testid="motor-umrechnung-port"
          >
            <option value="">{u.fromVehiclePlaceholder}</option>
            {availablePorts.map((port) => (
              <option key={port} value={port}>
                {`${t.motor.steuerdiagramm[port]} (${vehicleTimings[port]} ${KW})`}
              </option>
            ))}
          </NativeSelect>
        </div>
        <div className="space-y-1.5">
          <p id={edgeLabelId} className="text-[0.8125rem] font-medium">
            {u.edge}
          </p>
          <Tabs value={kante} onValueChange={(value) => setKante(value as Steuerkante)}>
            <TabsList aria-labelledby={edgeLabelId} className="w-full">
              <TabsTrigger value="unterkante" title={u.unterkante} data-testid="motor-kante-unten">
                {u.unterkanteShort}
              </TabsTrigger>
              <TabsTrigger value="oberkante" title={u.oberkante} data-testid="motor-kante-oben">
                {u.oberkanteShort}
              </TabsTrigger>
            </TabsList>
          </Tabs>
          <p className="text-muted-foreground text-xs">
            {kante === 'unterkante' ? u.unterkante : u.oberkante}
          </p>
        </div>
        <div className="grid gap-4 sm:grid-cols-2">
          <UnitNumberField
            label={u.vorher}
            value={vorher}
            suffix={KW}
            onChange={set('vorher')}
            testId="motor-umrechnung-vorher"
          />
          <UnitNumberField
            label={u.nachher}
            value={nachher}
            suffix={KW}
            onChange={set('nachher')}
            testId="motor-umrechnung-nachher"
          />
        </div>
        <div className="grid grid-cols-2 gap-3">
          <ResultValue
            label={u.opensBefore}
            value={change?.winkel.oeffnetVorher}
            unit={KW}
            testId="motor-umrechnung-oeffnet-vorher"
          />
          <ResultValue
            label={u.closesBefore}
            value={change?.winkel.schliesstVorher}
            unit={KW}
            testId="motor-umrechnung-schliesst-vorher"
          />
          <ResultValue
            label={u.opensAfter}
            value={change?.winkel.oeffnetNachher}
            unit={KW}
            testId="motor-umrechnung-oeffnet-nachher"
          />
          <ResultValue
            label={u.closesAfter}
            value={change?.winkel.schliesstNachher}
            unit={KW}
            testId="motor-umrechnung-schliesst-nachher"
          />
          <ResultValue
            label={u.differenceDegree}
            value={change?.differenceDegree}
            unit={KW}
            testId="motor-umrechnung-diff-degree"
          />
          <ResultValue
            label={u.differenceLength}
            value={change?.differenceLength}
            unit={MM}
            testId="motor-umrechnung-diff-length"
          />
        </div>
      </Section>
    </div>
  );
}
