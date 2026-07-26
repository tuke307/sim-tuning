// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Test
{
    using SimTuning.Core.ModuleLogic;
    using SimTuning.Data.Models;
    using SkiaSharp;
    using System.IO;
    using Xunit;

    /// <summary>
    /// AuslassLogicTest. Phase 15: added real assertions (finite / non-null) to every test.
    /// </summary>
    public class AuslassLogicTest
    {
        /// <summary>
        /// Auspuffs the test.
        /// </summary>
        [Fact]
        public void AuspuffTest()
        {
            var vehicle = new VehiclesModel
            {
                Motor = new MotorModel
                {
                    ResonanzU = 8000,
                    Auslass = new AuslassModel
                    {
                        LaengeL = 100,
                        SteuerzeitSZ = 190,
                        DurchmesserD = 45.83,
                        FlaecheA = 20,
                        Auspuff = new AuspuffModel
                        {
                            AbgasT = 548.69,
                            AbgasV = 548.69,
                            DiffusorStage = 1,
                            DiffusorW = 4,
                            DiffusorW1 = 7.5,
                            KruemmerF = 10,
                            KruemmerW = 0,
                            MittelteilF = 2.9,
                            GegenKonusW = 0,
                            EndrohrL = 295,
                            EndrohrD = 27,
                        },
                    },
                },
            };

            SKBitmap auspuff = AuslassLogic.Auspuff(ref vehicle);

            Assert.NotNull(auspuff);

            // Export for manual inspection (cross-platform temp dir).
            string filePath = Path.Combine(SimTuning.Test.Constants.Directory, "Auspuff.png");
            using (var image = SKImage.FromBitmap(auspuff))
            using (var data = image.Encode())
            using (var stream = File.OpenWrite(filePath))
            {
                data.SaveTo(stream);
            }
        }

        /// <summary>
        /// Gases the velocity test.
        /// </summary>
        [Fact]
        public void GasVelocityTest()
        {
            double value = AuslassLogic.GetGasGeschwindigkeit(550);

            Assert.True(double.IsFinite(value));
        }

        /// <summary>
        /// Manifolds the diameter test.
        /// </summary>
        [Fact]
        public void ManifoldDiameterTest()
        {
            double value = AuslassLogic.GetKruemmerDurchmesser(2, 10);

            Assert.True(double.IsFinite(value));
        }

        /// <summary>
        /// Manifolds the length test.
        /// </summary>
        [Fact]
        public void ManifoldLengthTest()
        {
            double value = AuslassLogic.GetKruemmerLaenge(20, 20, 20);

            Assert.True(double.IsFinite(value));
        }

        /// <summary>
        /// Resonances the length test.
        /// </summary>
        [Fact]
        public void ResonanceLengthTest()
        {
            double value = AuslassLogic.GetResonanzLaenge(auslassSteuerwinkel: 180, abgasTemperatur: 550, resonanzDrehzahl: 8000);

            Assert.True(double.IsFinite(value));
        }

        /// <summary>
        /// Vehicles the port duration test.
        /// </summary>
        [Fact]
        public void VehiclePortDurationTest()
        {
            double value = AuslassLogic.GetVehiclePortDuration(auslassSteuerzeit: 180, drehzahl: 8000);

            Assert.True(double.IsFinite(value));
        }
    }
}
