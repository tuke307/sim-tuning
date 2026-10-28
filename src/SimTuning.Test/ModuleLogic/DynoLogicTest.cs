// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Test
{
    using SimTuning.Core.Models;
    using SimTuning.Core.ModuleLogic;
    using Xunit;

    /// <summary>
    /// DynoLogicTest. Phase 15: the old <c>PlotCreationTest</c> body was fully commented out and
    /// referenced removed APIs (OxyPlot <c>PngExporter</c>, the old <c>DynoLogic.PlotAudio</c>/
    /// <c>GetDrehzahlGraph</c> surface) — a false green. Replaced with a live test of the
    /// deterministic conversion extensions.
    /// </summary>
    public class DynoLogicTest
    {
        /// <summary>
        /// Converts a <see cref="DataPoint" /> to a LiveCharts <see cref="global::LiveChartsCore.Defaults.ObservablePoint" />.
        /// </summary>
        [Fact]
        public void ToObservablePointTest()
        {
            var dataPoint = new DataPoint(3, 4);

            var observablePoint = dataPoint.ToObservablePoint();

            Assert.Equal(3, observablePoint.X);
            Assert.Equal(4, observablePoint.Y);
        }
    }
}
