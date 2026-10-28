'use client';

import React, { useCallback, useEffect, useRef, useState } from 'react';
import { cn } from '@sim-tuning/ui/lib/utils';

export interface SwipeAction {
  id: string;
  label: string;
  icon: React.ComponentType<{ className?: string }>;
  onSelect: () => void;
  tone?: 'neutral' | 'accent' | 'destructive';
  testId?: string;
}

interface SwipeRowProps {
  children: React.ReactNode;
  /** Revealed when swiping right (shown on the leading edge), e.g. Edit / Duplicate. */
  leadingActions?: SwipeAction[];
  /** Revealed when swiping left (shown on the trailing edge), e.g. Delete. */
  trailingActions?: SwipeAction[];
  disabled?: boolean;
  className?: string;
  testId?: string;
}

const ACTION_WIDTH = 76;
const AXIS_LOCK_PX = 8;

const toneClass: Record<NonNullable<SwipeAction['tone']>, string> = {
  neutral: 'bg-[oklch(0.5_0.01_286)] text-white',
  accent: 'bg-primary text-primary-foreground',
  destructive: 'bg-destructive text-destructive-foreground',
};

/**
 * iOS-style swipe row: drag horizontally to reveal contextual actions. Vertical scrolling is
 * preserved via axis locking and `touch-action: pan-y`. Actions are removed from the tab order
 * while hidden; keyboard and pointer users get the same actions through a menu or edit mode.
 */
export function SwipeRow({
  children,
  leadingActions = [],
  trailingActions = [],
  disabled = false,
  className,
  testId,
}: SwipeRowProps) {
  const [offset, setOffset] = useState(0);
  const [dragging, setDragging] = useState(false);
  const rootRef = useRef<HTMLDivElement>(null);
  const gesture = useRef<{
    startX: number;
    startY: number;
    base: number;
    axis: 'x' | 'y' | null;
    pointerId: number;
  } | null>(null);
  const suppressClick = useRef(false);

  const leadingWidth = leadingActions.length * ACTION_WIDTH;
  const trailingWidth = trailingActions.length * ACTION_WIDTH;

  const close = useCallback(() => setOffset(0), []);

  // Close when tapping anywhere outside an open row.
  useEffect(() => {
    if (offset === 0) return;
    const onPointerDown = (e: PointerEvent) => {
      if (!rootRef.current?.contains(e.target as Node)) close();
    };
    document.addEventListener('pointerdown', onPointerDown);
    return () => document.removeEventListener('pointerdown', onPointerDown);
  }, [offset, close]);

  const onPointerDown = (e: React.PointerEvent) => {
    if (disabled || e.pointerType === 'mouse' || (!leadingWidth && !trailingWidth)) return;
    gesture.current = {
      startX: e.clientX,
      startY: e.clientY,
      base: offset,
      axis: null,
      pointerId: e.pointerId,
    };
  };

  const onPointerMove = (e: React.PointerEvent) => {
    const g = gesture.current;
    if (!g || g.pointerId !== e.pointerId) return;
    const dx = e.clientX - g.startX;
    const dy = e.clientY - g.startY;
    if (!g.axis) {
      if (Math.abs(dx) < AXIS_LOCK_PX && Math.abs(dy) < AXIS_LOCK_PX) return;
      g.axis = Math.abs(dx) > Math.abs(dy) ? 'x' : 'y';
      if (g.axis === 'x') {
        setDragging(true);
        (e.currentTarget as HTMLElement).setPointerCapture(e.pointerId);
      }
    }
    if (g.axis !== 'x') return;
    const next = g.base + dx;
    // Rubber-band beyond the available action width.
    const max = leadingWidth;
    const min = -trailingWidth;
    const clamped =
      next > max ? max + (next - max) * 0.2 : next < min ? min + (next - min) * 0.2 : next;
    setOffset(clamped);
  };

  const onPointerEnd = (e: React.PointerEvent) => {
    const g = gesture.current;
    gesture.current = null;
    if (!g || g.axis !== 'x') return;
    suppressClick.current = true;
    setDragging(false);
    const dx = e.clientX - g.startX;
    const target = g.base + dx;
    if (target > leadingWidth / 2 && leadingWidth > 0) setOffset(leadingWidth);
    else if (target < -trailingWidth / 2 && trailingWidth > 0) setOffset(-trailingWidth);
    else setOffset(0);
  };

  const onClickCapture = (e: React.MouseEvent) => {
    if (suppressClick.current) {
      suppressClick.current = false;
      e.preventDefault();
      e.stopPropagation();
      return;
    }
    if (offset !== 0) {
      e.preventDefault();
      e.stopPropagation();
      close();
    }
  };

  const renderActions = (actions: SwipeAction[], side: 'leading' | 'trailing') => {
    const isOpen = side === 'leading' ? offset > 0 : offset < 0;
    return (
      <div
        aria-hidden={!isOpen}
        className={cn(
          'absolute inset-y-0 flex',
          side === 'leading' ? 'left-0' : 'right-0',
          isOpen ? 'visible' : 'invisible',
        )}
      >
        {actions.map((action) => {
          const Icon = action.icon;
          return (
            <button
              key={action.id}
              type="button"
              tabIndex={isOpen ? 0 : -1}
              data-testid={action.testId}
              onClick={() => {
                close();
                action.onSelect();
              }}
              className={cn(
                'flex h-full flex-col items-center justify-center gap-1 text-[0.8125rem] font-medium',
                toneClass[action.tone ?? 'neutral'],
              )}
              style={{ width: ACTION_WIDTH }}
            >
              <Icon className="h-5 w-5" />
              {action.label}
            </button>
          );
        })}
      </div>
    );
  };

  return (
    <div
      ref={rootRef}
      data-testid={testId}
      className={cn('relative overflow-hidden', className)}
      onKeyDown={(e) => {
        if (e.key === 'Escape' && offset !== 0) close();
      }}
    >
      {leadingActions.length > 0 && renderActions(leadingActions, 'leading')}
      {trailingActions.length > 0 && renderActions(trailingActions, 'trailing')}
      <div
        onPointerDown={onPointerDown}
        onPointerMove={onPointerMove}
        onPointerUp={onPointerEnd}
        onPointerCancel={onPointerEnd}
        onClickCapture={onClickCapture}
        className={cn(
          'bg-card relative touch-pan-y',
          !dragging &&
            'transition-transform duration-300 ease-(--ease-out-quart) motion-reduce:transition-none',
        )}
        style={{ transform: `translate3d(${offset}px, 0, 0)` }}
      >
        {children}
      </div>
    </div>
  );
}
