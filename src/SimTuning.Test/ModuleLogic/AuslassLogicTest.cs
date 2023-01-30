// Copyright (c) 2021 tuke productions. All rights reserved.
namespace SimTuning.Test
{
    using SimTuning.Core.ModuleLogic;
    using SimTuning.Data.Models;
    using SimTuning.Test;
    using SkiaSharp;
    using System.IO;
    using Xunit;

    /// <summary>
    /// AuslassLogicTest.
    /// </summary>
    public class AuslassLogicTest
    {
        /// <summary>
        /// Auspuffs the test.
        /// </summary>
        [Fact]
        public void AuspuffTest()
        {
            SKBitmap auspuff;
            string _fileName;
            string _filePath;
            VehiclesModel _vehicle;

            // Vehicle Creation
            _vehicle = new VehiclesModel();
            _vehicle.Motor = new MotorModel();
            _vehicle.Motor.Auslass = new AuslassModel();
            _vehicle.Motor.Auslass.Auspuff = new AuspuffModel();

            _fileName = "Auspuff.png";
            _filePath = Path.Combine(SimTuning.Test.Constants.Directory, _fileName);

            #region Daten

            _vehicle.Motor.Auslass.Auspuff.AbgasT = 548.69;
            _vehicle.Motor.Auslass.Auspuff.AbgasV = 548.69;
            _vehicle.Motor.Auslass.LaengeL = 100;
            _vehicle.Motor.Auslass.SteuerzeitSZ = 190;
            _vehicle.Motor.ResonanzU = 8000;
            _vehicle.Motor.Auslass.DurchmesserD = 45.83;
            _vehicle.Motor.Auslass.FlaecheA = 20;
            _vehicle.Motor.Auslass.Auspuff.DiffusorW1 = 7.5;

            // 6 bis 12
            _vehicle.Motor.Auslass.Auspuff.KruemmerF = 10;
            _vehicle.Motor.Auslass.Auspuff.KruemmerW = 0;

            // 2 bis 3
            _vehicle.Motor.Auslass.Auspuff.MittelteilF = 2.9;

            _vehicle.Motor.Auslass.Auspuff.GegenKonusW = 0;
            // _vehicle.Motor.Auslass.Auspuff.GegenkonusL = 0;

            _vehicle.Motor.Auslass.Auspuff.DiffusorStage = 1;
            _vehicle.Motor.Auslass.Auspuff.DiffusorW = 4;
            // _vehicle.Motor.Auslass.Auspuff.DiffusorW1 = 0;
            // _vehicle.Motor.Auslass.Auspuff.DiffusorW2 = 0;
            // _vehicle.Motor.Auslass.Auspuff.DiffusorW3 = 0;

            _vehicle.Motor.Auslass.Auspuff.EndrohrL = 295;
            _vehicle.Motor.Auslass.Auspuff.EndrohrD = 27;

            #endregion Daten

            VehiclesModel _vehicle2 = _vehicle;
            auspuff = AuslassLogic.Auspuff(ref _vehicle2);
            _vehicle = _vehicle2;

            using (var image = SKImage.FromBitmap(auspuff))
            using (var data = image.Encode())
            {
                // save the data to a stream
                using (var stream = File.OpenWrite(_filePath))
                {
                    data.SaveTo(stream);
                }
            }
        }

        /// <summary>
        /// Gases the velocity test.
        /// </summary>
        [Fact]
        public void GasVelocityTest()
        {
            double value;
            double abgasT;

            abgasT = 550;

            value = AuslassLogic.GetGasGeschwindigkeit(abgasT);
        }

        /// <summary>
        /// Manifolds the diameter test.
        /// </summary>
        [Fact]
        public void ManifoldDiameterTest()
        {
            double value;
            double auslassA;
            int percentage;

            auslassA = 2;
            percentage = 10;

            value = AuslassLogic.GetKruemmerDurchmesser(auslassA, percentage);
        }

        /// <summary>
        /// Manifolds the length test.
        /// </summary>
        [Fact]
        public void ManifoldLengthTest()
        {
            double value;
            double kruemerdurchmesser;
            double drehmomentfaktor;
            double auslassLaenge;

            kruemerdurchmesser = 20;
            drehmomentfaktor = 20;
            auslassLaenge = 20;

            value = AuslassLogic.GetKruemmerLaenge(kruemerdurchmesser, drehmomentfaktor, auslassLaenge);
        }

        /// <summary>
        /// Resonances the length test.
        /// </summary>
        [Fact]
        public void ResonanceLengthTest()
        {
            double value;
            double auslassSteuerwinkel;
            double abgasTemperatur;
            double resonanzDrehzahl;

            auslassSteuerwinkel = 180;
            abgasTemperatur = 550;
            resonanzDrehzahl = 8000;

            value = AuslassLogic.GetResonanzLaenge(auslassSteuerwinkel, abgasTemperatur, resonanzDrehzahl);
        }

        /// <summary>
        /// Vehicles the port duration test.
        /// </summary>
        [Fact]
        public void VehiclePortDurationTest()
        {
            double value;
            double auslassSteuerzeit;
            double drehzahl;

            auslassSteuerzeit = 180;
            drehzahl = 8000;

            value = AuslassLogic.GetVehiclePortDuration(auslassSteuerzeit, drehzahl);
        }
    }
}