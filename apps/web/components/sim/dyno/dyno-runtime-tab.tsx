'use client';

import React, { useEffect, useMemo, useRef, useState } from 'react';
import { AlertTriangle, Play, RotateCcw, Save, Square } from 'lucide-react';
import type { DynoDto, VehicleDto } from '@sim-tuning/common/schemas';
import { Alert, AlertDescription, AlertTitle } from '@sim-tuning/ui/components/ui/alert';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { Checkbox } from '@sim-tuning/ui/components/ui/checkbox';
import { Label } from '@sim-tuning/ui/components/ui/label';
import { cn } from '@sim-tuning/ui/lib/utils';
import { Section } from '@/components/sim/shared/module-page';
import { ResultValue } from '@/components/sim/shared/unit-number-field';
import { useLocale } from '@/providers/i18n/locale-context';
import { useSimData } from '@/providers/sim/sim-data-context';
import type { DynoAudio } from '@/lib/sim/dyno/audio-input';
import { fillTemplate } from '@/lib/sim/dyno/format';
import {
  fitCoastDown,
  maxSpeed,
  MS_TO_KMH,
  speedSeries,
  type SampleLike,
} from '@/lib/sim/dyno/gps';
import { formatStopwatch, type RunPhase } from '@/lib/sim/dyno/runtime';
import { DynoScatterChart, type ScatterSeriesSpec } from './dyno-charts';
import { DynoNotice, type NoticeMessage } from './dyno-notice';
import { useAudioRecorder } from './use-audio-recorder';
import { useGpsRun, type GpsError } from './use-gps-run';
import { useSaveToDyno } from './use-save-to-dyno';

/** Run phases: a 2px top rule in a state colour; the phase name is always spelled out too. */
const PHASE_STYLES: Record<RunPhase, string> = {
  pre: 'bg-border',
  countdownAcceleration: 'bg-warning',
  acceleration: 'bg-success',
  countdownRollout: 'bg-warning',
  rollout: 'bg-mod-einlass',
  done: 'bg-border',
};

interface DynoRuntimeTabProps {
  active: boolean;
  vehicle: VehicleDto;
  dyno: DynoDto | null;
  readOnly: boolean;
  onAudioChange: (audio: DynoAudio) => void;
  onDynoChange: (dyno: DynoDto) => void;
}

/**
 * v2 `DynoRuntimeView` + `DynoGeschwindigkeitView` + `DynoAusrollenView`: guided GPS run
 * (pull away → full throttle → coast down) with optional engine-sound recording during the
 * acceleration, then speed charts of the stored runs and a coast-down evaluation.
 */
