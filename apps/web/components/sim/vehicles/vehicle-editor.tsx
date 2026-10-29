'use client';

import React, { useEffect, useId, useRef, useState } from 'react';
import { Copy, RotateCcw, Save, Trash2 } from 'lucide-react';
import type { VehicleDto } from '@sim-tuning/common/schemas';
import { FIELD_BASE_UNITS } from '@sim-tuning/common/units';
import {
  AlertDialog,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@sim-tuning/ui/components/ui/alert-dialog';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { Input } from '@sim-tuning/ui/components/ui/input';
import { Label } from '@sim-tuning/ui/components/ui/label';
import { Textarea } from '@sim-tuning/ui/components/ui/textarea';
import { ConfirmDestructiveDialog } from '@/components/sim/confirm-destructive-dialog';
import { ReadOnlyBanner, Section } from '@/components/sim/shared/module-page';
import { UnitNumberField } from '@/components/sim/shared/unit-number-field';
import { confirmLeave, setLeaveGuard } from '@/lib/navigation/leave-guard';
import {
  AUSLASS_FIELDS,
  draftToInput,
  EINLASS_FIELDS,
  interpolate,
  isDraftDirty,
  MOTOR_FIELDS,
  setDraftField,
  UEBERSTROEMER_FIELDS,
  validateDraft,
  vehicleToDraft,
  type DraftSection,
  type FieldSpec,
  type VehicleDraft,
} from '@/lib/sim/vehicles/draft';
import { useLocale } from '@/providers/i18n/locale-context';
import { useSimData } from '@/providers/sim/sim-data-context';

function TextField({
  label,
  value,
  onChange,
  readOnly,
  multiline,
  testId,
  required,
}: {
  label: string;
  value: string;
  onChange: (value: string) => void;
  readOnly: boolean;
  multiline?: boolean;
  testId: string;
  required?: boolean;
}) {
  const id = useId();
  return (
    <div className="space-y-1.5">
      <Label htmlFor={id} className="text-[0.8125rem] font-medium">
        {label}
      </Label>
      {multiline ? (
        <Textarea
          id={id}
          value={value}
          readOnly={readOnly}
          maxLength={2000}
          onChange={(e) => onChange(e.target.value)}
          data-testid={testId}
        />
      ) : (
        <Input
          id={id}
          value={value}
          readOnly={readOnly}
          required={required}
          maxLength={120}
          aria-invalid={required && !value.trim() ? true : undefined}
          onChange={(e) => onChange(e.target.value)}
          data-testid={testId}
        />
      )}
    </div>
  );
}

/** Edit form for the selected vehicle (v2 VehiclesView "Daten" + "Motor" tabs). */
export function VehicleEditor({ vehicle }: { vehicle: VehicleDto }) {
  const { t } = useLocale();
  const { repository, setVehicle, selectVehicle, refresh } = useSimData();
  const [draft, setDraft] = useState<VehicleDraft>(() => vehicleToDraft(vehicle));
  const [status, setStatus] = useState<'idle' | 'saving' | 'saved' | 'error'>('idle');
  const [deleteOpen, setDeleteOpen] = useState(false);
  const [leaveOpen, setLeaveOpen] = useState(false);
  const leaveResolve = useRef<((ok: boolean) => void) | null>(null);
  const readOnly = vehicle.isSystem;
  const dirty = !readOnly && isDraftDirty(draft, vehicle);
  const error = validateDraft(draft);

  const finishLeave = (ok: boolean) => {
    setLeaveOpen(false);
    leaveResolve.current?.(ok);
    leaveResolve.current = null;
  };

  // While dirty, tab/module/vehicle switches ask "save / discard / cancel" first.
  useEffect(() => {
    if (!dirty) return;
    const unregister = setLeaveGuard(
      () =>
        new Promise<boolean>((resolve) => {
          leaveResolve.current?.(false);
          leaveResolve.current = resolve;
          setLeaveOpen(true);
        }),
    );
    const onBeforeUnload = (event: BeforeUnloadEvent) => {
      event.preventDefault();
      // Legacy browsers only show the prompt when returnValue is set.
      event.returnValue = '';
    };
    window.addEventListener('beforeunload', onBeforeUnload);
    return () => {
      unregister();
      window.removeEventListener('beforeunload', onBeforeUnload);
    };
  }, [dirty]);

  // A pending navigation must not hang when the editor goes away.
  useEffect(
    () => () => {
      leaveResolve.current?.(false);
      leaveResolve.current = null;
    },
    [],
  );

  const update = (next: VehicleDraft) => {
    setDraft(next);
    if (status !== 'saving') setStatus('idle');
  };

  const save = async (): Promise<boolean> => {
    if (readOnly || error) return false;
    setStatus('saving');
    try {
      const saved = await repository.updateVehicle(vehicle.id, draftToInput(draft));
      setVehicle(saved);
      setDraft(vehicleToDraft(saved));
      setStatus('saved');
      return true;
    } catch {
      setStatus('error');
      return false;
    }
  };

  const duplicate = async () => {
    // The copy is made from the stored vehicle, so unsaved edits are saved or discarded first.
    if (!(await confirmLeave())) return;
    try {
      const copy = await repository.duplicateVehicle(
        vehicle.id,
        `${vehicle.name} ${t.vehicles.copySuffix}`.slice(0, 120),
      );
      setVehicle(copy);
      selectVehicle(copy.id);
    } catch {
      setStatus('error');
    }
  };

  const remove = async () => {
    try {
      await repository.deleteVehicle(vehicle.id);
      selectVehicle(null);
      await refresh();
    } catch {
      setStatus('error');
    }
  };

  const renderFields = <S extends DraftSection>(
    section: S,
    fields: readonly FieldSpec<Extract<keyof VehicleDraft[S], string>>[],
    labels: Record<string, string>,
  ) => (
    <div className="grid gap-4 sm:grid-cols-2">
      {fields.map((field) => (
        <UnitNumberField
          key={field.key}
          label={labels[field.key] ?? field.key}
          value={(draft[section] as Record<string, number | null>)[field.key]}
          baseUnit={field.baseUnit}
          suffix={field.suffix}
          readOnly={readOnly}
          onChange={(value) => update(setDraftField(draft, section, field.key, value))}
          testId={`vehicle-field-${section}-${field.key}`}
        />
      ))}
    </div>
  );

  const statusText =
    status === 'saving'
      ? t.vehicles.saving
      : status === 'saved'
        ? t.vehicles.saved
        : status === 'error'
          ? t.vehicles.errorSave
          : error && !readOnly
            ? t.vehicles.nameRequired
            : dirty
              ? t.vehicles.unsaved
              : '';

  return (
    <form
      className="space-y-4"
      aria-label={vehicle.name}
      onSubmit={(e) => {
        e.preventDefault();
        void save();
      }}
    >
      <ReadOnlyBanner />
      <div className="bg-card flex flex-wrap items-center gap-2 rounded-md border p-3">
        {!readOnly && (
          <>
            <Button type="submit" size="sm" disabled={!dirty || !!error || status === 'saving'}>
              <Save className="h-4 w-4" />
              {t.vehicles.save}
            </Button>
            <Button
              type="button"
              size="sm"
              variant="ghost"
              disabled={!dirty}
              onClick={() => update(vehicleToDraft(vehicle))}
            >
              <RotateCcw className="h-4 w-4" />
              {t.vehicles.discard}
            </Button>
          </>
        )}
        <Button
          type="button"
          size="sm"
          variant="secondary"
          onClick={() => void duplicate()}
          data-testid="vehicle-duplicate"
        >
          <Copy className="h-4 w-4" />
          {t.vehicles.duplicate}
        </Button>
        {!readOnly && (
          <Button
            type="button"
            size="sm"
            variant="destructive"
            onClick={() => setDeleteOpen(true)}
            data-testid="vehicle-delete"
          >
            <Trash2 className="h-4 w-4" />
            {t.vehicles.delete}
          </Button>
        )}
        <p
          role="status"
          aria-live="polite"
          data-testid="vehicle-save-status"
          className={
            status === 'error' || (error && !readOnly)
              ? 'text-destructive ml-auto min-h-5 text-xs'
              : 'text-muted-foreground ml-auto min-h-5 text-xs'
          }
        >
          {statusText}
        </p>
      </div>

      <Section title={t.vehicles.sectionGeneral}>
        <div className="grid gap-4 sm:grid-cols-2">
          <TextField
            label={t.vehicles.fields.name}
            value={draft.name}
            readOnly={readOnly}
            required
            onChange={(name) => update({ ...draft, name })}
            testId="vehicle-field-name"
          />
          <TextField
            label={t.vehicles.fields.motorName}
            value={draft.motorName}
            readOnly={readOnly}
            required
            onChange={(motorName) => update({ ...draft, motorName })}
            testId="vehicle-field-motor-name"
          />
          <UnitNumberField
            label={t.vehicles.fields.gewicht}
            value={draft.gewicht}
            baseUnit={FIELD_BASE_UNITS.vehicle.gewicht}
            readOnly={readOnly}
            onChange={(gewicht) => update({ ...draft, gewicht })}
            testId="vehicle-field-gewicht"
          />
          <div className="sm:col-span-2">
            <TextField
              label={t.vehicles.fields.beschreibung}
              value={draft.beschreibung}
              readOnly={readOnly}
              multiline
              onChange={(beschreibung) => update({ ...draft, beschreibung })}
              testId="vehicle-field-beschreibung"
            />
          </div>
        </div>
      </Section>

      <Section title={t.vehicles.sectionMotor}>
        {renderFields('motor', MOTOR_FIELDS, t.vehicles.motorFields)}
      </Section>
      <Section title={t.vehicles.sectionEinlass}>
        {renderFields('einlass', EINLASS_FIELDS, t.vehicles.portFields)}
      </Section>
      <Section title={t.vehicles.sectionAuslass}>
        {renderFields('auslass', AUSLASS_FIELDS, t.vehicles.portFields)}
      </Section>
      <Section title={t.vehicles.sectionUeberstroemer}>
        {renderFields('ueberstroemer', UEBERSTROEMER_FIELDS, t.vehicles.portFields)}
      </Section>

      <AlertDialog
        open={leaveOpen}
        onOpenChange={(open) => {
          if (!open) finishLeave(false);
        }}
      >
        <AlertDialogContent data-testid="vehicle-leave-dialog">
          <AlertDialogHeader className="text-left">
            <AlertDialogTitle>{t.vehicles.leaveTitle}</AlertDialogTitle>
            <AlertDialogDescription className="text-[0.9375rem]">
              {interpolate(t.vehicles.leaveDescription, { name: vehicle.name })}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel data-testid="vehicle-leave-cancel">
              {t.common.cancel}
            </AlertDialogCancel>
            <Button
              type="button"
              variant="secondary"
              onClick={() => {
                update(vehicleToDraft(vehicle));
                finishLeave(true);
              }}
              data-testid="vehicle-leave-discard"
            >
              {t.vehicles.leaveDiscard}
            </Button>
            <Button
              type="button"
              disabled={!!error || status === 'saving'}
              onClick={async () => {
                finishLeave(await save());
              }}
              data-testid="vehicle-leave-save"
            >
              <Save className="h-4 w-4" />
              {t.vehicles.leaveSave}
            </Button>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      <ConfirmDestructiveDialog
        open={deleteOpen}
        onOpenChange={setDeleteOpen}
        title={t.vehicles.deleteTitle}
        description={interpolate(t.vehicles.deleteDescription, { name: vehicle.name })}
        confirmLabel={t.vehicles.delete}
        onConfirm={remove}
        testId="vehicle-delete-dialog"
      />
    </form>
  );
}
