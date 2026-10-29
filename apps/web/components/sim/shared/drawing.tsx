'use client';

import React, { createContext, useContext, useMemo, useState, type ReactNode } from 'react';
import { cn } from '@sim-tuning/ui/lib/utils';
import { useLocale } from '@/providers/i18n/locale-context';

/*
 * Signature parts of the Werkstattzeichnung (DESIGN.md): numbered callouts (Positionsnummern)
 * placed on a drawing and repeated in a Stückliste, plus dimension lines (Maßlinien).
 */

interface PositionHighlight {
  active: number | null;
  setActive: (pos: number | null) => void;
}

const PositionHighlightContext = createContext<PositionHighlight | null>(null);

/** Shares the highlighted position between a drawing and its Stückliste. */
export function PositionHighlightProvider({ children }: { children: ReactNode }) {
  const [active, setActive] = useState<number | null>(null);
  const value = useMemo(() => ({ active, setActive }), [active]);
  return (
    <PositionHighlightContext.Provider value={value}>{children}</PositionHighlightContext.Provider>
  );
}

function usePositionHighlight(): PositionHighlight {
  return useContext(PositionHighlightContext) ?? { active: null, setActive: () => {} };
}

/** Circled monospace position number for HTML contexts (Stückliste, legends). */
export function Callout({
  n,
  active,
  className,
}: {
  n: number;
  active?: boolean;
  className?: string;
}) {
  return (
    <span
      className={cn(
        'font-data inline-flex size-6 shrink-0 items-center justify-center rounded-full border text-[0.75rem] leading-none',
        active ? 'border-primary bg-primary text-primary-foreground' : 'border-foreground/70',
        className,
      )}
    >
      {n}
    </span>
  );
}

/**
 * Circled position number placed on an SVG drawing. Decorative for assistive tech: the linked
 * Stückliste row carries the same information as text.
 */
export function SvgCallout({
  n,
  x,
  y,
  r = 14,
  fontSize,
}: {
  n: number;
  x: number;
  y: number;
  r?: number;
  fontSize?: number;
}) {
  const { active, setActive } = usePositionHighlight();
  const on = active === n;
  return (
    <g
      aria-hidden
      data-callout={n}
      onPointerEnter={() => setActive(n)}
      onPointerLeave={() => setActive(null)}
      className="cursor-default"
    >
      <circle
        cx={x}
        cy={y}
        r={r}
        strokeWidth={r / 9}
        className={on ? 'fill-primary stroke-primary' : 'fill-card stroke-foreground'}
      />
      <text
        x={x}
        y={y}
        textAnchor="middle"
        dominantBaseline="central"
        fontSize={fontSize ?? r * 1.15}
        className={cn('font-data', on ? 'fill-primary-foreground' : 'fill-foreground')}
      >
        {n}
      </text>
    </g>
  );
}

/** Arrow head for dimension lines; render once inside `<defs>`. */
export function DimensionMarker({ id, size = 12 }: { id: string; size?: number }) {
  return (
    <marker
      id={id}
      viewBox="0 0 10 10"
      refX="10"
      refY="5"
      markerWidth={size}
      markerHeight={size}
      markerUnits="userSpaceOnUse"
      orient="auto-start-reverse"
    >
      <path d="M0,1.5 L10,5 L0,8.5 z" className="fill-primary" />
    </marker>
  );
}

/**
 * Dimension line in signal colour with arrow heads at both ends. Draws in once (200 ms,
 * stroke-dashoffset) when it mounts; `prefers-reduced-motion` shows it immediately.
 */
export function DimensionLine({
  x1,
  y1,
  x2,
  y2,
  markerId,
  strokeWidth = 1.5,
  ...rest
}: {
  x1: number;
  y1: number;
  x2: number;
  y2: number;
  markerId: string;
  strokeWidth?: number;
} & Omit<React.SVGProps<SVGLineElement>, 'x1' | 'y1' | 'x2' | 'y2'>) {
  return (
    <line
      x1={x1}
      y1={y1}
      x2={x2}
      y2={y2}
      pathLength={1}
      strokeWidth={strokeWidth}
      markerStart={`url(#${markerId})`}
      markerEnd={`url(#${markerId})`}
      data-kind="dimension"
      className="stroke-primary dimension-draw"
      {...rest}
    />
  );
}

