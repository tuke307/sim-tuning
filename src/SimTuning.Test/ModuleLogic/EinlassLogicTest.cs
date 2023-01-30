// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Core.ModuleLogic;
using SimTuning.Test;
using Xunit;

namespace SimTuning.Test
{
    /// <summary>
    /// EinlassLogicTest.
    /// </summary>
    public class EinlassLogicTest 
    {
        /// <summary>
        /// Resonanzs the laenge test.
        /// </summary>
        [Fact]
        public void ResonanzLaengeTest()
        {
            double value;
            double einlassflaeche;
            double einlasssteuerwinkel;
            double kurbelgehausevolumen;
            double resonanzdrehzahl;
            double ansaugleitungsdurchmesser;

            einlassflaeche = 20;
            einlasssteuerwinkel = 120;
            kurbelgehausevolumen = 17000;
            resonanzdrehzahl = 8000;
            ansaugleitungsdurchmesser = 20;

            value = EinlassLogic.GetResonanzLaenge(
                einlassflaeche,
                einlasssteuerwinkel,
                kurbelgehausevolumen,
                resonanzdrehzahl,
                ansaugleitungsdurchmesser);
        }

        /// <summary>
        /// Vergasers the durchmesser test.
        /// </summary>
        [Fact]
        public void VergaserDurchmesserTest()
        {
            double value;
            double hubvolumen;
            double resonanzdrehzahl;
            double widerstandsFaktor;

            hubvolumen = 50000;
            resonanzdrehzahl = 8000;
            widerstandsFaktor = 0.9;

            value = EinlassLogic.GetVergaserDurchmesser(hubvolumen, resonanzdrehzahl, widerstandsFaktor);
        }

        /// <summary>
        /// Vergasers the hauptduesen durchmesser test.
        /// </summary>
        [Fact]
        public void VergaserHauptduesenDurchmesserTest()
        {
            double value;
            double vergasergroeße;
            double widerstandsFaktor;

            vergasergroeße = 18;
            widerstandsFaktor = 0.95;

            value = EinlassLogic.GetVergaserHauptduesenDurchmesser(vergasergroeße, widerstandsFaktor);
        }
    }
}