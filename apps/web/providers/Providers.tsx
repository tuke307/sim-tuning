'use client';

import { ReactNode } from 'react';
import { ThemeProvider } from '@/providers/theme/theme-provider';
import { AuthProvider } from '@/providers/auth/auth-context';
import { LocaleProvider } from '@/providers/i18n/locale-context';
import { Toaster } from '@sim-tuning/ui/components/ui/sonner';
import { MotionConfig, useReducedMotion } from 'framer-motion';
import { ANIMATION_CONFIG, TRANSITIONS } from '../lib/animations/animation-config';

interface ProvidersProps {
  children: ReactNode;
}

export function Providers({ children }: ProvidersProps) {
  const shouldReduce = useReducedMotion();

  return (
    <ThemeProvider>
      <AuthProvider>
        <LocaleProvider>
          <MotionConfig
            reducedMotion={shouldReduce ? 'user' : 'never'}
            transition={shouldReduce ? ANIMATION_CONFIG.reducedMotion : TRANSITIONS.smooth}
          >
            {children}
          </MotionConfig>
          <Toaster />
        </LocaleProvider>
      </AuthProvider>
    </ThemeProvider>
  );
}
