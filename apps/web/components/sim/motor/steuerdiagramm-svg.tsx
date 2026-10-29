'use client';

import React, { useId } from 'react';
import {
  getSteuerdiagramm,
  type SteuerdiagrammGeometry,
  type SteuerdiagrammLine,
  type SteuerdiagrammPort,
} from '@sim-tuning/common/logic';
import { DimensionMarker, DimensionPath, SvgCallout } from '@/components/sim/shared/drawing';

/**
 * Port line styles: module colour plus a dash pattern so colour is never the only cue
 * (Einlass solid, Auslass long dash, Überströmer dot-dash).
 */
export const PORT_STYLES: Record<SteuerdiagrammPort, { stroke: string; dash?: string }> = {
  einlass: { stroke: 'stroke-mod-einlass' },
  auslass: { stroke: 'stroke-mod-auslass', dash: '22 10' },
  ueberstroemer: { stroke: 'stroke-mod-ueberstroemer', dash: '18 7 4 7' },
};

/** Dash patterns scaled down for legend samples (28 px wide). */
export const PORT_SAMPLE_DASH: Record<SteuerdiagrammPort, string | undefined> = {
  einlass: undefined,
  auslass: '7 3',
  ueberstroemer: '6 2.5 1.5 2.5',
};

export type EdgeKey = `${SteuerdiagrammPort}-${SteuerdiagrammLine['edge']}`;

const EMPTY_GEOMETRY: SteuerdiagrammGeometry = { ...getSteuerdiagramm(0, 0, 0), lines: [] };

/** Point at `factor` × the line length from the crank centre along a port line. */
function along(g: SteuerdiagrammGeometry, line: SteuerdiagrammLine, factor: number) {
  return {
    x: g.center + (line.x2 - g.center) * factor,
    y: g.center + (line.y2 - g.center) * factor,
  };
}

/** Short arc between two port lines at radius `r` (used for the blowdown angle). */
function arcBetween(
  g: SteuerdiagrammGeometry,
  a: SteuerdiagrammLine,
  b: SteuerdiagrammLine,
  r: number,
): { d: string; label: { x: number; y: number } } | null {
  const angle = (l: SteuerdiagrammLine) => Math.atan2(l.y2 - g.center, l.x2 - g.center);
  const a1 = angle(a);
  const a2 = angle(b);
  let delta = a2 - a1;
  while (delta > Math.PI) delta -= 2 * Math.PI;
  while (delta < -Math.PI) delta += 2 * Math.PI;
  if (Math.abs(delta) < 0.02) return null;
  const p1 = { x: g.center + r * Math.cos(a1), y: g.center + r * Math.sin(a1) };
  const p2 = { x: g.center + r * Math.cos(a2), y: g.center + r * Math.sin(a2) };
  const sweep = delta > 0 ? 1 : 0;
  const mid = a1 + delta / 2;
  const labelR = r - 46;
  return {
    d: `M ${p1.x} ${p1.y} A ${r} ${r} 0 0 ${sweep} ${p2.x} ${p2.y}`,
    label: { x: g.center + labelR * Math.cos(mid), y: g.center + labelR * Math.sin(mid) },
  };
}

/**
 * Renders the `getSteuerdiagramm` geometry (v2 SkiaSharp bitmap replacement) as an accessible,
 * theme-aware drawing on the 5 mm grid. Opening/closing edges carry numbered callouts that link
 * to the Stückliste; the blowdown angle is dimensioned in signal colour.
 */
export function SteuerdiagrammSvg({
  geometry,
  title,
  description,
  labels,
  positions,
  blowdownLabel,
}: {
  geometry: SteuerdiagrammGeometry | null;
  title: string;
  description: string;
  labels: { OT: string; UT: string };
  positions?: Partial<Record<EdgeKey, number>>;
  /** Formatted blowdown angle written next to its dimension arc. */
  blowdownLabel?: string;
}) {
  const uid = `sd${useId().replace(/[^a-zA-Z0-9_-]/g, '')}`;
  const titleId = `${uid}-title`;
  const descId = `${uid}-desc`;
  const markerId = `${uid}-arrow`;
  const g = geometry ?? EMPTY_GEOMETRY;
  const margin = g.center - g.radius;
  const find = (port: SteuerdiagrammPort, edge: SteuerdiagrammLine['edge']) =>
    g.lines.find((l) => l.port === port && l.edge === edge);
  const auslassOpen = find('auslass', 'oeffnet');
  const ueberOpen = find('ueberstroemer', 'oeffnet');
  const blowdown =
    auslassOpen && ueberOpen ? arcBetween(g, auslassOpen, ueberOpen, g.radius * 0.72) : null;

  return (
    <div className="drawing-grid overflow-hidden rounded-md border">
      <svg
        viewBox={`0 0 ${g.size} ${g.size}`}
        role="img"
        aria-labelledby={`${titleId} ${descId}`}
        data-testid="motor-steuerdiagramm-svg"
        className="mx-auto block aspect-square w-full max-w-[28rem]"
      >
        <title id={titleId}>{title}</title>
        <desc id={descId}>{description}</desc>
        <defs>
          <DimensionMarker id={markerId} size={16} />
        </defs>
        <g className="stroke-muted-foreground" strokeWidth={1.5}>
          <line x1={g.center} y1={margin - 12} x2={g.center} y2={g.size - margin + 12} />
          <line x1={margin - 12} y1={g.center} x2={g.size - margin + 12} y2={g.center} />
        </g>
        <circle
          cx={g.center}
          cy={g.center}
          r={g.radius}
          className="stroke-foreground fill-none"
          strokeWidth={3}
        />
        {g.lines.map((line) => (
          <line
            key={`${line.port}-${line.edge}`}
            x1={line.x1}
            y1={line.y1}
            x2={line.x2}
            y2={line.y2}
            strokeWidth={5}
            strokeDasharray={PORT_STYLES[line.port].dash}
            className={PORT_STYLES[line.port].stroke}
            data-testid={`motor-steuerdiagramm-line-${line.port}-${line.edge}`}
          />
        ))}
        {blowdown && (
          <DimensionPath
            key={blowdown.d}
            d={blowdown.d}
            markerId={markerId}
            strokeWidth={2.5}
            data-testid="motor-steuerdiagramm-vorauslass-arc"
          />
        )}
        {blowdown && blowdownLabel && (
          <text
            x={blowdown.label.x}
            y={blowdown.label.y}
            textAnchor="middle"
            dominantBaseline="central"
            fontSize={24}
            paintOrder="stroke"
            strokeWidth={6}
            className="fill-primary stroke-card font-data"
          >
            {blowdownLabel}
          </text>
        )}
        <circle cx={g.center} cy={g.center} r={7} className="fill-foreground" />
        {g.labels.map((label) => (
          <text
            key={label.text}
            x={label.x}
            y={label.y}
            className="fill-foreground font-display font-semibold"
            fontSize={28}
            letterSpacing="0.06em"
          >
            {labels[label.text]}
          </text>
        ))}
        {g.lines.map((line) => {
          const n = positions?.[`${line.port}-${line.edge}`];
          if (n === undefined) return null;
          const p = along(g, line, 1.105);
          return <SvgCallout key={`c-${line.port}-${line.edge}`} n={n} x={p.x} y={p.y} r={20} />;
        })}
      </svg>
    </div>
  );
}
