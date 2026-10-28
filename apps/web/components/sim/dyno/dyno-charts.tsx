'use client';

import React from 'react';
import {
  CartesianGrid,
  Legend,
  Line,
  LineChart,
  ResponsiveContainer,
  Scatter,
  ScatterChart,
  Tooltip,
  XAxis,
  YAxis,
  ZAxis,
} from 'recharts';

/**
 * Local chart frame. `@sim-tuning/ui`'s `ChartContainer` cannot be used here: apps/web resolves
 * its own copy of recharts, so the UI package's `ResponsiveContainer` context never reaches the
 * charts below (they rendered empty). Styling mirrors `ChartContainer`.
 */
function ChartFrame({
  children,
  testId,
}: {
  children: React.ComponentProps<typeof ResponsiveContainer>['children'];
  testId?: string;
}) {
  return (
    <div
      data-slot="chart"
      data-testid={testId}
      className="[&_.recharts-cartesian-axis-tick_text]:fill-muted-foreground [&_.recharts-cartesian-grid_line]:stroke-border/60 h-80 w-full text-xs [&_.recharts-surface]:outline-hidden"
    >
      <ResponsiveContainer width="100%" height="100%">
        {children}
      </ResponsiveContainer>
    </div>
  );
}

/** Distinct cluster colours ("Graph 1…n"), cycling for many clusters. */
export const SERIES_COLORS = [
  'var(--chart-1)',
  'var(--chart-2)',
  'var(--chart-3)',
  'var(--chart-4)',
  'var(--chart-5)',
  '#0ea5e9',
  '#a855f7',
  '#22c55e',
  '#ef4444',
  '#eab308',
];

export const seriesColor = (index: number) => SERIES_COLORS[index % SERIES_COLORS.length]!;

export interface XYPoint {
  x: number;
  y: number;
}

export interface ScatterSeriesSpec {
  key: string;
  name: string;
  color: string;
  points: XYPoint[];
  /** Draw as line without markers (fitted curves). */
  line?: boolean;
}

const integer = new Intl.NumberFormat(undefined, { maximumFractionDigits: 0 });
const formatTick = (value: number) => integer.format(value);

/** Recharts renders nothing for line-only scatter series markers. */
const NoMarker = () => <g />;

/** Scatter plot with optional line series (RPM points, clusters, fitted curve, GPS speeds). */
export function DynoScatterChart({
  series,
  xLabel,
  yLabel,
  testId,
}: {
  series: ScatterSeriesSpec[];
  xLabel: string;
  yLabel: string;
  testId?: string;
}) {
  return (
    <ChartFrame testId={testId}>
      <ScatterChart margin={{ top: 8, right: 16, bottom: 24, left: 8 }}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis
          type="number"
          dataKey="x"
          name={xLabel}
          domain={['dataMin', 'dataMax']}
          tickFormatter={formatTick}
          label={{ value: xLabel, position: 'insideBottom', offset: -12 }}
        />
        <YAxis
          type="number"
          dataKey="y"
          name={yLabel}
          domain={['auto', 'auto']}
          tickFormatter={formatTick}
          width={64}
          label={{ value: yLabel, angle: -90, position: 'insideLeft', offset: 4 }}
        />
        <ZAxis range={[14, 14]} />
        <Tooltip
          cursor={{ strokeDasharray: '3 3' }}
          formatter={(value) => (typeof value === 'number' ? formatTick(value) : String(value))}
        />
        <Legend verticalAlign="top" itemSorter={null} />
        {series.map((s) =>
          s.line ? (
            <Scatter
              key={s.key}
              name={s.name}
              data={s.points}
              fill={s.color}
              line={{ stroke: s.color, strokeWidth: 2.5 }}
              shape={NoMarker}
              isAnimationActive={false}
            />
          ) : (
            <Scatter
              key={s.key}
              name={s.name}
              data={s.points}
              fill={s.color}
              isAnimationActive={false}
            />
          ),
        )}
      </ScatterChart>
    </ChartFrame>
  );
}

/** Power curve: PS over U/min. */
export function PowerCurveChart({
  points,
  name,
  xLabel,
  yLabel,
  testId,
}: {
  points: { drehzahl: number; ps: number }[];
  name: string;
  xLabel: string;
  yLabel: string;
  testId?: string;
}) {
  return (
    <ChartFrame testId={testId}>
      <LineChart data={points} margin={{ top: 8, right: 16, bottom: 24, left: 8 }}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis
          type="number"
          dataKey="drehzahl"
          domain={['dataMin', 'dataMax']}
          tickFormatter={formatTick}
          label={{ value: xLabel, position: 'insideBottom', offset: -12 }}
        />
        <YAxis
          type="number"
          domain={['auto', 'auto']}
          width={72}
          tickFormatter={(v: number) =>
            new Intl.NumberFormat(undefined, {
              notation: 'compact',
              maximumFractionDigits: 2,
            }).format(v)
          }
          label={{ value: yLabel, angle: -90, position: 'insideLeft', offset: 4 }}
        />
        <Tooltip
          labelFormatter={(value) => `${formatTick(Number(value))} U/min`}
          formatter={(value) =>
            typeof value === 'number'
              ? new Intl.NumberFormat(undefined, { maximumFractionDigits: 2 }).format(value)
              : String(value)
          }
        />
        <Legend verticalAlign="top" itemSorter={null} />
        <Line
          type="monotone"
          dataKey="ps"
          name={name}
          stroke="var(--chart-1)"
          strokeWidth={2.5}
          dot={false}
          isAnimationActive={false}
        />
      </LineChart>
    </ChartFrame>
  );
}
