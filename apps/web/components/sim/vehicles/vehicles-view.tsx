'use client';

import React from 'react';
import { ModulePage } from '@/components/sim/shared/module-page';
import { useLocale } from '@/providers/i18n/locale-context';

export function VehiclesView() {
  const { t } = useLocale();
  return (
    <ModulePage title={t.vehicles.title} subtitle={t.vehicles.subtitle} showVehicle={false}>
      <p className="text-muted-foreground text-sm">{t.home.comingSoon}</p>
    </ModulePage>
  );
}