export function DynoRuntimeTab({
  active,
  vehicle,
  dyno,
  readOnly,
  onAudioChange,
  onDynoChange,
}: DynoRuntimeTabProps) {
  const { t } = useLocale();
  const r = t.dyno.runtime;
  const { repository } = useSimData();
  const saveToDyno = useSaveToDyno(vehicle, dyno, onDynoChange);
  const run = useGpsRun();
  const recorder = useAudioRecorder();
  const [recordAudio, setRecordAudio] = useState(true);
  const [audioReady, setAudioReady] = useState(false);
  const [saving, setSaving] = useState(false);
  const [notice, setNotice] = useState<NoticeMessage | null>(null);
  const previousPhase = useRef<RunPhase>(run.state.phase);
  const { phase } = run.state;

  // Engine sound is recorded exactly during the "Vollgas" phase so it lines up with the GPS run.
  const { start: startRecorder, stop: stopRecorder } = recorder;
  useEffect(() => {
    const previous = previousPhase.current;
    previousPhase.current = phase;
    if (phase === 'acceleration' && previous !== 'acceleration' && recordAudio) {
      void startRecorder();
    }
    if (previous === 'acceleration' && phase !== 'acceleration') {
      void stopRecorder().then((recorded) => {
        if (!recorded) return;
        onAudioChange(recorded);
        setAudioReady(true);
      });
    }
  }, [phase, recordAudio, startRecorder, stopRecorder, onAudioChange]);

  const secure = typeof window === 'undefined' || window.isSecureContext;
  const geolocation = typeof navigator === 'undefined' || 'geolocation' in navigator;

  const gpsErrorText = (error: GpsError | null) => {
    switch (error) {
      case 'denied':
        return r.gpsDenied;
      case 'insecure':
        return r.insecure;
      case 'timeout':
        return r.gpsTimeout;
      case 'unsupported':
        return r.noGeolocation;
      case 'unavailable':
        return r.gpsUnavailable;
      default:
        return null;
    }
  };

  const save = async () => {
    if (readOnly) return;
    setSaving(true);
    try {
      const saved = await saveToDyno((target) =>
        repository.saveDynoData(target.id, {
          geschwindigkeit: run.state.geschwindigkeit,
          ausrollen: run.state.ausrollen,
        }),
      );
      if (saved) setNotice({ kind: 'success', text: r.saved });
    } catch {
      setNotice({ kind: 'error', text: t.sim.errorGeneric });
    } finally {
      setSaving(false);
    }
  };

  const isCountdown = phase === 'countdownAcceleration' || phase === 'countdownRollout';
  const isMeasuring = phase === 'acceleration' || phase === 'rollout';
  const speedKmh = (run.state.currentSpeed ?? 0) * MS_TO_KMH;
  const hasRunData = run.state.geschwindigkeit.length + run.state.ausrollen.length > 0;

  return (
    <div className="space-y-4">
      <Alert>
        <AlertTriangle className="h-4 w-4" />
        <AlertTitle>{r.warningTitle}</AlertTitle>
        <AlertDescription>
          <ul className="list-disc space-y-1 pl-4">
            <li>{r.warningForeground}</li>
            <li>{r.warningHttps}</li>
            <li>{r.warningSafety}</li>
          </ul>
        </AlertDescription>
      </Alert>
      {!secure && <DynoNotice message={{ kind: 'error', text: r.insecure }} />}
      {!geolocation && <DynoNotice message={{ kind: 'error', text: r.noGeolocation }} />}

      <Section title={r.title}>
        <div
          data-testid="dyno-run-state"
          data-phase={phase}
          className="bg-raised relative flex flex-col items-center gap-2 overflow-hidden rounded-md border px-4 py-8 text-center"
        >
          <span aria-hidden className={cn('absolute inset-x-0 top-0 h-0.5', PHASE_STYLES[phase])} />
          <p className="font-display text-lg font-semibold tracking-[0.06em] uppercase">
            {r.phase[phase]}
          </p>
          {isCountdown && (
            <p className="font-data text-5xl" aria-live="polite">
              {Math.ceil(run.state.countdownMs / 1000)}
            </p>
          )}
          {isMeasuring && (
            <p className="font-data text-4xl">{formatStopwatch(run.state.elapsedMs)}</p>
          )}
          <p className="text-muted-foreground text-sm">
            {r.speed}: <span className="text-foreground font-data">{speedKmh.toFixed(1)} km/h</span>
          </p>
          <p className="text-muted-foreground text-[0.8125rem]">
            {fillTemplate(r.samples, {
              acceleration: run.state.geschwindigkeit.length,
              rollout: run.state.ausrollen.length,
            })}
          </p>
        </div>

        <div className="flex items-center gap-2">
          <Checkbox
            id="dyno-run-record-audio"
            checked={recordAudio}
            disabled={phase !== 'pre' && phase !== 'done'}
            onCheckedChange={(checked) => setRecordAudio(checked === true)}
          />
          <Label htmlFor="dyno-run-record-audio" className="text-[0.8125rem]">
            {r.recordAudio}
          </Label>
        </div>

        <div className="flex flex-wrap gap-2">
          {(phase === 'pre' || phase === 'done') && (
            <Button
              type="button"
              onClick={() => {
                setNotice(null);
                setAudioReady(false);
                run.start();
              }}
              disabled={!secure || !geolocation}
              data-testid="dyno-run-start"
            >
              <Play className="h-4 w-4" />
              {r.start}
            </Button>
          )}
          {(phase === 'countdownAcceleration' || phase === 'acceleration') && (
            <Button type="button" onClick={run.stop} data-testid="dyno-run-stop-acceleration">
              <Square className="h-4 w-4" />
              {r.stopAcceleration}
            </Button>
          )}
          {(phase === 'countdownRollout' || phase === 'rollout') && (
            <Button type="button" onClick={run.stop} data-testid="dyno-run-stop-rollout">
              <Square className="h-4 w-4" />
              {r.stopRollout}
            </Button>
          )}
          {phase !== 'pre' && (
            <Button
              type="button"
              variant="secondary"
              onClick={run.reset}
              data-testid="dyno-run-reset"
            >
              <RotateCcw className="h-4 w-4" />
              {r.reset}
            </Button>
          )}
          {phase === 'done' && (
            <Button
              type="button"
              disabled={readOnly || saving || !hasRunData}
              onClick={() => void save()}
              data-testid="dyno-run-save"
            >
              <Save className="h-4 w-4" />
              {r.save}
            </Button>
          )}
        </div>
        {phase === 'done' && hasRunData && (readOnly || !dyno) && (
          <p className="text-muted-foreground text-[0.8125rem]">
            {readOnly ? t.dyno.readOnlyDyno : r.needDyno}
          </p>
        )}
        <DynoNotice
          message={
            gpsErrorText(run.gpsError) ? { kind: 'error', text: gpsErrorText(run.gpsError)! } : null
          }
          testId="dyno-run-gps-error"
        />
        {audioReady && <DynoNotice message={{ kind: 'info', text: r.audioReady }} />}
        <DynoNotice message={notice} testId="dyno-run-notice" />
      </Section>

      <div className="grid gap-4 xl:grid-cols-2">
        <RunChart
          title={r.accelerationTitle}
          samples={dyno?.geschwindigkeit ?? []}
          active={active}
          testId="dyno-run-acceleration"
        />
        <RunChart
          title={r.rolloutTitle}
          samples={dyno?.ausrollen ?? []}
          active={active}
          massKg={vehicle.gewicht}
          coastDown
          testId="dyno-run-rollout"
        />
      </div>
    </div>
  );
}

