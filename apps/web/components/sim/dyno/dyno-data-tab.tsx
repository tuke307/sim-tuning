'use client';

import React, { useId, useRef, useState } from 'react';
import { Download, Save, Trash2, Upload } from 'lucide-react';
import {
  DynoExportSchema,
  DynoInputSchema,
  type DynoDto,
  type VehicleDto,
} from '@sim-tuning/common/schemas';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { Input } from '@sim-tuning/ui/components/ui/input';
import { Label } from '@sim-tuning/ui/components/ui/label';
import { NativeSelect } from '@sim-tuning/ui/components/ui/native-select';
import { Textarea } from '@sim-tuning/ui/components/ui/textarea';
import { ConfirmDestructiveDialog } from '@/components/sim/confirm-destructive-dialog';
import { ResultValue } from '@/components/sim/shared/unit-number-field';
import { Section } from '@/components/sim/shared/module-page';
import { useLocale } from '@/providers/i18n/locale-context';
import { useSimData } from '@/providers/sim/sim-data-context';
import { downloadBlob, safeFileName } from '@/lib/sim/dyno/audio-input';
import { DynoNotice, type NoticeMessage } from './dyno-notice';

/** Dyno exports are small JSON files; anything larger than this is rejected before parsing. */
const MAX_IMPORT_BYTES = 20 * 1024 * 1024;

interface DynoDataTabProps {
  vehicle: VehicleDto;
  dyno: DynoDto | null;
  readOnly: boolean;
  onDynoChange: (dyno: DynoDto | null) => void;
}

