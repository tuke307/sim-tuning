'use client';

import React, { useId, useState } from 'react';
import type { SteuerdiagrammPort } from '@sim-tuning/common/logic';
import type { VehicleDto } from '@sim-tuning/common/schemas';
import { Label } from '@sim-tuning/ui/components/ui/label';
import { NativeSelect } from '@sim-tuning/ui/components/ui/native-select';
import {
  PositionHighlightProvider,
  StrokeSample,
  Stueckliste,
  type StuecklisteGroup,
} from '@/components/sim/shared/drawing';
import { Section } from '@/components/sim/shared/module-page';
import { ResultValue, UnitNumberField } from '@/components/sim/shared/unit-number-field';
import {
  computeSteuerzeiten,
  PORT_TIMING_PRESETS,
  STEUERDIAGRAMM_PORTS,
} from '@/lib/sim/motor/calculations';
import { useLocale } from '@/providers/i18n/locale-context';
import { useSimData } from '@/providers/sim/sim-data-context';
import { ApplyToVehicle } from './apply-to-vehicle';
import {
  PORT_SAMPLE_DASH,
  PORT_STYLES,
  SteuerdiagrammSvg,
  type EdgeKey,
} from './steuerdiagramm-svg';

const KW = '°KW';

/** v2 MotorSteuerdiagrammView + PortTimingPopup presets. */
export function SteuerdiagrammPanel({ vehicle }: { vehicle: VehicleDto }) {
  const { t } = useLocale();
  const { rounding } = useSimData();
  const presetId = useId();
  const s = t.motor.steuerdiagramm;
  const [timings, setTimings] = useState<Record<SteuerdiagrammPort, number | null>>({
    einlass: vehicle.motor?.einlass?.steuerzeitSZ ?? null,
    auslass: vehicle.motor?.auslass?.steuerzeitSZ ?? null,
    ueberstroemer: vehicle.motor?.ueberstroemer?.steuerzeitSZ ?? null,
  });
  const result = computeSteuerzeiten(timings);
  const { format } = new Intl.NumberFormat(undefined, {
    maximumFractionDigits: rounding.roundingAccuracy,
  });
  const fmt = (v: number | null | undefined) =>
    v === null || v === undefined ? '–' : `${format(v)} ${KW}`;
  const num = (v: number | null | undefined) => (v === null || v === undefined ? '–' : format(v));

  const description = STEUERDIAGRAMM_PORTS.map((port) => {
    const a = result.angles[port];
    return a ? `${s[port]}: ${s.opens} ${fmt(a.oeffnet)}, ${s.closes} ${fmt(a.schliesst)}` : null;
  })
    .filter(Boolean)
    .join('; ');

  const hasAll = STEUERDIAGRAMM_PORTS.every((p) => result.angles[p] !== null);

  // Opening/closing edges are numbered ①–⑥ on the diagram and in the Stückliste.
  const positions: Partial<Record<EdgeKey, number>> = {};
  const groups: StuecklisteGroup[] = STEUERDIAGRAMM_PORTS.map((port, i) => {
    const a = result.angles[port];
    const open = i * 2 + 1;
    const close = i * 2 + 2;
    if (a) {
      positions[`${port}-oeffnet`] = open;
      positions[`${port}-schliesst`] = close;
    }
    const portLabel = (
      <span className="flex items-center gap-2">
        <StrokeSample className={PORT_STYLES[port].stroke} dash={PORT_SAMPLE_DASH[port]} />
        <span>
          {s[port]} · {s.duration}
        </span>
      </span>
    );
    return {
      key: port,
      testId: `motor-angles-${port}`,
      rows: [
        { key: `${port}-dauer`, label: portLabel, value: num(a?.steuerzeit), unit: KW },
        {
          key: `${port}-oeffnet`,
          pos: a ? open : undefined,
          label: `${s[port]} ${s.opens.toLowerCase()}`,
          value: num(a?.oeffnet),
          unit: KW,
        },
        {
          key: `${port}-schliesst`,
          pos: a ? close : undefined,
          label: `${s[port]} ${s.closes.toLowerCase()}`,
          value: num(a?.schliesst),
          unit: KW,
        },
      ],
    };
  });

  return (
    <PositionHighlightProvider>
      <div className="grid items-start gap-4 lg:grid-cols-2">
        <Section
          title={s.diagramTitle}
          className="lg:sticky lg:top-20"
          bodyClassName="space-y-3 p-3"
        >
          <SteuerdiagrammSvg
            geometry={result.diagram}
            title={s.diagramTitle}
            description={
              description ? `${s.diagramDescription} ${description}` : s.diagramDescription
            }
            labels={{ OT: s.ot, UT: s.ut }}
            positions={positions}
            blowdownLabel={
              result.vorauslass === null || result.vorauslass === undefined
                ? undefined
                : `${format(result.vorauslass)}°`
            }
          />
          <ul
            aria-label={s.legend}
            className="flex flex-wrap justify-center gap-x-5 gap-y-1 text-[0.8125rem]"
          >
            {STEUERDIAGRAMM_PORTS.map((port) => (
              <li key={port} className="flex items-center gap-2">
                <StrokeSample className={PORT_STYLES[port].stroke} dash={PORT_SAMPLE_DASH[port]} />
                {s[port]}
              </li>
            ))}
          </ul>
        </Section>
        <div className="space-y-4">
          <Section title={s.inputsTitle}>
            <div className="space-y-1.5">
              <Label htmlFor={presetId} className="text-[0.8125rem] font-medium">
                {s.preset}
              </Label>
              <NativeSelect
                id={presetId}
                value=""
                onChange={(e) => {
                  const preset = PORT_TIMING_PRESETS.find((p) => p.key === e.target.value);
                  if (preset) {
                    setTimings({
                      einlass: preset.einlass,
                      auslass: preset.auslass,
                      ueberstroemer: preset.ueberstroemer,
                    });
                  }
                }}
                data-testid="motor-steuerdiagramm-preset"
                className="w-full"
              >
                <option value="">{s.presetPlaceholder}</option>
                {PORT_TIMING_PRESETS.map((preset) => (
                  <option key={preset.key} value={preset.key}>
                    {`${s.presets[preset.key]} (${preset.einlass} / ${preset.auslass} / ${preset.ueberstroemer})`}
                  </option>
                ))}
              </NativeSelect>
            </div>
            <div className="grid gap-4 sm:grid-cols-3">
              {STEUERDIAGRAMM_PORTS.map((port) => (
                <UnitNumberField
                  key={port}
                  label={s[port]}
                  value={timings[port]}
                  suffix={KW}
                  onChange={(value) => setTimings((prev) => ({ ...prev, [port]: value }))}
                  testId={`motor-steuerdiagramm-${port}`}
                />
              ))}
            </div>
            <ApplyToVehicle
              testId="motor-steuerdiagramm-apply"
              patch={
                hasAll
                  ? {
                      einlass: { steuerzeitSZ: timings.einlass },
                      auslass: { steuerzeitSZ: timings.auslass },
                      ueberstroemer: { steuerzeitSZ: timings.ueberstroemer },
                    }
                  : null
              }
            />
          </Section>
          <Section title={s.anglesTitle}>
            <Stueckliste
              caption={s.anglesTitle}
              groups={groups}
              testId="motor-steuerdiagramm-table"
            />
            <ResultValue
              label={s.vorauslass}
              value={result.vorauslass}
              unit={KW}
              testId="motor-steuerdiagramm-vorauslass"
            />
          </Section>
        </div>
      </div>
    </PositionHighlightProvider>
  );
}
