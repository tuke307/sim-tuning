// Copyright (c) 2021 tuke productions. All rights reserved.
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.Linq;

namespace SimTuning.Core.ModuleLogic
{
    public class TuningLogic
    {
        #region variables

        private double[] origSeriesX = new double[] { 3500, 3556, 3604, 3658, 3719, 3779, 3827, 3881, 3947, 4001, 4055, 4103, 4151, 4211, 4271, 4343, 4415, 4511, 4608, 4686, 4782, 4890, 5011, 5131, 5282, 5433, 5590, 5741, 5898, 6067, 6200, 6363, 6508, 6641, 6774, 6877, 6987, 7096, 7193, 7284, 7381, 7496 };

        private double[] origSeriesY = new double[] { 2.15, 2.20, 2.25, 2.30, 2.35, 2.40, 2.45, 2.51, 2.57, 2.62, 2.68, 2.74, 2.79, 2.85, 2.92, 2.99, 3.06, 3.14, 3.22, 3.29, 3.35, 3.44, 3.51, 3.58, 3.62, 3.64, 3.65, 3.64, 3.62, 3.60, 3.58, 3.53, 3.48, 3.43, 3.36, 3.30, 3.22, 3.13, 3.05, 2.96, 2.86, 2.72 };

        //public ISeries PlotTuning { get; private set; }

        //public PlotController PlotTuningController { get; private set; }

        #endregion variables

        //public PlotController Controller_pan_off()
        //{
        //    // unbind
        //    PlotTuningController.UnbindMouseDown(OxyMouseButton.Right);

        //    return PlotTuningController;
        //}

        //public PlotController Controller_pan_on()
        //{
        //    // bind
        //    PlotTuningController.BindMouseDown(OxyMouseButton.Right, OxyPlot.PlotCommands.PanAt);

        //    return PlotTuningController;
        //}

        //public PlotController Controller_tracker_off()
        //{
        //    // unbind
        //    PlotTuningController.UnbindMouseDown(OxyMouseButton.Left);

        //    return PlotTuningController;
        //}

        //public PlotController Controller_tracker_on()
        //{
        //    // bind
        //    PlotTuningController.BindMouseDown(OxyMouseButton.Left, OxyPlot.PlotCommands.Track); // alle werte
        //    PlotTuningController.BindMouseDown(OxyMouseButton.Left, OxyPlot.PlotCommands.SnapTrack); // Punkte des Graphen
        //    // plot_controller.BindMouseDown(OxyMouseButton.Left, OxyPlot.PlotCommands.HoverSnapTrack); plot_controller.BindMouseDown(OxyMouseButton.Left, OxyPlot.PlotCommands.HoverPointsOnlyTrack);

        //    return PlotTuningController;
        //}

        //public PlotController Controller_zoom_off()
        //{
        //    // unbind
        //    PlotTuningController.UnbindMouseWheel();

        //    return PlotTuningController;
        //}

        //public PlotController Controller_zoom_on()
        //{
        //    // bind
        //    PlotTuningController.BindMouseWheel(OxyPlot.PlotCommands.ZoomWheel);
        //    PlotTuningController.BindMouseWheel(OxyModifierKeys.Control, OxyPlot.PlotCommands.ZoomWheelFine);

        //    return PlotTuningController;
        //}

        /// <summary>
        /// Defines the plot.
        /// </summary>
        public void DefinePlot()
        {
            //PlotTuning = new ISeries();
            //PlotTuningController = new PlotController();

            // Achsen
            //PlotTuning.Axes.Add(new OxyPlot.Axes.LinearAxis { Title = "Drehzahl in U/min", Position = OxyPlot.Axes.AxisPosition.Bottom, MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot });
            //PlotTuning.Axes.Add(new OxyPlot.Axes.LinearAxis { Title = "Leistung in PS", Position = OxyPlot.Axes.AxisPosition.Left, MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot });

            // Legende
            //PlotTuning.Legends.FirstOrDefault().LegendPosition = LegendPosition.RightTop;
        }

        /// <summary>
        /// Fügt die Original-Kurve ein.
        /// </summary>
        public void OriginalSeries()
        {
            // Datein einfügen mit legende und Achseneinteilung
            //LineSeries origSeries = new LineSeries<double>();

            //for (int i = 0; i <= origSeriesX.GetUpperBound(0) - 1; i++)
            //    origSeries.Points.Add(new ISeries(origSeriesX[i], origSeriesY[i]));

            //// Style
            //origSeries.Title = "Original";
            //origSeries.Color = OxyColors.Red;
            //origSeries.MarkerFill = OxyColors.DarkRed;
            //origSeries.MarkerType = MarkerType.Circle;

            //// einfügen des Graphen
            //PlotTuning.Series.Add(origSeries);
        }

        /// <summary>
        /// Fügt die Tuning-Kurve ein.
        /// </summary>
        public void TuningSeries()
        {
            // Datein einfügen mit legende und Achseneinteilung
            //OxyPlot.Series.LineSeries tuningSeries = new OxyPlot.Series.LineSeries();

            //// Punkte einfügen for (int zaehler = 0; zaehler <= dataX2.GetUpperBound(0); zaehler++) { tuningSeries.Points.Add(new ISeries(dataX2[zaehler], dataY2[zaehler])); }

            //// Style
            //tuningSeries.Title = "Tuning";
            //tuningSeries.Color = OxyColors.Blue;
            //tuningSeries.MarkerFill = OxyColors.DarkBlue;
            //tuningSeries.MarkerType = MarkerType.Circle;

            //// einfügen des Graphen
            //PlotTuning.Series.Add(tuningSeries);
        }
    }
}