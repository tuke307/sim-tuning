// Copyright (c) 2021 tuke productions. All rights reserved.
using LiveChartsCore;
using MathNet.Numerics;
using SimTuning.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimTuning.Core.ModuleLogic
{
    /// <summary>
    /// Dyno Logik.
    /// </summary>
    public static class DynoLogic
    {
        #region variables

        private static Func<double, double> ausrollFunction;
        private static List<ISeries> ausrollPoints = new List<ISeries>();
        private static Func<double, double> drehzahlfunction;
        private static Func<double, double> geschwindigkeitsFunction;
        private static List<ISeries> geschwindigkeitsPoints = new List<ISeries>();
        private static List<ISeries> leistungsPoints = new List<ISeries>();
        private static int regressionsAnzahl = 5;

        /// <summary>
        /// Gets or sets the graphauswahl.
        /// </summary>
        /// <value>The graphauswahl.</value>
        public static int Graphauswahl { get; set; }

        /// <summary>
        /// Gets the plot audio.
        /// </summary>
        /// <value>The plot audio.</value>
        public static ISeries PlotAudio { get; private set; }

        /// <summary>
        /// Gets the plot ausrollen.
        /// </summary>
        /// <value>The plot ausrollen.</value>
        public static ISeries PlotAusrollen { get; private set; }

        /// <summary>
        /// Gets the plot beschleunigung.
        /// </summary>
        /// <value>The plot beschleunigung.</value>
        public static ISeries PlotGeschwindigkeit { get; private set; }

        /// <summary>
        /// Gets the plot strength.
        /// </summary>
        /// <value>The plot strength.</value>
        public static ISeries PlotLeistung { get; private set; }

        /// <summary>
        /// Gets array Handling.
        /// 1. Liste = Graphen, 2. Liste = Punkte.
        /// </summary>
        private static List<List<ISeries>> DrehzahlArrayPoints => AudioLogic.DrehzahlPoints;

        /// <summary>
        /// Gets the drehzahl points.
        /// </summary>
        /// <value>The drehzahl points.</value>
        private static List<ISeries> DrehzahlPoints => AudioLogic.DrehzahlPoints[Graphauswahl];

        #endregion variables

        /// <summary>
        /// Entfernt einen Punkt in einer PlotSeries.
        /// TODO: implement.
        /// </summary>
        /// <param name="ISeries">The data point.</param>
        public static void EntfernePunkt(this ISeries ISeries/*, ScreenPoint screenPoint*/)
        {
            try
            {
                //OxyPlot.ElementCollection<OxyPlot.Axes.Axis> axisList = ISeries.Axes;

                //Axis xAxis = axisList.FirstOrDefault(ax => ax.Position == AxisPosition.Bottom);
                //Axis yAxis = axisList.FirstOrDefault(ax => ax.Position == AxisPosition.Left);

                //ISeries ISeries = OxyPlot.Axes.Axis.InverseTransform(screenPoint, xAxis, yAxis);

                //int count;
                //count = ((LineSeries)ISeries.Series[0]).Points.Count();
                //((LineSeries)ISeries.Series[0]).Points.Remove(ISeries);
                //count = ((LineSeries)ISeries.Series[0]).Points.Count();
                //ISeries.InvalidatePlot(true);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Berechnet.
        /// TODO: vervollständigen.
        /// </summary>
        public static void GetAusrollGraphFitted(List<AusrollenModel> ausrollenModels)
        {
            DefiniereAusrollGraph();

            // Points definieren
            for (int i = 0; i < ausrollenModels.Count; i++)
            {
                //ausrollPoints.Add(new ISeries(
                //    (double)ausrollenModels[i].CreatedDate.Value.Millisecond,
                //    ausrollenModels[i].Speed.Value
                //));
            }

            // Regressions-Punkte bilden
            //ausrollFunction = Fit.PolynomialFunc(
            //    ausrollPoints.Select(x => x.X).ToArray(),
            //    ausrollPoints.Select(y => y.Y).ToArray(),
            //    regressionsAnzahl);

            LadeAusrollGraph(fitted: true);
        }

        /// <summary>
        /// Gibt das Diagramm aus den Spectogram Daten zurück. vorher muss Audio-Spectrogram der Audio bereits einmal berechnet sein mit AudioLogic.GetSpectrogram().
        /// </summary>
        /// <param name="areas">if set to <c>true</c> [filter] Punkte werden zu Bereichen zugeordnet.</param>
        public static void GetDrehzahlGraph(bool areas = false, double intensity = 0.75, double areaAbstand = 8)
        {
            DefiniereDrehzahlGraph();

            AudioLogic.GetDrehzahlGraph(areas: areas, intensity: intensity, areaAbstand: areaAbstand);

            LadeDrehzahlGraph(fitted: false);
        }

        /// <summary>
        /// Gets the fitted drehzahl graph.
        /// </summary>
        /// <param name="drehzahlModels">The drehzahl models.</param>
        public static void GetDrehzahlGraphFitted(out List<DrehzahlModel> drehzahlModels)
        {
            DefiniereDrehzahlGraph();

            // Regressions-Punkte bilden
            //drehzahlfunction = Fit.PolynomialFunc(
            //    DrehzahlPoints.Select(x => x.X).ToArray(),
            //    DrehzahlPoints.Select(x => x.Y).ToArray(),
            //    regressionsAnzahl);

            var _drehzahlModels = new List<DrehzahlModel>();

            // Audio Werte (X, Y) hinzufügen
            for (int count = 0; count < DrehzahlPoints.Count; count++)
            {
                //_drehzahlModels.Add(new DrehzahlModel()
                //{
                //    Zeit = DrehzahlPoints[count].X,
                //    Drehzahl = DrehzahlPoints[count].Y,
                //});
            }

            LadeDrehzahlGraph(fitted: true);

            drehzahlModels = _drehzahlModels;
        }

        /// <summary>
        /// Berechnet.
        /// TODO: vervollständigen.
        /// </summary>
        public static void GetGeschwindigkeitsGraphFitted(List<GeschwindigkeitModel> beschleunigungModels)
        {
            DefiniereGeschwindigkeitsGraph();

            // Points definieren
            for (int i = 0; i < beschleunigungModels.Count; i++)
            {
                //geschwindigkeitsPoints.Add(new ISeries(
                //    (double)beschleunigungModels[i].CreatedDate.Value.Millisecond,
                //    beschleunigungModels[i].Speed.Value
                //));
            }

            // Regressions-Punkte bilden
            //geschwindigkeitsFunction = Fit.PolynomialFunc(
            //    geschwindigkeitsPoints.Select(x => x.X).ToArray(),
            //    geschwindigkeitsPoints.Select(y => y.Y).ToArray(),
            //    regressionsAnzahl);

            LadeGeschwindigkeitsGraph(fitted: true);
        }

        /// <summary>
        /// Berechnet die Leistung(PS) aus Beschleunigung, Drehzahl und Ausrollen.
        /// </summary>
        /// <param name="dyno">The dyno.</param>
        /// <param name="plot">The plot.</param>
        /// <param name="ps">The ps.</param>
        /// <param name="nm">The nm.</param>
        public static void GetLeistungsGraph(double gewicht, out List<DynoPsModel> dynoPsModels)
        {
            DefiniereLeistungsGraph();

            List<DynoPsModel> dynoPs = new List<DynoPsModel>();

            // var max = ausrollPoints.Select(x => x.X).Max();
            //double max = geschwindigkeitsPoints.Select(x => x.X).Max();

            // jede 1/100 (0.01) sekunde wird die Formel ausgeführt
            //for (double i = 0.0; i < max; i += 0.01)
            //{
            //    double zeit = i;
            //    double masse = gewicht;
            //    double geschwindigkeit = geschwindigkeitsFunction(zeit);
            //    // a = v/t
            //    double beschleunigung = geschwindigkeit / zeit;
            //    double cw = 0;
            //    double y = 0;

            //    double ps = DynoLogic.StrengthFormula(masse, geschwindigkeit, beschleunigung, cw, y);

            //    //leistungsPoints.Add(new ISeries(zeit, ps));
            //}

            LadeLeistungsGraph();

            dynoPsModels = dynoPs;
        }

        /// <summary>
        /// Funktion zur Anwendung der Formel zur Kraftberechnung.
        /// Formel: P=m*v*a+cwv^y+1.
        /// </summary>
        /// <param name="masse">The masse.</param>
        /// <param name="geschwindigkeit">The geschwindigkeit.</param>
        /// <param name="beschleunigung">The beschleunigung.</param>
        /// <param name="cw">The cw.</param>
        /// <param name="y">The y.</param>
        /// <returns>Kraft in PS.</returns>
        public static double StrengthFormula(double masse, double geschwindigkeit, double beschleunigung, double cw = 0, double y = 0)
        {
            return (masse * geschwindigkeit * beschleunigung) + (cw * y);
        }

        /// <summary>
        /// Definieres the ausroll plot.
        /// </summary>
        private static void DefiniereAusrollGraph()
        {
            if (PlotAusrollen != null)
            {
                return;
            }

            //PlotAusrollen = new ISeries();

            //// Achsen
            //PlotAusrollen.Axes.Add(new OxyPlot.Axes.LinearAxis { Title = "Zeit in s", Position = OxyPlot.Axes.AxisPosition.Bottom, MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot });
            //PlotAusrollen.Axes.Add(new OxyPlot.Axes.LinearAxis { Title = "Geschwindigkeit in km/h", Position = OxyPlot.Axes.AxisPosition.Left, MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot });

            //PlotAusrollen.IsLegendVisible = true;
            //PlotAusrollen.Legends.FirstOrDefault().LegendPosition = LegendPosition.RightTop;
        }

        /// <summary>
        /// Initialisierung des Graphen.
        /// </summary>
        private static void DefiniereDrehzahlGraph()
        {
            // if (PlotAudio != null) { return; }

            //PlotAudio = new ISeries();

            //// Achsen
            //PlotAudio.Axes.Add(new OxyPlot.Axes.LinearAxis { Title = "Zeit in s", Position = OxyPlot.Axes.AxisPosition.Bottom, MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot });
            //PlotAudio.Axes.Add(new OxyPlot.Axes.LinearAxis { Title = "Drehzahl in 1/min", Position = OxyPlot.Axes.AxisPosition.Left, MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot });

            //PlotAudio.IsLegendVisible = true;
            //PlotAudio.Legends.FirstOrDefault().LegendPosition = LegendPosition.RightTop;
        }

        /// <summary>
        /// Definieres the beschleunigungs plot.
        /// </summary>
        private static void DefiniereGeschwindigkeitsGraph()
        {
            if (PlotGeschwindigkeit != null)
            {
                return;
            }

            //PlotGeschwindigkeit = new ISeries();

            //// Achsen
            //PlotGeschwindigkeit.Axes.Add(new OxyPlot.Axes.LinearAxis { Title = "Zeit in s", Position = OxyPlot.Axes.AxisPosition.Bottom, MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot });
            //PlotGeschwindigkeit.Axes.Add(new OxyPlot.Axes.LinearAxis { Title = "Geschwindigkeit in km/h", Position = OxyPlot.Axes.AxisPosition.Left, MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot });

            //PlotGeschwindigkeit.IsLegendVisible = true;
            //PlotGeschwindigkeit.Legends.FirstOrDefault().LegendPosition = LegendPosition.RightTop;
        }

        /// <summary>
        /// Initialisierung des Graphen.
        /// </summary>
        private static void DefiniereLeistungsGraph()
        {
            //PlotLeistung = new ISeries();

            //// Achsen
            //PlotLeistung.Axes.Add(new OxyPlot.Axes.LinearAxis { Title = "Drehzahl in 1/min", Position = OxyPlot.Axes.AxisPosition.Bottom });
            //PlotLeistung.Axes.Add(new OxyPlot.Axes.LinearAxis { Title = "Leistung in PS", Position = OxyPlot.Axes.AxisPosition.Left });

            //PlotLeistung.IsLegendVisible = true;
            //PlotLeistung.Legends.FirstOrDefault().LegendPosition = LegendPosition.RightTop;
        }

        /// <summary>
        /// Lades the ausroll graph.
        /// </summary>
        private static void LadeAusrollGraph(bool fitted = false)
        {
            if (fitted)
            {
                //OxyPlot.Series.FunctionSeries functionSeries = new OxyPlot.Series.FunctionSeries(
                //   ausrollFunction,
                //   ausrollPoints.Select(x => x.X).Min(),
                //   ausrollPoints.Select(x => x.X).Max(),
                //   100);

                //PlotAusrollen.Series.Add(functionSeries);
            }
            else
            {
                //OxyPlot.Series.LineSeries lineSeries = new OxyPlot.Series.LineSeries();

                //lineSeries.Points.AddRange(ausrollPoints);

                //lineSeries.LineStyle = LineStyle.None;
                //lineSeries.MarkerType = MarkerType.Diamond;

                //PlotAusrollen.Series.Add(lineSeries);
            }
        }

        /// <summary>
        /// Definiert Graph für alle Punkte und den Areas.
        /// </summary>
        private static void LadeDrehzahlGraph(bool fitted = false)
        {
            // PlotAudio.Series.Clear();

            if (fitted)
            {
                // Graph
                //OxyPlot.Series.FunctionSeries functionSeries = new OxyPlot.Series.FunctionSeries(
                //    drehzahlfunction,
                //    DrehzahlPoints.Select(x => x.X).Min(),
                //    DrehzahlPoints.Select(x => x.X).Max(),
                //    100);

                //PlotAudio.Series.Add(functionSeries);
            }
            else
            {
                // graph
                //List<OxyPlot.Series.LineSeries> audioLine = new List<OxyPlot.Series.LineSeries>();

                //// spalte einfügen
                //for (int anzahl = 0; anzahl < DrehzahlArrayPoints.Count; anzahl++)
                //{
                //    audioLine.Add(new OxyPlot.Series.LineSeries { });
                //    audioLine[anzahl].Points.AddRange(DrehzahlArrayPoints[anzahl]);

                //    // Style
                //    audioLine[anzahl].Title = "Graph " + (anzahl + 1);
                //    // if (filtered_graph) { sound_graph[anzahl].LineStyle = LineStyle.Automatic; filtered_graph = false; } else { sound_graph[anzahl].LineStyle = LineStyle.None; }
                //    audioLine[anzahl].LineStyle = LineStyle.None;
                //    audioLine[anzahl].MarkerType = MarkerType.Diamond;

                //    // Graph einfügen
                //    PlotAudio.Series.Add(audioLine[anzahl]);
                //}
            }
        }

        /// <summary>
        /// Lades the beschleunigungs graph.
        /// </summary>
        private static void LadeGeschwindigkeitsGraph(bool fitted = false)
        {
            if (fitted)
            {
                //OxyPlot.Series.FunctionSeries functionSeries = new OxyPlot.Series.FunctionSeries(
                //   geschwindigkeitsFunction,
                //   geschwindigkeitsPoints.Select(x => x.X).Min(),
                //   geschwindigkeitsPoints.Select(x => x.X).Max(),
                //   100);

                //PlotGeschwindigkeit.Series.Add(functionSeries);
            }
            else
            {
                //OxyPlot.Series.LineSeries lineSeries = new OxyPlot.Series.LineSeries();

                //lineSeries.Points.AddRange(geschwindigkeitsPoints);

                //lineSeries.LineStyle = LineStyle.None;
                //lineSeries.MarkerType = MarkerType.Diamond;

                //PlotGeschwindigkeit.Series.Add(lineSeries);
            }
        }

        /// <summary>
        /// Lades the leistungs graph.
        /// </summary>
        private static void LadeLeistungsGraph()
        {
            // OxyPlot.Series.LineSeries leistungPsSeries = new OxyPlot.Series.LineSeries();

            //// Punkte
            ////for (int zaehler = 0; zaehler < dyno.Drehzahl.Count; zaehler++)
            ////{
            ////    // Nm leistung_nm.Points.Add(new ISeries(dynoNm[zaehler].Drehzahl,
            ////    // dynoNm[zaehler].Nm));

            ////    // PS
            // leistungPsSeries.Points.Add(new ISeries(dynoPs[zaehler].Drehzahl, dynoPs[zaehler].Ps)); }

            //// Style, Beschriftung leistung_nm.Title = "Leistung-Nm"; leistung_nm.Color =
            //// OxyColors.DarkRed;

            // leistungPsSeries.Title = "Leistung-PS"; leistungPsSeries.Color = OxyColors.DarkBlue;

            //// Graphen einfügen PlotStrength.Series.Add(leistung_nm);
            // PlotLeistung.Series.Add(leistungPsSeries);
        }
    }
}