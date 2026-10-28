'use client';

import React, { useState } from 'react';
import { Calculator, Save } from 'lucide-react';
import type { DynoDto, DynoPsPointDto, VehicleDto } from '@sim-tuning/common/schemas';
import { roundTo } from '@sim-tuning/common/utils/client';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { Section } from '@/components/sim/shared/module-page';
import { ResultValue, UnitNumberField } from '@/components/sim/shared/unit-number-field';
import { useLocale } from '@/providers/i18n/locale-context';
import { useSimData } from '@/providers/sim/sim-data-context';
import {
  airDensity,
  computePowerCurve,
  DEFAULT_DIAGNOSIS_PARAMS,
  peakPower,
  type DiagnosisParams,
} from '@/lib/sim/dyno/power-curve';
import { PowerCurveChart } from './dyno-charts';
import { DynoNotice, type NoticeMessage } from './dyno-notice';

interface DynoDiagnosisTabProps {
  active: boolean;
  vehicle: VehicleDto;
  dyno: DynoDto | null;
  readOnly: boolean;
  onDynoChange: (dyno: DynoDto) => void;
}

/**
 * v2 `DynoDiagnosisView`: power over engine speed from the stored RPM curve, vehicle mass and
 * environment (air density). The parameters are not persisted (as in v2, except the vehicle
 * weight which v3 edits under Vehicles).
 */
export function DynoDiagnosisTab({
  active,
  vehicle,
  dyno,
  readOnly,
  onDynoChange,
}: DynoDiagnosisTabProps) {
  const { t } = useLocale();
  const d = t.dyno.diagnosis;
  const { repository, environments } = useSimData();
  const environment = environments.find((env) => env.id === dyno?.environmentId) ?? null;
  const [params, setParams] = useState<DiagnosisParams>(() => ({
    ...DEFAULT_DIAGNOSIS_PARAMS,
    gewicht: vehicle.gewicht ?? DEFAULT_DIAGNOSIS_PARAMS.gewicht,
    luftdichte: roundTo(airDensity(environment), 4),
  }));
  const [calculation, setCalculation] = useState<{
    source: DynoDto['drehzahl'];
    curve: DynoPsPointDto[];
  } | null>(null);
  const [notice, setNotice] = useState<NoticeMessage | null>(null);
  const [saving, setSaving] = useState(false);

  const setParam = (key: keyof DiagnosisParams) => (value: number | null) =>
    setParams((prev) => ({ ...prev, [key]: value ?? prev[key] }));

  // A calculation is only shown while it still belongs to the stored RPM curve.
  const computed =
    calculation && dyno && calculation.source === dyno.drehzahl ? calculation.curve : null;
  const curve = computed ?? dyno?.dynoPs ?? [];
  const peak = peakPower(curve);
  const hasRpm = (dyno?.drehzahl.length ?? 0) > 0;

  const calculate = () => {
    if (!dyno) return;
    const next = computePowerCurve(dyno.drehzahl, params);
    setCalculation({ source: dyno.drehzahl, curve: next });
    setNotice(next.length === 0 ? { kind: 'error', text: d.noResult } : null);
  };

  const save = async () => {
    if (!dyno || !computed || readOnly) return;
    setSaving(true);
    try {
      const saved = await repository.saveDynoData(dyno.id, { dynoPs: computed });
      onDynoChange(saved);
      setCalculation(null);
      setNotice({ kind: 'success', text: d.saved });
    } catch {
      setNotice({ kind: 'error', text: t.sim.errorGeneric });
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="space-y-4">
      <div className="grid gap-4 lg:grid-cols-2">
        <Section title={d.paramsTitle}>
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-1 xl:grid-cols-2">
            <UnitNumberField
              label={d.cw}
              value={params.cw}
              onChange={setParam('cw')}
              testId="dyno-diag-cw"
            />
            <UnitNumberField
              label={d.gesamtuebersetzung}
              value={params.gesamtuebersetzung}
              onChange={setParam('gesamtuebersetzung')}
              testId="dyno-diag-ratio"
            />
            <UnitNumberField
              label={d.gewicht}
              value={params.gewicht}
              baseUnit="kilogram"
              onChange={setParam('gewicht')}
              testId="dyno-diag-gewicht"
            />
            <UnitNumberField
              label={d.frontA}
              value={params.frontA}
              baseUnit="squareMeter"
              onChange={setParam('frontA')}
              testId="dyno-diag-fronta"
            />
            <UnitNumberField
              label={d.luftdichte}
              value={params.luftdichte}
              suffix="kg/m³"
              hint={d.luftdichteHint}
              onChange={setParam('luftdichte')}
              testId="dyno-diag-density"
            />
          </div>
        </Section>
        <Section title={d.environment}>
          {environment ? (
            <div className="space-y-2">
              <p className="text-[0.8125rem] font-medium">{environment.name}</p>
              <div className="grid grid-cols-2 gap-2">
                <ResultValue label={d.temperature} value={environment.temperaturT} unit="°C" />
                <ResultValue label={d.pressure} value={environment.luftdruckP} unit="hPa" />
              </div>
            </div>
          ) : (
            <p className="text-muted-foreground text-[0.8125rem]">{d.noEnvironment}</p>
          )}
        </Section>
      </div>

      <Section title={d.title}>
        {!hasRpm && <p className="text-muted-foreground text-[0.8125rem]">{d.noRpm}</p>}
        <div className="flex flex-wrap gap-2">
          <Button
            type="button"
            disabled={!dyno || !hasRpm}
            onClick={calculate}
            data-testid="dyno-diag-calculate"
          >
            <Calculator className="h-4 w-4" />
            {d.calculate}
          </Button>
          <Button
            type="button"
            variant="secondary"
            disabled={!dyno || !computed || computed.length === 0 || readOnly || saving}
            onClick={() => void save()}
            data-testid="dyno-diag-save"
          >
            <Save className="h-4 w-4" />
            {d.save}
          </Button>
        </div>
        <DynoNotice message={notice} testId="dyno-diag-notice" />
        {curve.length > 0 && (
          <>
            {!computed && <p className="text-muted-foreground text-xs">{d.stored}</p>}
            <div className="grid grid-cols-2 gap-2">
              <ResultValue label={d.peak} value={peak?.ps} unit="PS" testId="dyno-diag-peak" />
              <ResultValue label={d.peakAt} value={peak?.drehzahl} unit="U/min" />
            </div>
            {active && (
              <PowerCurveChart
                points={curve}
                name={d.series}
                xLabel={d.rpmAxis}
                yLabel={d.psAxis}
                testId="dyno-diag-chart"
              />
            )}
          </>
        )}
      </Section>
    </div>
  );
}
