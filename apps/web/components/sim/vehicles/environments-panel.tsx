'use client';

import React, { useId, useState } from 'react';
import { CloudSun, Lock, Pencil, Plus, Trash2 } from 'lucide-react';
import type { EnvironmentDto } from '@sim-tuning/common/schemas';
import { FIELD_BASE_UNITS, UNITS } from '@sim-tuning/common/units';
import { Badge } from '@sim-tuning/ui/components/ui/badge';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { Card } from '@sim-tuning/ui/components/ui/card';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@sim-tuning/ui/components/ui/dialog';
import { Input } from '@sim-tuning/ui/components/ui/input';
import { Label } from '@sim-tuning/ui/components/ui/label';
import { ConfirmDestructiveDialog } from '@/components/sim/confirm-destructive-dialog';
import { UnitNumberField } from '@/components/sim/shared/unit-number-field';
import { interpolate } from '@/lib/sim/vehicles/draft';
import { useLocale } from '@/providers/i18n/locale-context';
import { useSimData } from '@/providers/sim/sim-data-context';

type EditorState = { mode: 'create' } | { mode: 'edit'; environment: EnvironmentDto } | null;

function EnvironmentDialog({
  state,
  onClose,
}: {
  state: Exclude<EditorState, null>;
  onClose: () => void;
}) {
  const { t } = useLocale();
  const { repository, refresh } = useSimData();
  const initial = state.mode === 'edit' ? state.environment : null;
  const nameId = useId();
  const errorId = useId();
  const [name, setName] = useState(initial?.name ?? '');
  const [temperaturT, setTemperaturT] = useState<number | null>(initial?.temperaturT ?? null);
  const [luftdruckP, setLuftdruckP] = useState<number | null>(initial?.luftdruckP ?? null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const submit = async () => {
    const trimmed = name.trim();
    if (!trimmed) {
      setError(t.vehicles.nameRequired);
      return;
    }
    setBusy(true);
    try {
      const input = { name: trimmed, temperaturT, luftdruckP };
      if (initial) await repository.updateEnvironment(initial.id, input);
      else await repository.createEnvironment(input);
      await refresh();
      onClose();
    } catch {
      setError(t.vehicles.errorSave);
    } finally {
      setBusy(false);
    }
  };

  return (
    <Dialog open onOpenChange={(open) => !open && onClose()}>
      <DialogContent data-testid="environment-dialog">
        <form
          className="space-y-4"
          onSubmit={(e) => {
            e.preventDefault();
            void submit();
          }}
        >
          <DialogHeader className="text-left">
            <DialogTitle>
              {initial ? t.vehicles.editEnvironment : t.vehicles.newEnvironment}
            </DialogTitle>
            <DialogDescription>{t.vehicles.environmentsHint}</DialogDescription>
          </DialogHeader>
          <div className="space-y-1.5">
            <Label htmlFor={nameId} className="text-[0.8125rem] font-medium">
              {t.vehicles.environmentFields.name}
            </Label>
            <Input
              id={nameId}
              value={name}
              required
              maxLength={120}
              aria-invalid={error ? true : undefined}
              aria-describedby={error ? errorId : undefined}
              onChange={(e) => {
                setName(e.target.value);
                setError(null);
              }}
              data-testid="environment-name"
            />
          </div>
          <div className="grid gap-4 sm:grid-cols-2">
            <UnitNumberField
              label={t.vehicles.environmentFields.temperaturT}
              value={temperaturT}
              baseUnit={FIELD_BASE_UNITS.environment.temperaturT}
              onChange={setTemperaturT}
              testId="environment-temperatur"
            />
            <UnitNumberField
              label={t.vehicles.environmentFields.luftdruckP}
              value={luftdruckP}
              baseUnit={FIELD_BASE_UNITS.environment.luftdruckP}
              onChange={setLuftdruckP}
              testId="environment-luftdruck"
            />
          </div>
          <p id={errorId} role="alert" className="text-destructive min-h-4 text-xs">
            {error}
          </p>
          <DialogFooter>
            <Button type="button" variant="secondary" onClick={onClose}>
              {t.common.cancel}
            </Button>
            <Button type="submit" disabled={busy} data-testid="environment-submit">
              {t.vehicles.save}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

/** Environment (weather) management, v2 EnvironmentViewModel; starters are read-only. */
export function EnvironmentsPanel() {
  const { t } = useLocale();
  const { environments, repository, refresh, rounding } = useSimData();
  const [editor, setEditor] = useState<EditorState>(null);
  const [toDelete, setToDelete] = useState<EnvironmentDto | null>(null);
  const [panelError, setPanelError] = useState<string | null>(null);
  const { format } = new Intl.NumberFormat(undefined, {
    maximumFractionDigits: rounding.roundingAccuracy,
  });
  const sorted = [...environments].sort((a, b) => Number(a.isSystem) - Number(b.isSystem));
  const tUnit = UNITS[FIELD_BASE_UNITS.environment.temperaturT].abbreviation;
  const pUnit = UNITS[FIELD_BASE_UNITS.environment.luftdruckP].abbreviation;

  return (
    <Card className="space-y-4 rounded-md p-5">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-[0.9375rem] font-semibold">{t.vehicles.environmentsTitle}</h2>
          <p className="text-muted-foreground text-xs">{t.vehicles.environmentsHint}</p>
        </div>
        <Button
          type="button"
          size="sm"
          onClick={() => setEditor({ mode: 'create' })}
          data-testid="environment-create"
        >
          <Plus className="h-4 w-4" />
          {t.vehicles.newEnvironment}
        </Button>
      </div>
      {panelError && (
        <p role="alert" className="text-destructive text-xs">
          {panelError}
        </p>
      )}
      {sorted.length === 0 ? (
        <p className="text-muted-foreground text-sm">{t.vehicles.environmentsEmpty}</p>
      ) : (
        <ul className="grid gap-2 sm:grid-cols-2" data-testid="environment-list">
          {sorted.map((env) => (
            <li
              key={env.id}
              data-testid={`environment-item-${env.id}`}
              className="bg-raised flex items-center gap-3 rounded-md px-4 py-3"
            >
              <CloudSun className="text-primary h-5 w-5 shrink-0" aria-hidden />
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm font-semibold">{env.name}</p>
                <p className="text-muted-foreground text-xs font-data">
                  {env.temperaturT !== null ? `${format(env.temperaturT)} ${tUnit}` : '–'} ·{' '}
                  {env.luftdruckP !== null ? `${format(env.luftdruckP)} ${pUnit}` : '–'}
                </p>
              </div>
              {env.isSystem ? (
                <Badge variant="secondary" className="gap-1">
                  <Lock aria-hidden />
                  {t.sim.starterBadge}
                </Badge>
              ) : (
                <div className="flex shrink-0 gap-1">
                  <Button
                    type="button"
                    size="icon"
                    variant="ghost"
                    className="size-9"
                    aria-label={`${t.common.edit}: ${env.name}`}
                    onClick={() => setEditor({ mode: 'edit', environment: env })}
                  >
                    <Pencil className="h-4 w-4" />
                  </Button>
                  <Button
                    type="button"
                    size="icon"
                    variant="ghost"
                    className="hover:text-destructive size-9"
                    aria-label={`${t.common.delete}: ${env.name}`}
                    onClick={() => setToDelete(env)}
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
              )}
            </li>
          ))}
        </ul>
      )}
      {editor && (
        <EnvironmentDialog
          key={editor.mode === 'edit' ? editor.environment.id : 'new'}
          state={editor}
          onClose={() => setEditor(null)}
        />
      )}
      <ConfirmDestructiveDialog
        open={toDelete !== null}
        onOpenChange={(open) => !open && setToDelete(null)}
        title={t.vehicles.deleteEnvironmentTitle}
        description={interpolate(t.vehicles.deleteEnvironmentDescription, {
          name: toDelete?.name ?? '',
        })}
        confirmLabel={t.vehicles.delete}
        onConfirm={async () => {
          if (!toDelete) return;
          try {
            await repository.deleteEnvironment(toDelete.id);
            setPanelError(null);
            await refresh();
          } catch {
            setPanelError(t.vehicles.errorSave);
          }
        }}
        testId="environment-delete-dialog"
      />
    </Card>
  );
}
