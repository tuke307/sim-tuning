'use client';

import React, { useId } from 'react';
import type { auslass } from '@sim-tuning/common/logic';
import { getAuspuffViewBox } from '@/lib/sim/auslass/auslass-helpers';
import { useLocale } from '@/providers/i18n/locale-context';

type Geometry = ReturnType<typeof auslass.getAuspuffGeometry>;

const FRAME_COLOR = '#dc2626';
const DIMENSION_COLOR = '#000000';
const FONT_SIZE = 28;

/**
 * Vector port of the v2 SkiaSharp expansion chamber bitmap: red framework, black dimension lines
 * with double arrows, dimension codes and translated part names. Scales with its container.
 */
export function AuspuffDrawing({ geometry }: { geometry: Geometry }) {
  const { t } = useLocale();
  // React ids contain characters (`:`, `«»`) that are awkward inside `url(#…)` references.
  const uid = `auspuff${useId().replace(/[^a-zA-Z0-9_-]/g, '')}`;
  const titleId = `${uid}-title`;
  const descId = `${uid}-desc`;
  const arrowId = `${uid}-arrow`;
  const vb = getAuspuffViewBox(geometry);

  return (
    <div className="w-full overflow-hidden rounded-2xl border bg-white">
      <svg
        role="img"
        aria-labelledby={`${titleId} ${descId}`}
        viewBox={`${vb.x} ${vb.y} ${vb.width} ${vb.height}`}
        preserveAspectRatio="xMidYMid meet"
        className="block h-auto w-full"
        data-testid="auspuff-drawing"
      >
        <title id={titleId}>{t.auslass.anwendung.drawingTitle}</title>
        <desc id={descId}>{t.auslass.anwendung.drawingDescription}</desc>
        <defs>
          <marker
            id={arrowId}
            viewBox="0 0 10 10"
            refX="10"
            refY="5"
            markerWidth="12"
            markerHeight="12"
            markerUnits="userSpaceOnUse"
            orient="auto-start-reverse"
          >
            <path d="M0,0 L10,5 L0,10 z" fill={DIMENSION_COLOR} />
          </marker>
        </defs>
        <g fill="none" strokeLinecap="round">
          {geometry.rects.map((r, i) => (
            <rect
              key={`r${i}`}
              x={r.x}
              y={r.y}
              width={Math.max(0, r.width)}
              height={Math.max(0, r.height)}
              stroke={FRAME_COLOR}
              strokeWidth={3}
              data-kind="frame"
            />
          ))}
          {geometry.segments.map((s, i) => (
            <line
              key={`s${i}`}
              x1={s.x1}
              y1={s.y1}
              x2={s.x2}
              y2={s.y2}
              stroke={s.kind === 'frame' ? FRAME_COLOR : DIMENSION_COLOR}
              strokeWidth={s.kind === 'frame' ? 3 : 1.5}
              markerStart={s.kind === 'dimension' ? `url(#${arrowId})` : undefined}
              markerEnd={s.kind === 'dimension' ? `url(#${arrowId})` : undefined}
              data-kind={s.kind}
            />
          ))}
        </g>
        <g fill={DIMENSION_COLOR} fontSize={FONT_SIZE} fontFamily="system-ui, sans-serif">
          {geometry.valueLabels.map((l, i) => (
            <text key={`v${i}`} x={l.x} y={l.y} data-testid={`auspuff-value-${l.text}`}>
              {l.text}
            </text>
          ))}
          {geometry.partLabels.map((l) => (
            <text key={l.key} x={l.x} y={l.y} data-testid={`auspuff-part-${l.key}`}>
              {t.auslass.parts[l.key]}
            </text>
          ))}
        </g>
      </svg>
    </div>
  );
}
