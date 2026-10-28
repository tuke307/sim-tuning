'use client';

import React from 'react';
import { CheckCircle2, Lock, Plus } from 'lucide-react';
import type { VehicleDto } from '@sim-tuning/common/schemas';
import { Badge } from '@sim-tuning/ui/components/ui/badge';
import { Button } from '@sim-tuning/ui/components/ui/button';
import { Card } from '@sim-tuning/ui/components/ui/card';
import { cn } from '@sim-tuning/ui/lib/utils';
import { useLocale } from '@/providers/i18n/locale-context';

interface VehicleListProps {
  vehicles: VehicleDto[];
  selectedId: string | null;
  onSelect: (id: string) => void;
  onCreate: () => void;
}

/** v2 VehiclesView "Auswahl": own vehicles first, then starters; selection = active vehicle. */
export function VehicleList({ vehicles, selectedId, onSelect, onCreate }: VehicleListProps) {
  const { t } = useLocale();
  const sorted = [...vehicles].sort((a, b) => Number(a.isSystem) - Number(b.isSystem));
  return (
    <Card className="space-y-3 rounded-[1.25rem] p-4">
      <div className="flex items-center justify-between gap-2">
        <h2 className="text-[0.9375rem] font-semibold">{t.vehicles.listTitle}</h2>
        <Button type="button" size="sm" onClick={onCreate} data-testid="vehicle-create">
          <Plus className="h-4 w-4" />
          {t.vehicles.newVehicle}
        </Button>
      </div>
      <ul aria-label={t.vehicles.listAria} className="space-y-1.5" data-testid="vehicle-list">
        {sorted.map((vehicle) => {
          const selected = vehicle.id === selectedId;
          return (
            <li key={vehicle.id}>
              <button
                type="button"
                onClick={() => onSelect(vehicle.id)}
                aria-current={selected ? 'true' : undefined}
                data-testid={`vehicle-item-${vehicle.id}`}
                className={cn(
                  'focus-visible:ring-ring/50 flex w-full items-center gap-3 rounded-2xl border px-3.5 py-2.5 text-left transition-colors outline-none focus-visible:ring-[3px]',
                  selected
                    ? 'border-primary/50 bg-primary/10'
                    : 'hover:bg-secondary border-transparent',
                )}
              >
                <span className="min-w-0 flex-1">
                  <span className="block truncate text-sm font-semibold">{vehicle.name}</span>
                  <span className="text-muted-foreground block truncate text-xs">
                    {vehicle.motor?.name ?? t.vehicles.noEngine}
                  </span>
                </span>
                {vehicle.isSystem && (
                  <Badge variant="secondary" className="gap-1">
                    <Lock aria-hidden />
                    {t.sim.starterBadge}
                  </Badge>
                )}
                {selected && (
                  <CheckCircle2
                    className="text-primary h-4 w-4 shrink-0"
                    aria-label={t.vehicles.activeBadge}
                  />
                )}
              </button>
            </li>
          );
        })}
      </ul>
    </Card>
  );
}
