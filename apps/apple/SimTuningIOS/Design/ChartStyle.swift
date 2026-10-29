import Charts
import SwiftUI

extension View {
    /// Swift Charts on the drawing sheet: plot area on the 5 mm grid with a hairline frame,
    /// hairline grid lines, monospaced axis values. Series colours come from `Theme.series`.
    func stChartStyle() -> some View {
        chartPlotStyle { plot in
            plot
                .background { DrawingGrid() }
                .border(Theme.border, width: 1)
        }
        .chartXAxis {
            AxisMarks { _ in
                AxisGridLine(stroke: StrokeStyle(lineWidth: 0.5)).foregroundStyle(Theme.border)
                AxisTick(stroke: StrokeStyle(lineWidth: 1)).foregroundStyle(Theme.border)
                AxisValueLabel().font(.stData(.caption2, weight: .regular)).foregroundStyle(Theme.mutedForeground)
            }
        }
        .chartYAxis {
            AxisMarks { _ in
                AxisGridLine(stroke: StrokeStyle(lineWidth: 0.5)).foregroundStyle(Theme.border)
                AxisTick(stroke: StrokeStyle(lineWidth: 1)).foregroundStyle(Theme.border)
                AxisValueLabel().font(.stData(.caption2, weight: .regular)).foregroundStyle(Theme.mutedForeground)
            }
        }
    }
}
