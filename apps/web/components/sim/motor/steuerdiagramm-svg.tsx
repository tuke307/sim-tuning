'use client';

import React, { useId } from 'react';
import {
  getSteuerdiagramm,
  type SteuerdiagrammGeometry,
  type SteuerdiagrammPort,
} from '@sim-tuning/common/logic';

/** Port colours as in v2: Einlass red, Auslass blue, Überströmer green. */
export const PORT_COLOR_CLASSES: Record<SteuerdiagrammPort, { stroke: string; bg: string }> = {
  einlass: { stroke: 'stroke-red-500', bg: 'bg-red-500' },
  auslass: { stroke: 'stroke-blue-500', bg: 'bg-blue-500' },
  ueberstroemer: { stroke: 'stroke-green-500', bg: 'bg-green-500' },
};

const EMPTY_GEOMETRY: SteuerdiagrammGeometry = { ...getSteuerdiagramm(0, 0, 0), lines: [] };

/**
 * Renders the `getSteuerdiagramm` geometry (v2 SkiaSharp bitmap replacement) as an accessible,
 * theme-aware SVG. Keeps a fixed square aspect ratio so results never shift the layout.
 */
export function SteuerdiagrammSvg({
  geometry,
  title,
  description,
  labels,
}: {
  geometry: SteuerdiagrammGeometry | null;
  title: string;
  description: string;
  labels: { OT: string; UT: string };
}) {
  const titleId = useId();
  const descId = useId();
  const g = geometry ?? EMPTY_GEOMETRY;
  const margin = g.center - g.radius;

  return (
    <svg
      viewBox={`0 0 ${g.size} ${g.size}`}
      role="img"
      aria-labelledby={`${titleId} ${descId}`}
      data-testid="motor-steuerdiagramm-svg"
      className="mx-auto aspect-square w-full max-w-[26rem]"
    >
      <title id={titleId}>{title}</title>
      <desc id={descId}>{description}</desc>
      <circle
        cx={g.center}
        cy={g.center}
        r={g.radius}
        className="stroke-foreground fill-none"
        strokeWidth={4}
      />
      <g className="stroke-muted-foreground" strokeWidth={2} strokeDasharray="12 10">
        <line x1={g.center} y1={margin} x2={g.center} y2={g.size - margin} />
        <line x1={margin} y1={g.center} x2={g.size - margin} y2={g.center} />
      </g>
      {g.lines.map((line) => (
        <line
          key={`${line.port}-${line.edge}`}
          x1={line.x1}
          y1={line.y1}
          x2={line.x2}
          y2={line.y2}
          strokeWidth={6}
          strokeLinecap="round"
          className={PORT_COLOR_CLASSES[line.port].stroke}
          data-testid={`motor-steuerdiagramm-line-${line.port}-${line.edge}`}
        />
      ))}
      <circle cx={g.center} cy={g.center} r={7} className="fill-foreground" />
      {g.labels.map((label) => (
        <text
          key={label.text}
          x={label.x}
          y={label.y}
          className="fill-foreground font-semibold"
          fontSize={28}
        >
          {labels[label.text]}
        </text>
      ))}
    </svg>
  );
}
