'use client';

import React, { useId, useMemo, useRef, useState } from 'react';
import { Download, FileAudio, Filter, Mic, Play, Save, Sparkles, Square, X } from 'lucide-react';
import {
  AUDIO_FFT_SIZES,
  calculateClusters,
  encodeWavMono,
  fitDrehzahlCurve,
  type DrehzahlPoint,
} from '@sim-tuning/common/audio';
import type { DynoDto } from '@sim-tuning/common/schemas';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { Label } from '@sim-tuning/ui/components/ui/label';
import { NativeSelect } from '@sim-tuning/ui/components/ui/native-select';
import { Progress } from '@sim-tuning/ui/components/ui/progress';
import { Section } from '@/components/sim/shared/module-page';
import { ResultValue, UnitNumberField } from '@/components/sim/shared/unit-number-field';
import { useLocale } from '@/providers/i18n/locale-context';
import { useSimData } from '@/providers/sim/sim-data-context';
import {
  DEFAULT_AUDIO_OPTIONS,
  downsample,
  downsampleClusters,
  type AnalysisResult,
  type AudioAnalysisOptions,
} from '@/lib/sim/dyno/audio-analysis';
import {
  AUDIO_FILE_ACCEPT,
  decodeToPcm,
  downloadBlob,
  safeFileName,
  validateAudioFile,
  type DynoAudio,
} from '@/lib/sim/dyno/audio-input';
import { fillTemplate } from '@/lib/sim/dyno/format';
import { toDrehzahlSeries } from '@/lib/sim/dyno/power-curve';
import { DynoScatterChart, seriesColor, type ScatterSeriesSpec } from './dyno-charts';
import { DynoNotice, type NoticeMessage } from './dyno-notice';
import { useAudioAnalysis } from './use-audio-analysis';
import { useAudioRecorder } from './use-audio-recorder';

export const DEMO_RECORDING_URL = '/demo/dyno-acceleration.wav';

/** SVG scatter plots get slow beyond a few thousand markers; the analysis keeps every point. */
const MAX_PLOTTED_POINTS = 2500;

interface ClusterFilter {
  result: AnalysisResult;
  epsilon: number;
  clusters: DrehzahlPoint[][];
}

interface ClusterSelection {
  filter: ClusterFilter;
  index: number;
  fit: DrehzahlPoint[];
}

interface DynoAudioTabProps {
  active: boolean;
  dyno: DynoDto | null;
  readOnly: boolean;
  audio: DynoAudio | null;
  onAudioChange: (audio: DynoAudio | null) => void;
  onDynoChange: (dyno: DynoDto) => void;
}

/**
 * v2 `DynoAudioView`: engine-sound input, analysis parameters, RPM scatter plot, DBSCAN "filter"
 * into curves ("Graph 1…n"), pick a curve → degree-5 fit → save as the dyno's RPM curve.
 */
