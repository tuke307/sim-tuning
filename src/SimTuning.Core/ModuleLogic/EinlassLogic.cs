// Copyright (c) 2025 tuke productions. All rights reserved.
using System;

namespace SimTuning.Core.ModuleLogic
{
    /// <summary>
    /// Einlass Logik.
    /// </summary>
    public static class EinlassLogic
    {
        /// <summary>
        /// Berechnet die Einlass Resonanzlänge von Herzkasten bis Zylinder-Einlass.
        /// </summary>
        /// <param name="einlassflaeche">Einlassfläche in cm².</param>
        /// <param name="einlasssteuerwinkel">Einlass-Steuerwinkel.</param>
        /// <param name="kurbelgehausevolumen">Kurbelgehäuse-Volumen in cm³.</param>
        /// <param name="resonanzdrehzahl">Resonanzdrehzahl in 1/min.</param>
        /// <param name="ansaugleitungsdurchmesser">
        /// Ansaugleitungs-Durchmesser in cm.
        /// </param>
        /// <returns>Resonanzlänge in cm.</returns>
        public static double GetResonanzLaenge(double einlassflaeche, double einlasssteuerwinkel, double kurbelgehausevolumen, double resonanzdrehzahl, double ansaugleitungsdurchmesser)
        {
            double resonanzlaenge = einlassflaeche *
                                ((3062500 * Math.Pow(einlasssteuerwinkel, 2) / Math.Pow(resonanzdrehzahl, 2) *
                                kurbelgehausevolumen) -
                                (1 / ansaugleitungsdurchmesser)) * 10;

            resonanzlaenge = Math.Round(resonanzlaenge, 2);

            return resonanzlaenge;
        }

        /// <summary>
        /// Berechnet die Vergasergröße.
        /// Note: This calculation uses an empirical formula (rule of thumb) for approximation.
        /// For precise tuning, additional factors should be considered.
        /// </summary>
        /// <param name="hubvolumen">Hubvolumen in cm³.</param>
        /// <param name="resonanzdrehzahl">Resonanzdrehzahl in 1/min.</param>
        /// <param name="widerstandsFaktor">
        /// Strömungswiderstand-Faktor, Bereich von 0(schlecht) bis 1(beste).
        /// </param>
        /// <returns>Vergasergröße in mm.</returns>
        public static double GetVergaserDurchmesser(double hubvolumen, double resonanzdrehzahl, double widerstandsFaktor = 0.9)
        {
            double vergasergroeße = widerstandsFaktor * Math.Sqrt((hubvolumen / 1000) * resonanzdrehzahl);
            return Math.Round(vergasergroeße, 2);
        }

        /// <summary>
        /// Berechnet den Hauptdüsendurchmesser des Vergasers.
        /// Note: This calculation uses an empirical formula (rule of thumb) for approximation.
        /// For precise tuning, carburetor manufacturer specifications should be consulted.
        /// </summary>
        /// <param name="vergasergroeße">Die Vergasergröße in mm.</param>
        /// <param name="widerstandsFaktor">
        /// Strömungswiderstand-Faktor, Bereich von 0(schlecht) bis 1(beste).
        /// </param>
        /// <returns>Hauptdüsendurchmesser in μm.</returns>
        public static double GetVergaserHauptduesenDurchmesser(double vergasergroeße, double widerstandsFaktor = 0.95)
        {
            double hauptduesendurchmesser = vergasergroeße * 5 * widerstandsFaktor;
            return Math.Round(hauptduesendurchmesser, 0);
        }
    }
}