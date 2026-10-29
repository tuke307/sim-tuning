'use client';

import React, { useCallback, useEffect, useState } from 'react';
import { Copy } from 'lucide-react';
import type { DynoDto } from '@sim-tuning/common/schemas';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { Card } from '@sim-tuning/ui/components/ui/card';
import { Spinner } from '@sim-tuning/ui/components/ui/spinner';
import { ModulePage } from '@/components/sim/shared/module-page';
import { useLocale } from '@/providers/i18n/locale-context';
import { useSimData } from '@/providers/sim/sim-data-context';
import { useSubTab } from '@/lib/navigation/nav-state';
import type { DynoAudio } from '@/lib/sim/dyno/audio-input';
import { DynoAudioTab } from './dyno-audio-tab';
import { DynoDataTab } from './dyno-data-tab';
import { DynoDiagnosisTab } from './dyno-diagnosis-tab';
import { DynoRuntimeTab } from './dyno-runtime-tab';

const DYNO_TABS = ['data', 'audio', 'runtime', 'diagnosis'] as const;
type DynoTab = (typeof DYNO_TABS)[number];

interface LoadedDyno {
  vehicleId: string;
  dyno: DynoDto | null;
}

/**
 * Dyno module (v2 `DynoMainView`): Daten, Audio, Messung (GPS runtime) and Diagnose sub-tabs.
 * All tabs stay mounted (hidden when inactive) so a running GPS measurement or audio analysis
 * survives switching tabs.
 */
export function DynoView() {
  const { t } = useLocale();
  const { repository, selectedVehicle, setVehicle, selectVehicle } = useSimData();
  const [tab, setTab] = useSubTab<DynoTab>(DYNO_TABS, 'data');
  const [duplicating, setDuplicating] = useState(false);
  const [duplicateFailed, setDuplicateFailed] = useState(false);
  const [loaded, setLoaded] = useState<LoadedDyno | null>(null);
  const [audio, setAudio] = useState<DynoAudio | null>(null);

  const vehicleId = selectedVehicle?.id ?? null;
  const dynoId = selectedVehicle?.dynoId ?? null;

  useEffect(() => {
    if (!vehicleId) return;
    let cancelled = false;
    repository
      .getDyno(vehicleId)
      .then((dyno) => {
        if (!cancelled) setLoaded({ vehicleId, dyno });
      })
      .catch(() => {
        if (!cancelled) setLoaded({ vehicleId, dyno: null });
      });
    return () => {
      cancelled = true;
    };
  }, [repository, vehicleId, dynoId]);

  const onDynoChange = useCallback(
    (dyno: DynoDto | null) => {
      if (vehicleId) setLoaded({ vehicleId, dyno });
    },
    [vehicleId],
  );

  const ready = !!vehicleId && loaded?.vehicleId === vehicleId;
  const dyno = ready ? (loaded?.dyno ?? null) : null;
  const readOnly = !!selectedVehicle?.isSystem;

  const tabs: { key: DynoTab; label: string }[] = [
    { key: 'data', label: t.dyno.tabs.data },
    { key: 'audio', label: t.dyno.tabs.audio },
    { key: 'runtime', label: t.dyno.tabs.runtime },
    { key: 'diagnosis', label: t.dyno.tabs.diagnosis },
  ];

  const duplicate = async () => {
    if (!selectedVehicle) return;
    setDuplicating(true);
    setDuplicateFailed(false);
    try {
      const copy = await repository.duplicateVehicle(selectedVehicle.id);
      setVehicle(copy);
      selectVehicle(copy.id);
    } catch {
      setDuplicateFailed(true);
    } finally {
      setDuplicating(false);
    }
  };

  // Own vehicles get their dyno automatically on the first save; starter vehicles need a copy.
  const readOnlyHint =
    readOnly && ready ? (
      <Card className="flex flex-row flex-wrap items-center justify-between gap-3 border-dashed p-4 text-[0.9375rem]">
        <span>{t.dyno.readOnlyDyno}</span>
        <Button
          type="button"
          size="sm"
          variant="secondary"
          disabled={duplicating}
          onClick={() => void duplicate()}
        >
          <Copy className="h-4 w-4" aria-hidden />
          {t.dyno.duplicateVehicle}
        </Button>
        {duplicateFailed && (
          <p role="alert" className="text-destructive w-full text-[0.8125rem]">
            {t.sim.errorGeneric}
          </p>
        )}
      </Card>
    ) : null;

  return (
    <ModulePage
      module="dyno"
      title={t.dyno.title}
      subtitle={t.dyno.subtitle}
      tabs={tabs}
      activeTab={tab}
      onTabChange={setTab}
    >
      {!ready || !selectedVehicle ? (
        <p className="text-muted-foreground flex items-center gap-2 text-sm">
          <Spinner className="h-4 w-4" /> {t.dyno.loading}
        </p>
      ) : (
        <>
          <div hidden={tab !== 'data'} data-testid="dyno-panel-data">
            <DynoDataTab
              key={selectedVehicle.id}
              vehicle={selectedVehicle}
              dyno={dyno}
              readOnly={readOnly}
              onDynoChange={onDynoChange}
            />
          </div>
          <div hidden={tab !== 'audio'} data-testid="dyno-panel-audio" className="space-y-4">
            {readOnlyHint}
            <DynoAudioTab
              active={tab === 'audio'}
              vehicle={selectedVehicle}
              dyno={dyno}
              readOnly={readOnly}
              audio={audio}
              onAudioChange={setAudio}
              onDynoChange={onDynoChange}
            />
          </div>
          <div hidden={tab !== 'runtime'} data-testid="dyno-panel-runtime" className="space-y-4">
            {readOnlyHint}
            <DynoRuntimeTab
              active={tab === 'runtime'}
              vehicle={selectedVehicle}
              dyno={dyno}
              readOnly={readOnly}
              onAudioChange={setAudio}
              onDynoChange={onDynoChange}
            />
          </div>
          <div
            hidden={tab !== 'diagnosis'}
            data-testid="dyno-panel-diagnosis"
            className="space-y-4"
          >
            {readOnlyHint}
            <DynoDiagnosisTab
              key={`${selectedVehicle.id}:${dyno?.id ?? 'none'}:${dyno?.environmentId ?? ''}`}
              active={tab === 'diagnosis'}
              vehicle={selectedVehicle}
              dyno={dyno}
              readOnly={readOnly}
              onDynoChange={onDynoChange}
            />
          </div>
        </>
      )}
    </ModulePage>
  );
}
