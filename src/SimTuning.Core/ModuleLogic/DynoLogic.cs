// Copyright (c) 2021 tuke productions. All rights reserved.
using LiveChartsCore.Defaults;
using SimTuning.Core.Models;
using SimTuning.Data.Models;
using System;

namespace SimTuning.Core.ModuleLogic
{
    /// <summary>
    /// Dyno Logik.
    /// </summary>
    public static class DynoLogic
    {
        public static double GetLeistung(double n, double t, double p, double cw, double i, double r, double m, double c, double g, double s)
        {
            double v = GetGeschwindigkeit(r, n, i);
            double a = GetBeschleunigung(v, t);

            double fr = GetRollreibungskraft(c, m, g);
            double fa = GetBeschleunigungskraft(m, a);
            double fw = GetStrömungswiderstandskraft(p, cw, a, v);
            double fs = GetSteigungskraft(m, g, s);

            double P = GetFahrzeugLeistung(fr, fw, fs, fa, v);

            // Wattstunden
            //double Wh = P * (t / 3600);

            return P / 735.49875;
        }

        public static DrehzahlModel ToDrehzahlModel(this ObservablePoint observablePoint)
        {
            return new DrehzahlModel((double)observablePoint.X, (double)observablePoint.Y);
        }

        public static ObservablePoint ToObservablePoint(this DataPoint dataPoint)
        {
            return new ObservablePoint(dataPoint.X, dataPoint.Y);
        }

        /// <summary>
        /// Funktion zur Anwendung der Formel zur Beschleunigung.
        /// Formel: a = v/t.
        /// </summary>
        /// <param name="v">Geschwindigkeit.</param>
        /// <param name="t">Zeit in s.</param>
        /// <returns>Beschleunigung.</returns>
        private static double GetBeschleunigung(double v, double t)
        {
            return v / t;
        }

        /// <summary>
        /// Funktion zur Anwendung der Formel zur Beschleunigungskraft.
        /// Formel: Fa = m*a.
        /// </summary>
        /// <param name="m">Masse in kg.</param>
        /// <param name="a">Beschleunigung.</param>
        /// <returns>Beschleunigungskraft.</returns>
        private static double GetBeschleunigungskraft(double m, double a)
        {
            return m * a;
        }

        /// <summary>
        /// Funktion zur Anwendung der Formel zur Kraftberechnung.
        /// Formel: P = (Fr + Fw + Fs + Fa) * v.
        /// </summary>
        /// <param name="fr">Rollreibungskraft.</param>
        /// <param name="fw">Strömungswiderstandskraft.</param>
        /// <param name="fs">Steigungskraft.</param>
        /// <param name="fa">Beschleunigungskraft.</param>
        /// <param name="v">Geschwindigkeit.</param>
        /// <returns>Leistung in W.</returns>
        private static double GetFahrzeugLeistung(double fr, double fw, double fs, double fa, double v)
        {
            return (fr + fw + fs + fa) * v;
        }

        /// <summary>
        /// Funktion zur Anwendung der Formel zur Geschwindigkeit.
        /// Formel: v = (2*r*pi*n*60)/i*1000.
        /// </summary>
        /// <param name="r">Radhalbmesser.</param>
        /// <param name="n">Motordrehzahl.</param>
        /// <param name="i">Gesamtüberstzung.</param>
        /// <returns>Geschwindigkeit.</returns>
        private static double GetGeschwindigkeit(double r, double n, double i)
        {
            return (2 * r * Math.PI * n * 60) / i * 1000;
        }

        /// <summary>
        /// Funktion zur Anwendung der Formel zur Rollreibungskraft.
        /// Formel: Fr = c*m*g.
        /// </summary>
        /// <param name="c">Rollreibungswert (0,005 bis 0,050 (im Gelände bedeutend grösser)).</param>
        /// <param name="m">Masse in kg.</param>
        /// <param name="g">Erdbeschleunigung 9,81m/s².</param>
        /// <returns>Rollreibungskraft.</returns>
        private static double GetRollreibungskraft(double c, double m, double g)
        {
            return c * m * g;
        }

        /// <summary>
        /// Funktion zur Anwendung der Formel zur Steigungskraft.
        /// Formel: Fs = m*g*s.
        /// </summary>
        /// <param name="m">Masse in kg.</param>
        /// <param name="g">Erdbeschleunigung 9,81m/s².</param>
        /// <param name="s">Steigung (0).</param>
        /// <returns>Steigungskraft.</returns>
        private static double GetSteigungskraft(double m, double g, double s)
        {
            return m * g * s;
        }

        /// <summary>
        /// Funktion zur Anwendung der Formel zur Strömungswiderstandskraft.
        /// Formel: Fw = (p/2)*cw*A*(v^2).
        /// </summary>
        /// <param name="p">Luftdichte (1,2 kg/m3 von luft).</param>
        /// <param name="cw">Luftwiderstandskonstante (0.8).</param>
        /// <param name="a">Fläche des Fahrzeugs in m2 (0.75 m^2).</param>
        /// <param name="v">Geschw in m/s (27,8 m/s bei 100km/h).</param>
        /// <returns>Strömungswiderstandskraft.</returns>
        private static double GetStrömungswiderstandskraft(double p, double cw, double a, double v)
        {
            return (p / 2) * cw * a * Math.Pow(v, 2);
        }
    }
}