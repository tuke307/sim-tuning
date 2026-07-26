// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Test
{
    using SimTuning.Core.ModuleLogic;
    using Xunit;

    /// <summary>
    /// EinlassLogicTest. Phase 15: added real (finite) assertions to every test.
    /// </summary>
    public class EinlassLogicTest
    {
        /// <summary>
        /// Resonanzs the laenge test.
        /// </summary>
        [Fact]
        public void ResonanzLaengeTest()
        {
            double value = EinlassLogic.GetResonanzLaenge(
                einlassflaeche: 20,
                einlasssteuerwinkel: 120,
                kurbelgehausevolumen: 17000,
                resonanzdrehzahl: 8000,
                ansaugleitungsdurchmesser: 20);

            Assert.True(double.IsFinite(value));
        }

        /// <summary>
        /// Vergasers the durchmesser test.
        /// </summary>
        [Fact]
        public void VergaserDurchmesserTest()
        {
            double value = EinlassLogic.GetVergaserDurchmesser(hubvolumen: 50000, resonanzdrehzahl: 8000, widerstandsFaktor: 0.9);

            Assert.True(double.IsFinite(value));
        }

        /// <summary>
        /// Vergasers the hauptduesen durchmesser test.
        /// </summary>
        [Fact]
        public void VergaserHauptduesenDurchmesserTest()
        {
            double value = EinlassLogic.GetVergaserHauptduesenDurchmesser(vergasergroeße: 18, widerstandsFaktor: 0.95);

            Assert.True(double.IsFinite(value));
        }
    }
}
