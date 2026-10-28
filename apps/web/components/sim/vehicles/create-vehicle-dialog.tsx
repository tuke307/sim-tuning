'use client';

import React, { useId, useState } from 'react';
import { FIELD_BASE_UNITS } from '@sim-tuning/common/units';
import { Button } from '@sim-tuning/ui/components/ui/button';
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
import { Textarea } from '@sim-tuning/ui/components/ui/textarea';
import { UnitNumberField } from '@/components/sim/shared/unit-number-field';
import { newVehicleInput } from '@/lib/sim/vehicles/draft';
import { useLocale } from '@/providers/i18n/locale-context';
import { useSimData } from '@/providers/sim/sim-data-context';

/** v2 VehiclePopup / "Neues Fahrzeug": name, description, weight and engine name. */
export function CreateVehicleDialog({
  open,
  onOpenChange,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const { t } = useLocale();
  const { repository, setVehicle, selectVehicle } = useSimData();
  const ids = { name: useId(), beschreibung: useId(), motor: useId(), error: useId() };
  const [name, setName] = useState('');
  const [beschreibung, setBeschreibung] = useState('');
  const [gewicht, setGewicht] = useState<number | null>(null);
  const [motorName, setMotorName] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const reset = () => {
    setName('');
    setBeschreibung('');
    setGewicht(null);
    setMotorName('');
    setError(null);
  };

  const submit = async () => {
    if (!name.trim()) {
      setError(t.vehicles.nameRequired);
      return;
    }
    setBusy(true);
    try {
      const created = await repository.createVehicle(
        newVehicleInput({ name, beschreibung, gewicht, motorName }),
      );
      setVehicle(created);
      selectVehicle(created.id);
      reset();
      onOpenChange(false);
    } catch {
      setError(t.vehicles.errorSave);
    } finally {
      setBusy(false);
    }
  };

  return (
    <Dialog
      open={open}
      onOpenChange={(next) => {
        if (!next) reset();
        onOpenChange(next);
      }}
    >
      <DialogContent data-testid="vehicle-create-dialog">
        <form
          className="space-y-4"
          onSubmit={(e) => {
            e.preventDefault();
            void submit();
          }}
        >
          <DialogHeader className="text-left">
            <DialogTitle>{t.vehicles.createTitle}</DialogTitle>
            <DialogDescription>{t.vehicles.createDescription}</DialogDescription>
          </DialogHeader>
          <div className="space-y-1.5">
            <Label htmlFor={ids.name} className="text-[0.8125rem] font-medium">
              {t.vehicles.fields.name}
            </Label>
            <Input
              id={ids.name}
              value={name}
              required
              maxLength={120}
              aria-invalid={error ? true : undefined}
              aria-describedby={error ? ids.error : undefined}
              onChange={(e) => {
                setName(e.target.value);
                setError(null);
              }}
              data-testid="vehicle-create-name"
            />
          </div>
          <div className="space-y-1.5">
            <Label htmlFor={ids.motor} className="text-[0.8125rem] font-medium">
              {t.vehicles.fields.motorName}
            </Label>
            <Input
              id={ids.motor}
              value={motorName}
              maxLength={120}
              placeholder={name}
              onChange={(e) => setMotorName(e.target.value)}
              data-testid="vehicle-create-motor-name"
            />
          </div>
          <UnitNumberField
            label={t.vehicles.fields.gewicht}
            value={gewicht}
            baseUnit={FIELD_BASE_UNITS.vehicle.gewicht}
            onChange={setGewicht}
            testId="vehicle-create-gewicht"
          />
          <div className="space-y-1.5">
            <Label htmlFor={ids.beschreibung} className="text-[0.8125rem] font-medium">
              {t.vehicles.fields.beschreibung}
            </Label>
            <Textarea
              id={ids.beschreibung}
              value={beschreibung}
              maxLength={2000}
              onChange={(e) => setBeschreibung(e.target.value)}
              data-testid="vehicle-create-beschreibung"
            />
          </div>
          <p id={ids.error} role="alert" className="text-destructive min-h-4 text-xs">
            {error}
          </p>
          <DialogFooter>
            <Button
              type="button"
              variant="secondary"
              onClick={() => {
                reset();
                onOpenChange(false);
              }}
            >
              {t.common.cancel}
            </Button>
            <Button type="submit" disabled={busy} data-testid="vehicle-create-submit">
              {t.vehicles.create}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
