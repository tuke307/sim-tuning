'use client';

import React from 'react';
import { AlertTriangle, CheckCircle2, Info } from 'lucide-react';
import { cn } from '@sim-tuning/ui/lib/utils';

export interface NoticeMessage {
  kind: 'success' | 'error' | 'info';
  text: string;
}

/** Inline status line for dyno actions (announced to screen readers). */
export function DynoNotice({
  message,
  testId,
  className,
}: {
  message: NoticeMessage | null;
  testId?: string;
  className?: string;
}) {
  if (!message) return null;
  const Icon =
    message.kind === 'success' ? CheckCircle2 : message.kind === 'error' ? AlertTriangle : Info;
  return (
    <p
      role={message.kind === 'error' ? 'alert' : 'status'}
      data-testid={testId}
      className={cn(
        'flex items-start gap-2 rounded-2xl px-3 py-2 text-[0.8125rem]',
        message.kind === 'success' && 'bg-primary/12 text-primary',
        message.kind === 'error' && 'bg-destructive/12 text-destructive',
        message.kind === 'info' && 'bg-secondary text-foreground',
        className,
      )}
    >
      <Icon className="mt-0.5 h-4 w-4 shrink-0" />
      <span>{message.text}</span>
    </p>
  );
}
