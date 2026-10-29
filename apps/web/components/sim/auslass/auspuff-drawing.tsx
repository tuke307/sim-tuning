'use client';

import React, { useId } from 'react';
import type { auslass } from '@sim-tuning/common/logic';
import { DimensionLine, DimensionMarker, SvgCallout } from '@/components/sim/shared/drawing';
import { getAuspuffViewBox } from '@/lib/sim/auslass/auslass-helpers';
import { useLocale } from '@/providers/i18n/locale-context';

type Geometry = ReturnType<typeof auslass.getAuspuffGeometry>;

const FONT_SIZE = 30;
const PART_FONT_SIZE = 26;
const CALLOUT_R = 19;

/**
 * Vector port of the v2 SkiaSharp expansion chamber bitmap on the 5 mm grid: framework in the
 * Auslass colour, dimension lines with double arrows in signal colour, dimension codes, part
 * names and numbered callouts that link to the Stückliste (`positions`: code → position).
 */
export function AuspuffDrawing({
  geometry,
  positions = {},
}: {
  geometry: Geometry;
  positions?: Record<string, number>;
}) {
  const { t } = useLocale();
  // React ids contain characters (`:`, `«»`) that are awkward inside `url(#…)` references.
  const uid = `auspuff${useId().replace(/[^a-zA-Z0-9_-]/g, '')}`;
  const titleId = `${uid}-title`;
  const descId = `${uid}-desc`;
  const arrowId = `${uid}-arrow`;
  const vb = getAuspuffViewBox(geometry, 60, { crop: true });
  // Part names sit inside narrow sections (v2 positions); squeeze a name to the room up to the
  // next one instead of letting neighbours overlap.
  const sortedParts = [...geometry.partLabels].sort((a, b) => a.x - b.x);
  const lastRect = geometry.rects[geometry.rects.length - 1];
  const roomFor = (index: number) => {
    const label = sortedParts[index]!;
    const next = sortedParts[index + 1];
    const end = next ? next.x : lastRect ? lastRect.x + lastRect.width : label.x + 400;
    return end - label.x - 24;
  };

  return (
    <div className="drawing-grid w-full overflow-hidden rounded-md border">
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
          <DimensionMarker id={arrowId} size={18} />
        </defs>
        <g fill="none" strokeLinecap="square">
          {geometry.rects.map((r, i) => (
            <rect
              key={`r${i}`}
              x={r.x}
              y={r.y}
              width={Math.max(0, r.width)}
              height={Math.max(0, r.height)}
              strokeWidth={4}
              className="stroke-mod-auslass"
              data-kind="frame"
            />
          ))}
          {geometry.segments.map((s, i) =>
            s.kind === 'dimension' ? (
              <DimensionLine
                key={`s${i}-${s.x1}-${s.x2}-${s.y1}-${s.y2}`}
                x1={s.x1}
                y1={s.y1}
                x2={s.x2}
                y2={s.y2}
                markerId={arrowId}
                strokeWidth={2.5}
              />
            ) : (
              <line
                key={`s${i}`}
                x1={s.x1}
                y1={s.y1}
                x2={s.x2}
                y2={s.y2}
                strokeWidth={s.kind === 'frame' ? 4 : 1.5}
                className={s.kind === 'frame' ? 'stroke-mod-auslass' : 'stroke-muted-foreground'}
                data-kind={s.kind}
              />
            ),
          )}
        </g>
        <g fontSize={FONT_SIZE}>
          {geometry.valueLabels.map((l, i) => (
            <text
              key={`v${i}`}
              x={l.x}
              y={l.y}
              className="fill-primary font-data"
              data-testid={`auspuff-value-${l.text}`}
            >
              {l.text}
            </text>
          ))}
          {sortedParts.map((l, index) => {
            const text = t.auslass.parts[l.key];
            const room = roomFor(index);
            const estimate = text.length * PART_FONT_SIZE * 0.52;
            const squeeze = room > 0 && estimate > room;
            return (
              <text
                key={l.key}
                x={l.x}
                y={l.y}
                fontSize={PART_FONT_SIZE}
                dominantBaseline="central"
                className="fill-foreground font-display"
                textLength={squeeze ? room : undefined}
                lengthAdjust={squeeze ? 'spacingAndGlyphs' : undefined}
                data-testid={`auspuff-part-${l.key}`}
              >
                {text}
              </text>
            );
          })}
        </g>
        {geometry.valueLabels.map((l) => {
          const n = positions[l.text];
          if (n === undefined) return null;
          // Diameter codes (D1, D2) sit beside a vertical dimension: put the callout below them;
          // length codes sit under a horizontal dimension: put it to their left.
          const isDiameter = l.text.startsWith('D');
          const x = isDiameter ? l.x + 18 : l.x - CALLOUT_R - 8;
          const y = isDiameter ? l.y + CALLOUT_R + 22 : l.y - FONT_SIZE / 3;
          return <SvgCallout key={`c-${l.text}`} n={n} x={x} y={y} r={CALLOUT_R} />;
        })}
      </svg>
    </div>
  );
}