/** v2 `DynoDataView`: create/edit the dyno (name, description, environment), export, import, delete. */
export function DynoDataTab({ vehicle, dyno, readOnly, onDynoChange }: DynoDataTabProps) {
  const { t } = useLocale();
  const { repository, environments, setVehicle, selectVehicle } = useSimData();
  const formId = useId();
  const importRef = useRef<HTMLInputElement>(null);
  const [name, setName] = useState(dyno?.name ?? '');
  const [beschreibung, setBeschreibung] = useState(dyno?.beschreibung ?? '');
  const [environmentId, setEnvironmentId] = useState(dyno?.environmentId ?? '');
  const [busy, setBusy] = useState(false);
  const [notice, setNotice] = useState<NoticeMessage | null>(null);
  const [deleteOpen, setDeleteOpen] = useState(false);

  const refreshVehicle = async () => {
    const fresh = await repository.getVehicle(vehicle.id);
    if (fresh) setVehicle(fresh);
  };

  const save = async () => {
    const parsed = DynoInputSchema.safeParse({
      name,
      beschreibung: beschreibung.trim() ? beschreibung : null,
      environmentId: environmentId || null,
    });
    if (!parsed.success) {
      setNotice({ kind: 'error', text: t.dyno.data.nameRequired });
      return;
    }
    setBusy(true);
    try {
      const created = !dyno;
      const saved = await repository.saveDyno(vehicle.id, parsed.data);
      onDynoChange(saved);
      if (created) await refreshVehicle();
      setNotice({ kind: 'success', text: t.dyno.data.saved });
    } catch {
      setNotice({ kind: 'error', text: t.sim.errorGeneric });
    } finally {
      setBusy(false);
    }
  };

  const exportJson = async () => {
    if (!dyno) return;
    try {
      const data = await repository.exportDyno(dyno.id);
      downloadBlob(
        new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' }),
        safeFileName(`${vehicle.name}-${dyno.name}`, 'json'),
      );
    } catch {
      setNotice({ kind: 'error', text: t.sim.errorGeneric });
    }
  };

  const importJson = async (file: File) => {
    setNotice(null);
    if (file.size > MAX_IMPORT_BYTES) {
      setNotice({ kind: 'error', text: t.dyno.data.importTooLarge });
      return;
    }
    let json: unknown;
    try {
      json = JSON.parse(await file.text());
    } catch {
      setNotice({ kind: 'error', text: t.dyno.data.importInvalid });
      return;
    }
    // Untrusted file content: validate against the contract before it reaches the repository.
    const parsed = DynoExportSchema.safeParse(json);
    if (!parsed.success) {
      setNotice({ kind: 'error', text: t.dyno.data.importInvalid });
      return;
    }
    setBusy(true);
    try {
      const imported = await repository.importDyno(parsed.data);
      setVehicle(imported);
      selectVehicle(imported.id);
      setNotice({ kind: 'success', text: t.dyno.data.imported });
    } catch {
      setNotice({ kind: 'error', text: t.sim.errorGeneric });
    } finally {
      setBusy(false);
    }
  };

  const remove = async () => {
    if (!dyno) return;
    try {
      await repository.deleteDyno(dyno.id);
      onDynoChange(null);
      await refreshVehicle();
      setName('');
      setBeschreibung('');
      setEnvironmentId('');
      setNotice({ kind: 'success', text: t.dyno.data.deleted });
    } catch {
      setNotice({ kind: 'error', text: t.sim.errorGeneric });
    }
  };

  const disabled = readOnly || busy;

  return (
    <div className="grid gap-4 lg:grid-cols-2">
      <Section title={dyno ? t.dyno.data.sectionTitle : t.dyno.data.createTitle}>
        <form
          className="space-y-3"
          onSubmit={(e) => {
            e.preventDefault();
            void save();
          }}
        >
          <div className="space-y-1.5">
            <Label htmlFor={`${formId}-name`} className="text-[0.8125rem] font-medium">
              {t.dyno.data.name}
            </Label>
            <Input
              id={`${formId}-name`}
              data-testid="dyno-name"
              value={name}
              maxLength={120}
              placeholder={t.dyno.data.namePlaceholder}
              onChange={(e) => setName(e.target.value)}
              disabled={disabled}
            />
          </div>
          <div className="space-y-1.5">
            <Label htmlFor={`${formId}-desc`} className="text-[0.8125rem] font-medium">
              {t.dyno.data.description}
            </Label>
            <Textarea
              id={`${formId}-desc`}
              data-testid="dyno-description"
              value={beschreibung}
              maxLength={2000}
              onChange={(e) => setBeschreibung(e.target.value)}
              disabled={disabled}
            />
          </div>
          <div className="space-y-1.5">
            <Label htmlFor={`${formId}-env`} className="text-[0.8125rem] font-medium">
              {t.dyno.data.environment}
            </Label>
            <NativeSelect
              id={`${formId}-env`}
              data-testid="dyno-environment"
              value={environmentId}
              onChange={(e) => setEnvironmentId(e.target.value)}
              disabled={disabled}
            >
              <option value="">{t.dyno.data.noEnvironment}</option>
              {environments.map((env) => (
                <option key={env.id} value={env.id}>
                  {env.name}
                </option>
              ))}
            </NativeSelect>
          </div>
          {readOnly && <p className="text-muted-foreground text-xs">{t.dyno.readOnlyDyno}</p>}
          <div className="flex flex-wrap gap-2">
            <Button type="submit" disabled={disabled} data-testid="dyno-save">
              <Save className="h-4 w-4" />
              {dyno ? t.dyno.data.save : t.dyno.data.create}
            </Button>
            {dyno && !readOnly && (
              <Button
                type="button"
                variant="destructive"
                onClick={() => setDeleteOpen(true)}
                data-testid="dyno-delete"
              >
                <Trash2 className="h-4 w-4" />
                {t.dyno.data.delete}
              </Button>
            )}
          </div>
          <DynoNotice message={notice} testId="dyno-data-notice" />
        </form>
      </Section>

      <div className="space-y-4">
        {dyno && (
          <Section title={t.dyno.data.storedData}>
            <div className="grid grid-cols-2 gap-2">
              <ResultValue
                label={t.dyno.data.rpmPoints}
                value={dyno.drehzahl.length}
                testId="dyno-count-drehzahl"
              />
              <ResultValue
                label={t.dyno.data.powerPoints}
                value={dyno.dynoPs.length}
                testId="dyno-count-ps"
              />
              <ResultValue
                label={t.dyno.data.accelerationSamples}
                value={dyno.geschwindigkeit.length}
              />
              <ResultValue label={t.dyno.data.rolloutSamples} value={dyno.ausrollen.length} />
            </div>
          </Section>
        )}
        <Section title={t.dyno.data.transferTitle}>
          <p className="text-muted-foreground text-[0.8125rem]">{t.dyno.data.transferHint}</p>
          <div className="flex flex-wrap gap-2">
            <Button
              type="button"
              variant="secondary"
              disabled={!dyno}
              onClick={() => void exportJson()}
              data-testid="dyno-export"
            >
              <Download className="h-4 w-4" />
              {t.dyno.data.export}
            </Button>
            <Button
              type="button"
              variant="secondary"
              disabled={busy}
              onClick={() => importRef.current?.click()}
              data-testid="dyno-import"
            >
              <Upload className="h-4 w-4" />
              {t.dyno.data.import}
            </Button>
            <input
              ref={importRef}
              type="file"
              accept="application/json,.json"
              className="hidden"
              data-testid="dyno-import-input"
              onChange={(e) => {
                const file = e.target.files?.[0];
                e.target.value = '';
                if (file) void importJson(file);
              }}
            />
          </div>
        </Section>
      </div>

      <ConfirmDestructiveDialog
        open={deleteOpen}
        onOpenChange={setDeleteOpen}
        title={t.dyno.data.deleteTitle}
        description={t.dyno.data.deleteDescription}
        confirmLabel={t.common.delete}
        onConfirm={remove}
        testId="dyno-delete-dialog"
      />
    </div>
  );
}
