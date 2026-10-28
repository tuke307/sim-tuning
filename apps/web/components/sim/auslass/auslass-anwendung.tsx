'use client';

import React, { useId, useState } from 'react';
import { Calculator, Info } from 'lucide-react';
import type { VehicleDto } from '@sim-tuning/common/schemas';
import { FIELD_BASE_UNITS, type UnitId } from '@sim-tuning/common/units';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { Label } from '@sim-tuning/ui/components/ui/label';
import { NativeSelect } from '@sim-tuning/ui/components/ui/native-select';
import {
  Table,
  TableBody,
  TableCaption,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@sim-tuning/ui/components/ui/table';
import { Section } from '@/components/sim/shared/module-page';
import { ResultValue, UnitNumberField } from '@/components/sim/shared/unit-number-field';
import { AuspuffDrawing } from './auspuff-drawing';
import { FactorSlider } from './factor-slider';
import { SaveMotorButton } from './save-motor-button';
import {
  anwendungDraftFromVehicle,
  buildAnwendungUpdate,
  DIFFUSOR_STAGES,
  missingAnwendungFields,
  runAnwendung,
  type AnwendungDraft,
  type AnwendungOutcome,
  type AuspuffInput,
  type AuspuffResult,
  type DiffusorStage,
} from '@/lib/sim/auslass/auslass-helpers';
import { useLocale } from '@/providers/i18n/locale-context';
import { useSimData } from '@/providers/sim/sim-data-context';

type RowKey = keyof ReturnType<typeof useLocale>['t']['auslass']['anwendung']['rows'];

interface ResultRow {
  key: RowKey;
  code?: string;
  value: number;
}

function resultRows(result: AuspuffResult, input: AuspuffInput, gesamtL: number): ResultRow[] {
  const stage = input.auspuff.diffusorStage;
  const rows: ResultRow[] = [
    { key: 'resonanzL', value: result.resonanzL },
    { key: 'gesamtL', value: gesamtL },
    { key: 'kruemmerD', code: 'D1', value: result.kruemmerD },
    { key: 'kruemmerL', code: 'LK', value: result.kruemmerL },
    { key: 'diffusorD1', value: result.diffusorD1 },
    { key: 'diffusorL1', code: 'LD', value: result.diffusorL1 },
  ];
  if (stage >= 2) {
    rows.push(
      { key: 'diffusorD2', value: result.diffusorD2 },
      { key: 'diffusorL2', value: result.diffusorL2 },
    );
  }
  if (stage >= 3) {
    rows.push(
      { key: 'diffusorD3', value: result.diffusorD3 },
      { key: 'diffusorL3', value: result.diffusorL3 },
    );
  }
  rows.push(
    { key: 'mittelteilD', value: result.mittelteilD },
    { key: 'mittelteilL', code: 'LM', value: result.mittelteilL },
    { key: 'gegenkonusD', value: result.gegenkonusD },
    { key: 'gegenkonusL', code: 'LG', value: result.gegenkonusL },
    { key: 'endrohrD', code: 'D2', value: input.auspuff.endrohrD ?? 0 },
    { key: 'endrohrL', code: 'LE', value: input.auspuff.endrohrL ?? 0 },
  );
  return rows;
}

/** v2 `AuslassAnwendungView`: full expansion chamber calculation and drawing. */
export function AuslassAnwendung({ vehicle }: { vehicle: VehicleDto }) {
  const { t } = useLocale();
  const { rounding } = useSimData();
  const a = t.auslass.anwendung;
  const stageId = useId();
  const readOnly = vehicle.isSystem;
  const [draft, setDraft] = useState<AnwendungDraft>(() => anwendungDraftFromVehicle(vehicle));
  const [calc, setCalc] = useState<{ outcome: AnwendungOutcome; draftJson: string } | null>(null);
  const set = (key: keyof AnwendungDraft) => (value: number | null) =>
    setDraft((d) => ({ ...d, [key]: value }));

  const draftJson = JSON.stringify(draft);
  const missing = missingAnwendungFields(draft);
  const outcome = calc?.outcome ?? null;
  const stale = calc !== null && calc.draftJson !== draftJson;
  const ok = outcome?.status === 'ok' ? outcome : null;
  const fmt = new Intl.NumberFormat(undefined, {
    maximumFractionDigits: rounding.roundingAccuracy,
  });

  const field = (
    key: Exclude<keyof AnwendungDraft, 'diffusorStage'>,
    opts: { baseUnit?: UnitId; suffix?: string },
  ) => (
    <UnitNumberField
      label={a[key]}
      value={draft[key]}
      baseUnit={opts.baseUnit}
      suffix={opts.suffix}
      onChange={set(key)}
      readOnly={readOnly}
      testId={`auslass-anwendung-${key}`}
    />
  );

  return (
    <div className="space-y-5">
      <div className="grid gap-5 lg:grid-cols-2">
        <Section title={a.sectionDaten}>
          <div className="grid gap-4 sm:grid-cols-2">
            {field('abgasT', { baseUnit: FIELD_BASE_UNITS.auspuff.abgasT })}
            {field('resonanzU', { suffix: 'U/min' })}
            {field('auslassDurchmesserD', { baseUnit: FIELD_BASE_UNITS.auslass.durchmesserD })}
            {field('auslassLaengeL', { baseUnit: FIELD_BASE_UNITS.auslass.laengeL })}
            {field('auslassSteuerzeitSZ', { suffix: '°KW' })}
            {field('auslassFlaecheA', { baseUnit: FIELD_BASE_UNITS.auslass.flaecheA })}
          </div>
        </Section>

        <Section title={a.sectionKruemmer}>
          <FactorSlider
            label={a.kruemmerF}
            value={draft.kruemmerF}
            onChange={set('kruemmerF')}
            readOnly={readOnly}
            testId="auslass-anwendung-kruemmerF"
          />
          {field('kruemmerW', { suffix: '°' })}
        </Section>

        <Section title={a.sectionMittelteil}>{field('mittelteilF', {})}</Section>

        <Section title={a.sectionGegenkonus}>{field('gegenKonusW', { suffix: '°' })}</Section>

        <Section title={a.sectionEndrohr}>
          <div className="grid gap-4 sm:grid-cols-2">
            {field('endrohrD', { baseUnit: FIELD_BASE_UNITS.auspuff.endrohrD })}
            {field('endrohrL', { baseUnit: FIELD_BASE_UNITS.auspuff.endrohrL })}
          </div>
        </Section>

        <Section title={a.sectionDiffusor}>
          <div className="space-y-1.5">
            <Label htmlFor={stageId} className="text-[0.8125rem] font-medium">
              {a.diffusorStage}
            </Label>
            <NativeSelect
              id={stageId}
              data-testid="auslass-anwendung-diffusorStage"
              value={draft.diffusorStage}
              disabled={readOnly}
              onChange={(e) =>
                setDraft((d) => ({ ...d, diffusorStage: Number(e.target.value) as DiffusorStage }))
              }
              className="w-full sm:w-auto"
            >
              {DIFFUSOR_STAGES.map((stage) => (
                <option key={stage} value={stage}>
                  {a.stage.replace('{{value}}', String(stage))}
                </option>
              ))}
            </NativeSelect>
          </div>
          {draft.diffusorStage === 1 ? field('diffusorW1', { suffix: '°' }) : null}
          <p
            className="text-muted-foreground flex items-start gap-2 text-xs"
            data-testid="auslass-anwendung-stage-hint"
          >
            <Info className="mt-0.5 h-3.5 w-3.5 shrink-0" aria-hidden />
            {a.stageHint}
          </p>
        </Section>
      </div>

      <Section title={a.sectionErgebnis}>
        <div className="flex flex-wrap items-center gap-3">
          <Button
            type="button"
            variant="secondary"
            disabled={missing.length > 0}
            onClick={() => setCalc({ outcome: runAnwendung(draft), draftJson })}
            data-testid="auslass-anwendung-calculate"
          >
            <Calculator className="h-4 w-4" aria-hidden />
            {a.calculate}
          </Button>
        </div>
        <div role="status" aria-live="polite" className="space-y-1 text-[0.8125rem]">
          {missing.length > 0 && (
            <p className="text-muted-foreground" data-testid="auslass-anwendung-missing">
              {a.missing.replace('{{value}}', missing.map((key) => a[key]).join(', '))}
            </p>
          )}
          {outcome?.status === 'invalid' && !stale && (
            <p className="text-destructive" data-testid="auslass-anwendung-invalid">
              {a.invalid}
            </p>
          )}
          {ok && stale && <p className="text-muted-foreground">{a.stale}</p>}
        </div>

        {ok && (
          <div className={stale ? 'space-y-4 opacity-60' : 'space-y-4'}>
            <div className="grid gap-3 sm:grid-cols-2">
              <ResultValue
                label={a.rows.resonanzL}
                value={ok.result.resonanzL}
                unit="mm"
                testId="auslass-anwendung-resonanzL"
              />
              <ResultValue
                label={a.rows.gesamtL}
                value={ok.gesamtL}
                unit="mm"
                testId="auslass-anwendung-gesamtL"
              />
            </div>
            <AuspuffDrawing geometry={ok.geometry} />
            <div className="overflow-x-auto">
              <Table data-testid="auslass-anwendung-results">
                <TableCaption className="sr-only">{a.resultTable}</TableCaption>
                <TableHeader>
                  <TableRow>
                    <TableHead scope="col">{a.colPart}</TableHead>
                    <TableHead scope="col">{a.colCode}</TableHead>
                    <TableHead scope="col" className="text-right">
                      {a.colValue}
                    </TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {resultRows(ok.result, ok.input, ok.gesamtL).map((row) => (
                    <TableRow key={row.key}>
                      <TableHead scope="row" className="font-normal">
                        {a.rows[row.key]}
                      </TableHead>
                      <TableCell className="text-muted-foreground">{row.code ?? '–'}</TableCell>
                      <TableCell
                        className="text-right tabular-nums"
                        data-testid={`auslass-anwendung-row-${row.key}`}
                      >
                        {fmt.format(row.value)} mm
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          </div>
        )}

        <SaveMotorButton
          buildUpdate={(name) =>
            ok && !stale ? buildAnwendungUpdate(name, draft, ok.result, ok.gesamtL) : null
          }
          hint={a.saveHint}
          testId="auslass-anwendung-save"
        />
      </Section>
    </div>
  );
}