function RunChart({
  title,
  samples,
  active,
  massKg,
  coastDown,
  testId,
}: {
  title: string;
  samples: readonly SampleLike[];
  active: boolean;
  massKg?: number | null;
  coastDown?: boolean;
  testId: string;
}) {
  const { t } = useLocale();
  const r = t.dyno.runtime;
  const points = useMemo(() => speedSeries(samples), [samples]);
  const fit = useMemo(
    () => (coastDown ? fitCoastDown(points, massKg) : null),
    [coastDown, points, massKg],
  );
  const series: ScatterSeriesSpec[] = [
    {
      key: 'speed',
      name: title,
      color: coastDown ? 'var(--chart-2)' : 'var(--chart-4)',
      points: points.map((p) => ({ x: p.t, y: p.speed * MS_TO_KMH })),
    },
  ];
  const last = points[points.length - 1];
  if (fit && last) {
    series.push({
      key: 'fit',
      name: r.fitSeries,
      color: 'var(--foreground)',
      line: true,
      points: [
        { x: 0, y: fit.intercept * MS_TO_KMH },
        { x: last.t, y: (fit.intercept - fit.deceleration * last.t) * MS_TO_KMH },
      ],
    });
  }

  return (
    <Section title={title}>
      {points.length === 0 ? (
        <p className="text-muted-foreground text-[0.8125rem]" data-testid={`${testId}-empty`}>
          {r.noRunData}
        </p>
      ) : (
        <>
          <div className="grid grid-cols-2 gap-2">
            <ResultValue label={r.maxSpeed} value={maxSpeed(points) * MS_TO_KMH} unit="km/h" />
            <ResultValue label={r.duration} value={last?.t ?? 0} unit="s" />
            {fit && <ResultValue label={r.deceleration} value={fit.deceleration} unit="m/s²" />}
            {fit && <ResultValue label={r.resistance} value={fit.resistanceN} unit="N" />}
            {fit && <ResultValue label={r.fitQuality} value={fit.r2} />}
          </div>
          {active && (
            <DynoScatterChart
              series={series}
              xLabel={r.timeAxis}
              yLabel={r.speedAxis}
              testId={`${testId}-chart`}
            />
          )}
        </>
      )}
    </Section>
  );
}