/** Curved dimension (e.g. an angle arc) with the same arrows and draw-in as `DimensionLine`. */
export function DimensionPath({
  d,
  markerId,
  strokeWidth = 1.5,
  ...rest
}: {
  d: string;
  markerId: string;
  strokeWidth?: number;
} & Omit<React.SVGProps<SVGPathElement>, 'd'>) {
  return (
    <path
      d={d}
      fill="none"
      pathLength={1}
      strokeWidth={strokeWidth}
      markerStart={`url(#${markerId})`}
      markerEnd={`url(#${markerId})`}
      data-kind="dimension"
      className="stroke-primary dimension-draw"
      {...rest}
    />
  );
}

/** Line sample for legends: module colour plus dash pattern so colour is never the only cue. */
export function StrokeSample({
  className,
  dash,
}: {
  /** Tailwind `stroke-*` class. */
  className: string;
  dash?: string;
}) {
  return (
    <svg aria-hidden viewBox="0 0 28 6" className="h-1.5 w-7 shrink-0 overflow-visible">
      <line
        x1={1}
        y1={3}
        x2={27}
        y2={3}
        strokeWidth={2.5}
        strokeDasharray={dash}
        className={className}
      />
    </svg>
  );
}

export interface StuecklisteRow {
  key: string;
  /** Position number; shown as a callout and linked to the drawing. */
  pos?: number;
  label: ReactNode;
  /** Short dimension code from the drawing (LK, D1 …). */
  code?: string;
  value: string;
  unit?: string;
  rowTestId?: string;
  valueTestId?: string;
  unitTestId?: string;
}

export interface StuecklisteGroup {
  key: string;
  testId?: string;
  rows: StuecklisteRow[];
}

/**
 * Parts-list table (Pos · Benennung · Wert · Einheit). Hovering or focusing a row highlights the
 * matching callout on the drawing and vice versa.
 */
export function Stueckliste({
  caption,
  rows,
  groups,
  testId,
  className,
}: {
  caption: string;
  rows?: StuecklisteRow[];
  groups?: StuecklisteGroup[];
  testId?: string;
  className?: string;
}) {
  const { t } = useLocale();
  const { active, setActive } = usePositionHighlight();
  const allGroups = groups ?? [{ key: 'all', rows: rows ?? [] }];
  const th =
    'font-display text-muted-foreground h-9 px-2 text-left align-middle text-[0.75rem] font-semibold tracking-[0.06em] uppercase';

  return (
    <div className={cn('overflow-x-auto', className)}>
      <table data-testid={testId} className="w-full min-w-[18rem] border-collapse text-sm">
        <caption className="sr-only">{caption}</caption>
        <thead>
          <tr className="border-b">
            <th scope="col" className={cn(th, 'w-12 pl-0')}>
              {t.sim.pos}
            </th>
            <th scope="col" className={th}>
              {t.sim.designation}
            </th>
            <th scope="col" className={cn(th, 'text-right')}>
              {t.sim.value}
            </th>
            <th scope="col" className={cn(th, 'w-16 pr-0')}>
              {t.sim.unit}
            </th>
          </tr>
        </thead>
        {allGroups.map((group) => (
          <tbody key={group.key} data-testid={group.testId} className="border-b last:border-b-0">
            {group.rows.map((row) => {
              const on = row.pos !== undefined && active === row.pos;
              const link =
                row.pos !== undefined
                  ? {
                      tabIndex: 0,
                      onPointerEnter: () => setActive(row.pos!),
                      onPointerLeave: () => setActive(null),
                      onFocus: () => setActive(row.pos!),
                      onBlur: () => setActive(null),
                    }
                  : {};
              return (
                <tr
                  key={row.key}
                  data-testid={row.rowTestId}
                  data-pos={row.pos}
                  className={cn(
                    'outline-none focus-visible:outline-2 focus-visible:-outline-offset-2 focus-visible:outline-ring',
                    on && 'bg-raised',
                  )}
                  {...link}
                >
                  <td className="h-11 py-1 pr-2 align-middle">
                    {row.pos !== undefined ? <Callout n={row.pos} active={on} /> : null}
                  </td>
                  <th scope="row" className="px-2 py-1 text-left align-middle font-normal">
                    {row.label}
                    {row.code ? (
                      <span className="font-data text-muted-foreground ml-2 text-[0.75rem]">
                        {row.code}
                      </span>
                    ) : null}
                  </th>
                  <td
                    className="font-data px-2 py-1 text-right align-middle whitespace-nowrap"
                    data-testid={row.valueTestId}
                  >
                    {row.value}
                  </td>
                  <td
                    className="text-muted-foreground unit-caps py-1 pr-0 pl-1 align-middle whitespace-nowrap"
                    data-testid={row.unitTestId}
                  >
                    {row.unit ?? ''}
                  </td>
                </tr>
              );
            })}
          </tbody>
        ))}
      </table>
    </div>
  );
}
