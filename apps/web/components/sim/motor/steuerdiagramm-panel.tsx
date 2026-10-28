'use client';

import React, { useId, useState } from 'react';
import type { SteuerdiagrammPort } from '@sim-tuning/common/logic';
import type { VehicleDto } from '@sim-tuning/common/schemas';
import { Label } from '@sim-tuning/ui/components/ui/label';
import { NativeSelect } from '@sim-tuning/ui/components/ui/native-select';
import { cn } from '@sim-tuning/ui/lib/utils';
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
import { PORT_COLOR_CLASSES, SteuerdiagrammSvg } from './steuerdiagramm-svg';

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

  const description = STEUERDIAGRAMM_PORTS.map((port) => {
    const a = result.angles[port];
    return a ? `${s[port]}: ${s.opens} ${fmt(a.oeffnet)}, ${s.closes} ${fmt(a.schliesst)}` : null;
  })
    .filter(Boolean)
    .join('; ');

  const hasAll = STEUERDIAGRAMM_PORTS.every((p) => result.angles[p] !== null);

  return (
    <div className="grid items-start gap-4 lg:grid-cols-2">
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
          <div className="overflow-x-auto">
            <table className="w-full text-sm tabular-nums" data-testid="motor-steuerdiagramm-table">
              <thead>
                <tr className="text-muted-foreground text-left text-xs">
                  <th scope="col" className="py-1.5 pr-3 font-medium">
                    {s.port}
                  </th>
                  <th scope="col" className="py-1.5 pr-3 font-medium">
                    {s.duration}
                  </th>
                  <th scope="col" className="py-1.5 pr-3 font-medium">
                    {s.opens}
                  </th>
                  <th scope="col" className="py-1.5 font-medium">
                    {s.closes}
                  </th>
                </tr>
              </thead>
              <tbody>
                {STEUERDIAGRAMM_PORTS.map((port) => {
                  const a = result.angles[port];
                  return (
                    <tr key={port} className="border-t" data-testid={`motor-angles-${port}`}>
                      <th scope="row" className="py-2 pr-3 text-left font-medium">
                        <span className="flex items-center gap-2">
                          <span
                            aria-hidden
                            className={cn('h-2.5 w-2.5 rounded-full', PORT_COLOR_CLASSES[port].bg)}
                          />
                          {s[port]}
                        </span>
                      </th>
                      <td className="py-2 pr-3">{fmt(a?.steuerzeit)}</td>
                      <td className="py-2 pr-3">{fmt(a?.oeffnet)}</td>
                      <td className="py-2">{fmt(a?.schliesst)}</td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
          <ResultValue
            label={s.vorauslass}
            value={result.vorauslass}
            unit={KW}
            testId="motor-steuerdiagramm-vorauslass"
          />
        </Section>
      </div>
      <Section title={s.diagramTitle}>
        <SteuerdiagrammSvg
          geometry={result.diagram}
          title={s.diagramTitle}
          description={
            description ? `${s.diagramDescription} ${description}` : s.diagramDescription
          }
          labels={{ OT: s.ot, UT: s.ut }}
        />
        <ul aria-label={s.legend} className="flex flex-wrap justify-center gap-x-4 gap-y-1 text-xs">
          {STEUERDIAGRAMM_PORTS.map((port) => (
            <li key={port} className="flex items-center gap-1.5">
              <span
                aria-hidden
                className={cn('h-1 w-5 rounded-full', PORT_COLOR_CLASSES[port].bg)}
              />
              {s[port]}
            </li>
          ))}
        </ul>
      </Section>
    </div>
  );
}