export function DynoAudioTab({
  active,
  dyno,
  readOnly,
  audio,
  onAudioChange,
  onDynoChange,
}: DynoAudioTabProps) {
  const { t } = useLocale();
  const { repository } = useSimData();
  const fileRef = useRef<HTMLInputElement>(null);
  const fftId = useId();
  const clusterId = useId();
  const recorder = useAudioRecorder();
  const analysis = useAudioAnalysis();
  const [options, setOptions] = useState<AudioAnalysisOptions>(DEFAULT_AUDIO_OPTIONS);
  const [analyzedAudio, setAnalyzedAudio] = useState<DynoAudio | null>(null);
  const [filter, setFilter] = useState<ClusterFilter | null>(null);
  const [selection, setSelection] = useState<ClusterSelection | null>(null);
  const [loadingSource, setLoadingSource] = useState(false);
  const [notice, setNotice] = useState<NoticeMessage | null>(null);
  const [saving, setSaving] = useState(false);

  const a = t.dyno.audio;
  const result =
    analysis.state.result && analyzedAudio && analyzedAudio === audio
      ? analysis.state.result
      : null;
  const activeFilter = filter && result && filter.result === result ? filter : null;
  const activeSelection =
    selection && activeFilter && selection.filter === activeFilter ? selection : null;
  const running = analysis.state.status === 'running';

  const setOption = (key: keyof AudioAnalysisOptions) => (value: number | null) =>
    setOptions((prev) => ({ ...prev, [key]: value ?? prev[key] }));

  const loadAudio = (next: DynoAudio) => {
    analysis.reset();
    setNotice(null);
    onAudioChange(next);
  };

  const loadFile = async (file: File) => {
    const invalid = validateAudioFile(file);
    if (invalid) {
      setNotice({
        kind: 'error',
        text:
          invalid === 'tooLarge'
            ? a.fileTooLarge
            : invalid === 'empty'
              ? a.fileEmpty
              : a.fileUnsupported,
      });
      return;
    }
    setLoadingSource(true);
    try {
      const pcm = await decodeToPcm(await file.arrayBuffer());
      loadAudio({ ...pcm, source: 'file', label: file.name });
    } catch {
      setNotice({ kind: 'error', text: a.decodeFailed });
    } finally {
      setLoadingSource(false);
    }
  };

  const loadDemo = async () => {
    setLoadingSource(true);
    try {
      const response = await fetch(DEMO_RECORDING_URL);
      if (!response.ok) throw new Error(String(response.status));
      const pcm = await decodeToPcm(await response.arrayBuffer());
      loadAudio({ ...pcm, source: 'demo', label: a.sourceDemo });
    } catch {
      setNotice({ kind: 'error', text: a.demoFailed });
    } finally {
      setLoadingSource(false);
    }
  };

  const toggleRecording = async () => {
    if (recorder.status === 'recording') {
      const recorded = await recorder.stop();
      if (recorded) loadAudio(recorded);
      return;
    }
    setNotice(null);
    await recorder.start();
  };

  const analyze = () => {
    if (!audio) return;
    setNotice(null);
    setAnalyzedAudio(audio);
    analysis.run(audio, options);
  };

  const runFilter = () => {
    if (!result) return;
    const sameEpsilon = analysis.state.options?.epsilon === options.epsilon;
    // Re-clustering existing points is cheap (grid DBSCAN), so a changed ε runs on the main thread.
    const clusters = sameEpsilon
      ? result.clusters
      : calculateClusters(result.points, options.epsilon);
    setFilter({ result, epsilon: options.epsilon, clusters });
    setSelection(null);
  };

  const pickCluster = (value: string) => {
    if (!activeFilter || value === '') {
      setSelection(null);
      return;
    }
    const index = Number(value);
    const cluster = activeFilter.clusters[index];
    if (!cluster) return;
    setSelection({ filter: activeFilter, index, fit: fitDrehzahlCurve(cluster) });
  };

  const saveCurve = async () => {
    if (!dyno || !activeSelection || readOnly) return;
    setSaving(true);
    try {
      const saved = await repository.saveDynoData(dyno.id, {
        drehzahl: toDrehzahlSeries(activeSelection.fit),
      });
      onDynoChange(saved);
      setNotice({ kind: 'success', text: a.curveSaved });
    } catch {
      setNotice({ kind: 'error', text: t.sim.errorGeneric });
    } finally {
      setSaving(false);
    }
  };

  const series = useMemo<ScatterSeriesSpec[]>(() => {
    if (!result) return [];
    if (activeSelection) {
      const cluster = activeSelection.filter.clusters[activeSelection.index] ?? [];
      return [
        {
          key: `graph-${activeSelection.index}`,
          name: `${a.graph} ${activeSelection.index + 1}`,
          color: seriesColor(activeSelection.index),
          points: downsample(cluster, MAX_PLOTTED_POINTS),
        },
        {
          key: 'fit',
          name: a.fitSeries,
          color: 'var(--foreground)',
          points: downsample(activeSelection.fit, 600),
          line: true,
        },
      ];
    }
    if (activeFilter) {
      return downsampleClusters(activeFilter.clusters, MAX_PLOTTED_POINTS).map((points, i) => ({
        key: `graph-${i}`,
        name: `${a.graph} ${i + 1}`,
        color: seriesColor(i),
        points,
      }));
    }
    return [
      {
        key: 'points',
        name: a.pointsSeries,
        color: 'var(--chart-1)',
        points: downsample(result.points, MAX_PLOTTED_POINTS),
      },
    ];
  }, [result, activeFilter, activeSelection, a.graph, a.fitSeries, a.pointsSeries]);

  const plotted = series.reduce((sum, s) => sum + (s.line ? 0 : s.points.length), 0);
  const total = activeSelection
    ? (activeSelection.filter.clusters[activeSelection.index]?.length ?? 0)
    : activeFilter
      ? activeFilter.clusters.reduce((sum, c) => sum + c.length, 0)
      : (result?.points.length ?? 0);

  const errorText =
    analysis.state.status === 'error' && analysis.state.error
      ? analysis.state.error === 'cancelled'
        ? a.cancelled
        : a.errors[analysis.state.error]
      : null;
  const recorderError =
    recorder.error === 'denied'
      ? a.micDenied
      : recorder.error === 'unsupported'
        ? a.micUnsupported
        : recorder.error === 'failed'
          ? a.decodeFailed
          : null;

  const sourceLabel = audio
    ? audio.source === 'demo'
      ? a.sourceDemo
      : `${audio.source === 'file' ? a.sourceFile : a.sourceRecording}: ${audio.label}`
    : null;

  return (
    <div className="space-y-4">
      <div className="grid gap-4 lg:grid-cols-2">
        <Section title={a.sourceTitle}>
          <p className="text-muted-foreground text-[0.8125rem]">{a.sourceHint}</p>
          <div className="flex flex-wrap gap-2">
            <Button
              type="button"
              variant="secondary"
              disabled={loadingSource || recorder.status !== 'idle'}
              onClick={() => fileRef.current?.click()}
              data-testid="dyno-audio-upload"
            >
              <FileAudio className="h-4 w-4" />
              {a.upload}
            </Button>
            <input
              ref={fileRef}
              type="file"
              accept={AUDIO_FILE_ACCEPT}
              className="hidden"
              data-testid="dyno-audio-file-input"
              onChange={(e) => {
                const file = e.target.files?.[0];
                e.target.value = '';
                if (file) void loadFile(file);
              }}
            />
            <Button
              type="button"
              variant={recorder.status === 'recording' ? 'destructive' : 'secondary'}
              disabled={
                loadingSource ||
                recorder.status === 'requesting' ||
                recorder.status === 'processing'
              }
              onClick={() => void toggleRecording()}
              data-testid="dyno-audio-record"
            >
              {recorder.status === 'recording' ? (
                <Square className="h-4 w-4" />
              ) : (
                <Mic className="h-4 w-4" />
              )}
              {recorder.status === 'recording' ? a.stopRecording : a.record}
            </Button>
            <Button
              type="button"
              variant="secondary"
              disabled={loadingSource || recorder.status !== 'idle'}
              onClick={() => void loadDemo()}
              data-testid="dyno-audio-demo"
            >
              <Sparkles className="h-4 w-4" />
              {a.demo}
            </Button>
          </div>
          {recorder.status === 'recording' && (
            <p className="text-destructive flex items-center gap-2 text-[0.8125rem]" role="status">
              <span className="bg-destructive h-2 w-2 animate-pulse rounded-full" />
              {a.recording}
            </p>
          )}
          {(loadingSource || recorder.status === 'processing') && (
            <p className="text-muted-foreground text-[0.8125rem]" role="status">
              {a.decoding}
            </p>
          )}
          {recorderError && <DynoNotice message={{ kind: 'error', text: recorderError }} />}
          {audio ? (
            <div className="space-y-2" data-testid="dyno-audio-loaded">
              <p className="text-[0.8125rem] font-medium">
                {a.loaded}: {sourceLabel}
              </p>
              <div className="grid grid-cols-2 gap-2">
                <ResultValue
                  label={a.duration}
                  value={audio.samples.length / audio.sampleRate}
                  unit="s"
                />
                <ResultValue label={a.sampleRate} value={audio.sampleRate} unit="Hz" />
              </div>
              {audio.source === 'recording' && (
                <Button
                  type="button"
                  size="sm"
                  variant="ghost"
                  onClick={() =>
                    downloadBlob(
                      new Blob([encodeWavMono(audio.samples, audio.sampleRate)], {
                        type: 'audio/wav',
                      }),
                      safeFileName(`dyno-recording-${audio.label}`, 'wav'),
                    )
                  }
                >
                  <Download className="h-4 w-4" />
                  {a.downloadWav}
                </Button>
              )}
            </div>
          ) : (
            <p className="text-muted-foreground text-[0.8125rem]">{a.noAudio}</p>
          )}
        </Section>

        <Section title={a.paramsTitle}>
          <div className="grid gap-3 sm:grid-cols-2">
            <UnitNumberField
              label={a.frequenzbeginn}
              value={options.frequenzbeginn}
              suffix="U/min"
              onChange={setOption('frequenzbeginn')}
              testId="dyno-audio-frequenzbeginn"
            />
            <UnitNumberField
              label={a.frequenzende}
              value={options.frequenzende}
              suffix="U/min"
              onChange={setOption('frequenzende')}
              testId="dyno-audio-frequenzende"
            />
            <UnitNumberField
              label={a.epsilon}
              value={options.epsilon}
              onChange={setOption('epsilon')}
              testId="dyno-audio-epsilon"
            />
            <UnitNumberField
              label={a.intensity}
              value={options.intensity}
              suffix="%"
              onChange={setOption('intensity')}
              testId="dyno-audio-intensity"
            />
            <div className="space-y-1.5 sm:col-span-2">
              <Label htmlFor={fftId} className="text-[0.8125rem] font-medium">
                {a.fftSize}
              </Label>
              <NativeSelect
                id={fftId}
                data-testid="dyno-audio-fft"
                value={options.fftSize}
                onChange={(e) =>
                  setOptions((prev) => ({ ...prev, fftSize: Number(e.target.value) }))
                }
              >
                {AUDIO_FFT_SIZES.map((size) => (
                  <option key={size} value={size}>
                    2^{Math.log2(size)} = {size}
                  </option>
                ))}
              </NativeSelect>
            </div>
          </div>
          <div className="flex flex-wrap gap-2">
            {running ? (
              <Button
                type="button"
                variant="secondary"
                onClick={analysis.cancel}
                data-testid="dyno-audio-cancel"
              >
                <X className="h-4 w-4" />
                {a.cancel}
              </Button>
            ) : (
              <Button
                type="button"
                disabled={!audio || recorder.status !== 'idle'}
                onClick={analyze}
                data-testid="dyno-audio-analyze"
              >
                <Play className="h-4 w-4" />
                {a.analyze}
              </Button>
            )}
            <Button
              type="button"
              variant="secondary"
              disabled={!result || running}
              onClick={runFilter}
              data-testid="dyno-audio-filter"
            >
              <Filter className="h-4 w-4" />
              {a.filter}
            </Button>
          </div>
          {running && (
            <div className="space-y-1" role="status" data-testid="dyno-audio-progress">
              <Progress value={Math.round(analysis.state.progress * 100)} />
              <p className="text-muted-foreground text-xs">
                {a.analyzing} {Math.round(analysis.state.progress * 100)} %
              </p>
            </div>
          )}
          {errorText && (
            <DynoNotice
              message={{
                kind: analysis.state.error === 'cancelled' ? 'info' : 'error',
                text: errorText,
              }}
              testId="dyno-audio-error"
            />
          )}
          <p className="text-muted-foreground text-xs">{a.filterHint}</p>
        </Section>
      </div>

      {result && (
        <Section title={a.chartTitle}>
          <div className="flex flex-wrap items-end gap-3">
            <ResultValue label={a.points} value={result.points.length} testId="dyno-audio-points" />
            {activeFilter && (
              <ResultValue
                label={a.clusters}
                value={activeFilter.clusters.length}
                testId="dyno-audio-clusters"
              />
            )}
            {activeFilter && (
              <div className="min-w-[12rem] space-y-1.5">
                <Label htmlFor={clusterId} className="text-[0.8125rem] font-medium">
                  {a.pickCluster}
                </Label>
                <NativeSelect
                  id={clusterId}
                  data-testid="dyno-audio-cluster"
                  value={activeSelection ? String(activeSelection.index) : ''}
                  onChange={(e) => pickCluster(e.target.value)}
                >
                  <option value="">{a.pickClusterPlaceholder}</option>
                  {activeFilter.clusters.map((cluster, i) => (
                    <option key={i} value={i}>
                      {a.graph} {i + 1} ({cluster.length})
                    </option>
                  ))}
                </NativeSelect>
              </div>
            )}
          </div>
          {active && (
            <DynoScatterChart
              series={series}
              xLabel={a.timeAxis}
              yLabel={a.rpmAxis}
              testId="dyno-audio-chart"
            />
          )}
          {plotted < total && (
            <p className="text-muted-foreground text-xs">
              {fillTemplate(a.shownOf, { shown: plotted, total })}
            </p>
          )}
          {activeSelection && (
            <div className="flex flex-wrap items-center gap-2">
              <Button
                type="button"
                disabled={!dyno || readOnly || saving || activeSelection.fit.length === 0}
                onClick={() => void saveCurve()}
                data-testid="dyno-audio-save"
              >
                <Save className="h-4 w-4" />
                {a.saveCurve}
              </Button>
              {(!dyno || readOnly) && (
                <span className="text-muted-foreground text-xs">{a.needDyno}</span>
              )}
            </div>
          )}
        </Section>
      )}

      <DynoNotice message={notice} testId="dyno-audio-notice" />
      {dyno && dyno.drehzahl.length > 0 && (
        <p className="text-muted-foreground text-xs" data-testid="dyno-audio-stored">
          {fillTemplate(a.storedCurve, { count: dyno.drehzahl.length })}
        </p>
      )}
    </div>
  );
}
