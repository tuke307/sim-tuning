// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Test
{
    using SimTuning.Core.Models;
    using SimTuning.Core.ModuleLogic;
    using SkiaSharp;
    using System.IO;
    using Xunit;

    /// <summary>
    /// EngineLogicTest. Phase 15: each smoke test now asserts the result is a finite number
    /// (or non-null for image/model results) — previously the returns were read into unused locals.
    /// </summary>
    public class EngineLogicTest
    {
        /// <summary>
        /// Compressions the test.
        /// </summary>
        [Fact]
        public void CompressionTest()
        {
            double value = EngineLogic.GetCompression(hubraum: 50000, brennraum: 17000, durchmesser: 38);

            Assert.True(double.IsFinite(value));
        }

        /// <summary>
        /// Cylinders the hole diameter test.
        /// </summary>
        [Fact]
        public void CylinderHoleDiameterTest()
        {
            double value = EngineLogic.GetCylinderHoleDiameter(hubraum: 50000, hub: 17000);

            Assert.True(double.IsFinite(value));
        }

        /// <summary>
        /// Displacements the test.
        /// </summary>
        [Fact]
        public void DisplacementTest()
        {
            double value = EngineLogic.GetDisplacement(bohrungsdurchmesser: 38, hub: 17000);

            Assert.True(double.IsFinite(value));
        }

        /// <summary>
        /// Distances to ot test.
        /// </summary>
        [Fact]
        public void DistanceToOTTest()
        {
            double value = EngineLogic.GetDistanceToOT(pleullaenge: 44, hubradius: 22, deachsierung: 2, kwgrad: 120);

            Assert.True(double.IsFinite(value));
        }

        /// <summary>
        /// Grindings the diameters test.
        /// </summary>
        [Fact]
        public void GrindingDiametersTest()
        {
            GrindingDiametersModel value = EngineLogic.GetGrindingDiameters(38);

            Assert.NotNull(value);
        }

        /// <summary>
        /// Hubs the radius test.
        /// </summary>
        [Fact]
        public void HubRadiusTest()
        {
            double value = EngineLogic.GetHubRadius(hub: 44, pleullaenge: 44, deachsierung: 2);

            Assert.True(double.IsFinite(value));
        }

        /// <summary>
        /// Kolbens the durchmesser test.
        /// </summary>
        [Fact]
        public void KolbenDurchmesserTest()
        {
            double value = EngineLogic.GetKolbenDurchmesser(bohrungsdurchmesser: 38, einbauspiel: 0.03);

            Assert.True(double.IsFinite(value));
        }

        /// <summary>
        /// Kolbens the geschwindigkeit test.
        /// </summary>
        [Fact]
        public void KolbenGeschwindigkeitTest()
        {
            double value = EngineLogic.GetKolbenGeschwindigkeit(hub: 44, drehzahl: 8000);

            Assert.True(double.IsFinite(value));
        }

        /// <summary>
        /// Steuerdiagramms the test.
        /// </summary>
        [Fact]
        public void SteuerdiagrammTest()
        {
            SKBitmap value = EngineLogic.GetSteuerdiagramm(einlass: 120, auslass: 120, ueberstroemer: 120);

            Assert.NotNull(value);

            // Export the rendered diagram for manual inspection (cross-platform temp dir).
            string filePath = Path.Combine(SimTuning.Test.Constants.Directory, "Steuerdiagramm.png");
            using (var image = SKImage.FromBitmap(value))
            using (var data = image.Encode())
            using (var stream = File.OpenWrite(filePath))
            {
                data.SaveTo(stream);
            }
        }

        /// <summary>
        /// Steuerwinkels the test.
        /// </summary>
        [Fact]
        public void SteuerwinkelTest()
        {
            EngineLogic.GetSteuerwinkel(vorherSteuerzeit: 120, nachherSteuerzeit: 130, kolbenoberkante: true, kolbenunterkante: false);

            Assert.True(double.IsFinite(EngineLogic.GetSteuerwinkelOeffnet()));
            Assert.True(double.IsFinite(EngineLogic.GetSteuerwinkelSchließt()));
        }

        /// <summary>
        /// Converts to decreasinglengthtest.
        /// </summary>
        [Fact]
        public void ToDecreasingLengthTest()
        {
            double value = EngineLogic.GetToDecreasingLength(hubraum: 50000, brennraum: 17000, durchmesser: 38, zielVerdichtung: 12);

            Assert.True(double.IsFinite(value));
        }

        /// <summary>
        /// Vorauslasses the test.
        /// </summary>
        [Fact]
        public void VorauslassTest()
        {
            double value = EngineLogic.GetVorauslass(auslassSW: 190, ueberstroemerSW: 125);

            Assert.True(double.IsFinite(value));
        }
    }
}
