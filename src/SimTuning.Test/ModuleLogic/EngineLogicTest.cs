// Copyright (c) 2021 tuke productions. All rights reserved.
namespace SimTuning.Test
{
    using SimTuning.Core.Models;
    using SimTuning.Core.ModuleLogic;
    using SkiaSharp;
    using System.IO;
    using Xunit;

    /// <summary>
    /// EngineLogicTest.
    /// </summary>
    public class EngineLogicTest
    {
        /// <summary>
        /// Compressions the test.
        /// </summary>
        [Fact]
        public void CompressionTest()
        {
            double value;
            double hubraum;
            double brennraum;
            double durchmesser;

            hubraum = 50000;
            brennraum = 17000;
            durchmesser = 38;

            value = EngineLogic.GetCompression(
                hubraum,
                brennraum,
                durchmesser);
        }

        /// <summary>
        /// Cylinders the hole diameter test.
        /// </summary>
        [Fact]
        public void CylinderHoleDiameterTest()
        {
            double value;
            double hubraum;
            double hub;

            hubraum = 50000;
            hub = 17000;

            value = EngineLogic.GetCylinderHoleDiameter(hubraum, hub);
        }

        /// <summary>
        /// Displacements the test.
        /// </summary>
        [Fact]
        public void DisplacementTest()
        {
            double value;
            double bohrungsdurchmesser;
            double hub;

            hub = 17000;
            bohrungsdurchmesser = 38;

            value = EngineLogic.GetDisplacement(bohrungsdurchmesser, hub);
        }

        /// <summary>
        /// Distances to ot test.
        /// </summary>
        [Fact]
        public void DistanceToOTTest()
        {
            double value;
            double pleullaenge;
            double hubradius;
            double deachsierung;
            double kwgrad;

            pleullaenge = 44;
            hubradius = 22;
            deachsierung = 2;
            kwgrad = 120;

            value = EngineLogic.GetDistanceToOT(pleullaenge, hubradius, deachsierung, kwgrad);
        }

        /// <summary>
        /// Grindings the diameters test.
        /// </summary>
        [Fact]
        public void GrindingDiametersTest()
        {
            GrindingDiametersModel value;
            double durchmesser;

            durchmesser = 38;

            value = EngineLogic.GetGrindingDiameters(durchmesser);
        }

        /// <summary>
        /// Hubs the radius test.
        /// </summary>
        [Fact]
        public void HubRadiusTest()
        {
            double value;
            double hub;
            double pleullaenge;
            double deachsierung;

            hub = 44;
            pleullaenge = 44;
            deachsierung = 2;

            value = EngineLogic.GetHubRadius(hub, pleullaenge, deachsierung);
        }

        /// <summary>
        /// Kolbens the durchmesser test.
        /// </summary>
        [Fact]
        public void KolbenDurchmesserTest()
        {
            double value;
            double bohrungsdurchmesser;
            double einbauspiel;

            bohrungsdurchmesser = 38;
            einbauspiel = 0.03;

            value = EngineLogic.GetKolbenDurchmesser(bohrungsdurchmesser, einbauspiel);
        }

        /// <summary>
        /// Kolbens the geschwindigkeit test.
        /// </summary>
        [Fact]
        public void KolbenGeschwindigkeitTest()
        {
            double value;
            double hub;
            double drehzahl;

            hub = 44;
            drehzahl = 8000;

            value = EngineLogic.GetKolbenGeschwindigkeit(hub, drehzahl);
        }

        /// <summary>
        /// Steuerdiagramms the test.
        /// </summary>
        [Fact]
        public void SteuerdiagrammTest()
        {
            SKBitmap value;
            double einlass;
            double auslass;
            double ueberstroemer;
            string _fileName;
            string _filePath;

            _fileName = "Steuerdiagramm.png";
            _filePath = Path.Combine(SimTuning.Test.Constants.Directory, _fileName);
            einlass = 120;
            auslass = 120;
            ueberstroemer = 120;

            value = EngineLogic.GetSteuerdiagramm(einlass, auslass, ueberstroemer);

            using (var image = SKImage.FromBitmap(value))
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
        /// Steuerwinkels the test.
        /// </summary>
        [Fact]
        public void SteuerwinkelTest()
        {
            double value;
            double vorherSteuerzeit;
            double nachherSteuerzeit;
            bool kolbenoberkante;
            bool kolbenunterkante;

            vorherSteuerzeit = 120;
            nachherSteuerzeit = 130;
            kolbenoberkante = true;
            kolbenunterkante = false;

            EngineLogic.GetSteuerwinkel(vorherSteuerzeit, nachherSteuerzeit, kolbenoberkante, kolbenunterkante);
            value = EngineLogic.GetSteuerwinkelOeffnet();
            value = EngineLogic.GetSteuerwinkelSchließt();
        }

        /// <summary>
        /// Converts to decreasinglengthtest.
        /// </summary>
        [Fact]
        public void ToDecreasingLengthTest()
        {
            double value;
            double hubraum;
            double brennraum;
            double durchmesser;
            double zielVerdichtung;

            hubraum = 50000;
            brennraum = 17000;
            durchmesser = 38;
            zielVerdichtung = 12;

            value = EngineLogic.GetToDecreasingLength(hubraum, brennraum, durchmesser, zielVerdichtung);
        }

        /// <summary>
        /// Vorauslasses the test.
        /// </summary>
        [Fact]
        public void VorauslassTest()
        {
            double value;
            double auslassSW;
            double ueberstroemerSW;

            auslassSW = 190;
            ueberstroemerSW = 125;

            value = EngineLogic.GetVorauslass(auslassSW, ueberstroemerSW);
        }
    }
}