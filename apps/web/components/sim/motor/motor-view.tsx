'use client';

import React from 'react';
import { ModulePage } from '@/components/sim/shared/module-page';
import { useLocale } from '@/providers/i18n/locale-context';

export function MotorView() {
  const { t } = useLocale();
  return (
    <ModulePage title={t.motor.title} subtitle={t.motor.subtitle}>
      <p className="text-muted-foreground text-sm">{t.home.comingSoon}</p>
    </ModulePage>
  );
}
