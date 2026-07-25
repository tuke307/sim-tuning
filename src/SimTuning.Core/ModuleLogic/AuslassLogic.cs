// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Core.ModuleLogic
{
    using SkiaSharp;
    using System;

    /// <summary>
    /// Auslass Logik.
    /// </summary>
    public static class AuslassLogic
    {
        /// <summary>
        /// Berechnet einen Auspuff und generiert eine Visualisierung.
        /// Note: Multi-stage diffuser calculations are partially implemented.
        /// Future enhancement: Complete implementation for 2 and 3-stage diffusors.
        /// </summary>
        /// <param name="vehicle">Die Daten für die Auspuffberechnung.</param>
        /// <returns>Bild des Auspuffs.</returns>
        public static SKBitmap Auspuff(ref Data.Models.VehiclesModel vehicle)
        {
            #region Berechnung

            vehicle.Motor.Auslass.Auspuff.ResonanzL = GetResonanzLaenge(vehicle.Motor.Auslass.SteuerzeitSZ.Value, vehicle.Motor.Auslass.Auspuff.AbgasT.Value, vehicle.Motor.ResonanzU.Value);

            // KRÜMMER
            vehicle.Motor.Auslass.Auspuff.KruemmerD = vehicle.Motor.Auslass.DurchmesserD.Value; /*Get_KruemmerDurchmesser(vehicleflaeche);*/
            vehicle.Motor.Auslass.Auspuff.KruemmerL = GetKruemmerLaenge(vehicle.Motor.Auslass.DurchmesserD.Value, vehicle.Motor.Auslass.Auspuff.KruemmerF.Value, vehicle.Motor.Auslass.LaengeL.Value);

            // MITTELTEIL
            vehicle.Motor.Auslass.Auspuff.MittelteilD = Math.Round(Math.Sqrt(vehicle.Motor.Auslass.FlaecheA.Value * 4 / Math.PI) * vehicle.Motor.Auslass.Auspuff.MittelteilF.Value, 2);

            // Konus
            vehicle.Motor.Auslass.Auspuff.DiffusorL1 = 0;
            vehicle.Motor.Auslass.Auspuff.DiffusorD1 = 0;
            vehicle.Motor.Auslass.Auspuff.DiffusorL2 = 0;
            vehicle.Motor.Auslass.Auspuff.DiffusorD2 = 0;
            vehicle.Motor.Auslass.Auspuff.DiffusorL3 = 0;
            vehicle.Motor.Auslass.Auspuff.DiffusorD3 = 0;
            switch (vehicle.Motor.Auslass.Auspuff.DiffusorStage)
            {
                case 1:
                    vehicle.Motor.Auslass.Auspuff.DiffusorD1 = Math.Round((2 * Math.Tan(vehicle.Motor.Auslass.Auspuff.KruemmerW.Value * Math.PI / 360) * vehicle.Motor.Auslass.Auspuff.KruemmerL.Value) + vehicle.Motor.Auslass.Auspuff.KruemmerD.Value, 2);
                    vehicle.Motor.Auslass.Auspuff.DiffusorL1 = Math.Round((vehicle.Motor.Auslass.Auspuff.MittelteilD.Value - vehicle.Motor.Auslass.Auspuff.DiffusorD1.Value) / (2 * Math.Tan(vehicle.Motor.Auslass.Auspuff.DiffusorW1.Value * (2 * Math.PI / 360))), 2);
                    break;

                case 2:
                    vehicle.Motor.Auslass.Auspuff.DiffusorL1 = 0;
                    vehicle.Motor.Auslass.Auspuff.DiffusorD1 = 0;
                    vehicle.Motor.Auslass.Auspuff.DiffusorL2 = 0;
                    vehicle.Motor.Auslass.Auspuff.DiffusorD2 = 0;
                    break;

                case 3:
                    vehicle.Motor.Auslass.Auspuff.DiffusorL1 = 0;
                    vehicle.Motor.Auslass.Auspuff.DiffusorD1 = 0;
                    vehicle.Motor.Auslass.Auspuff.DiffusorL2 = 0;
                    vehicle.Motor.Auslass.Auspuff.DiffusorD2 = 0;
                    vehicle.Motor.Auslass.Auspuff.DiffusorL3 = 0;
                    vehicle.Motor.Auslass.Auspuff.DiffusorD3 = 0;
                    break;

                default:
                    break;
            }

            // GEGENKONUS
            vehicle.Motor.Auslass.Auspuff.GegenkonusL = Math.Round((vehicle.Motor.Auslass.Auspuff.MittelteilD.Value - vehicle.Motor.Auslass.Auspuff.EndrohrD.Value) / (2 * Math.Tan(vehicle.Motor.Auslass.Auspuff.GegenKonusW.Value * Math.PI / 180)), 2);
            vehicle.Motor.Auslass.Auspuff.GegenkonusD = vehicle.Motor.Auslass.Auspuff.MittelteilD.Value;

            // MITTELTEIL
            vehicle.Motor.Auslass.Auspuff.MittelteilL = Math.Round(vehicle.Motor.Auslass.Auspuff.ResonanzL.Value - (vehicle.Motor.Auslass.Auspuff.KruemmerL.Value + vehicle.Motor.Auslass.Auspuff.DiffusorL1.Value + vehicle.Motor.Auslass.Auspuff.GegenkonusL.Value / 2), 2);

            #endregion Berechnung

            #region Groeßen

            /*
           * BILD
         * 1 = Krümmer
         * 2 = konus 1
         * 3 = konus 2
         * 4 = konus 3
         * 5 = Mittelstück
         * 6 = Gegenkonus
         * 7 = Endrohr
         */

            // allgemeine Bildgröße
            int pic_width = 2000;
            int pic_height = 1000;

            // HORIZONTAL, länge der Teile
            int width1 = (int)vehicle.Motor.Auslass.Auspuff.KruemmerL;
            int width2 = (int)vehicle.Motor.Auslass.Auspuff.DiffusorL1;
            int width3 = (int)vehicle.Motor.Auslass.Auspuff.DiffusorL2;
            int width4 = (int)vehicle.Motor.Auslass.Auspuff.DiffusorL3;
            int width5 = (int)vehicle.Motor.Auslass.Auspuff.MittelteilL;
            int width6 = (int)vehicle.Motor.Auslass.Auspuff.GegenkonusL;
            int width7 = (int)vehicle.Motor.Auslass.Auspuff.EndrohrL;

            // VERTIKAL, (Anfangs)Höhe der Teile
            int height1 = (int)vehicle.Motor.Auslass.Auspuff.KruemmerD * 2;
            int height2 = (int)vehicle.Motor.Auslass.Auspuff.DiffusorD1 * 2;
            int height3 = (int)vehicle.Motor.Auslass.Auspuff.DiffusorD2 * 2;
            int height4 = (int)vehicle.Motor.Auslass.Auspuff.DiffusorD3 * 2;
            int height5 = (int)vehicle.Motor.Auslass.Auspuff.MittelteilD * 2;
            int height6 = (int)vehicle.Motor.Auslass.Auspuff.GegenkonusD * 2;
            int height7 = (int)vehicle.Motor.Auslass.Auspuff.EndrohrD * 2;

            #endregion Groeßen

            #region Bild

            SKBitmap bmp_auspuff = new SKBitmap(pic_width, pic_height);
            SKCanvas graphic_auspuff = new SKCanvas(bmp_auspuff);

            SKPaint blackPen = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                StrokeWidth = 3,
                IsAntialias = true,
            };

            // SkiaSharp 3.x moved text metrics off SKPaint onto SKFont: SKPaint.TextSize and
            // DrawText(string,float,float,SKPaint) are obsolete. One label font (default
            // typeface, size 25) replaces the former blackPen.TextSize for all labels below.
            using SKFont labelFont = new SKFont { Size = 25 };

            SKPaint blackDoubleArrowPen = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                StrokeWidth = 4,
                IsAntialias = true,
                // AdjustableArrowCap bigArrow = new AdjustableArrowCap(3, 5); //spitzer
                // Pfeil blackDoubleArrowPen.CustomEndCap = bigArrow;
                // blackDoubleArrowPen.CustomStartCap = bigArrow;
            };

            SKPaint redPen = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.DarkRed,
                StrokeWidth = 4,
                IsAntialias = true,
            };

            #endregion Bild

            #region Positionshilfen

            int xbeginn = 100; // Abstand links
            int xstart1 = xbeginn;
            int xstart2 = xstart1 + width1;
            int xstart3 = xstart2 + width2;
            int xstart4 = xstart3 + width3;
            int xstart5 = xstart4 + width4;
            int xstart6 = xstart5 + width5;
            int xstart7 = xstart6 + width6;

            int ymiddle = (pic_height / 2) + (pic_height / 4);
            int ydiff1 = height1 / 2;
            int ydiff2 = height2 / 2;
            int ydiff3 = height3 / 2;
            int ydiff4 = height4 / 2;
            int ydiff5 = height5 / 2;
            int ydiff6 = height6 / 2;
            int ydiff7 = height7 / 2;

            #endregion Positionshilfen

            #region AuspuffGeruest

            // 1
            graphic_auspuff.DrawLine(
                new SKPoint(xstart1, ymiddle - ydiff1),
                new SKPoint(xstart2, ymiddle - ydiff2),
                redPen); // oben
            graphic_auspuff.DrawLine(
                new SKPoint(xstart1, ymiddle + ydiff1),
                new SKPoint(xstart2, ymiddle + ydiff2),
                redPen); // unten
            graphic_auspuff.DrawLine(
                new SKPoint(xstart1, ymiddle - ydiff1),
                new SKPoint(xstart1, ymiddle + ydiff1),
                redPen); // Links

            // ONE-Stage Diffusor
            if (vehicle.Motor.Auslass.Auspuff.DiffusorL1 != 0)
            {
                // TWO-Stage Diffusor
                if (vehicle.Motor.Auslass.Auspuff.DiffusorL2 != 0)
                {
                    // THREE-Stage Diffusor
                    if (vehicle.Motor.Auslass.Auspuff.DiffusorL3 != 0)
                    {
                        // THREE-Stages malen
                        graphic_auspuff.DrawLine(xstart4, ymiddle - ydiff4,
                                                 xstart5, ymiddle - ydiff5,
                                                 redPen); // oben
                        graphic_auspuff.DrawLine(xstart4, ymiddle + ydiff4,
                                                 xstart5, ymiddle + ydiff5,
                                                 redPen); // unten
                        graphic_auspuff.DrawLine(xstart4, ymiddle - ydiff4,
                                                 xstart4, ymiddle + ydiff4,
                                                 redPen); // links
                    }
                    else // TWO-Stages malen
                    {
                        graphic_auspuff.DrawLine(
                            new SKPoint(xstart3, ymiddle - ydiff3),
                            new SKPoint(xstart4, ymiddle - ydiff4),
                            redPen); // oben
                        graphic_auspuff.DrawLine(
                            new SKPoint(xstart3, ymiddle + ydiff3),
                            new SKPoint(xstart4, ymiddle + ydiff4),
                            redPen); // unten
                        graphic_auspuff.DrawLine(
                            new SKPoint(xstart3, ymiddle - ydiff3),
                            new SKPoint(xstart3, ymiddle + ydiff3),
                            redPen); // links
                    }
                }
                else // ONE-Stage malen
                {
                    // 2
                    graphic_auspuff.DrawLine(
                        new SKPoint(xstart2, ymiddle - ydiff2),
                        new SKPoint(xstart5, ymiddle - ydiff5),
                        redPen); // oben
                    graphic_auspuff.DrawLine(
                        new SKPoint(xstart2, ymiddle + ydiff2),
                        new SKPoint(xstart5, ymiddle + ydiff5),
                        redPen); // unten
                    graphic_auspuff.DrawLine(
                        new SKPoint(xstart2, ymiddle - ydiff2),
                        new SKPoint(xstart2, ymiddle + ydiff2),
                        redPen); // links
                }
            }

            // 5
            graphic_auspuff.DrawRect(xstart5, ymiddle - ydiff5,
                                     width5, height5,
                                     redPen);
            // 6
            graphic_auspuff.DrawLine(
                new SKPoint(xstart6, ymiddle - ydiff6),
                new SKPoint(xstart7, ymiddle - ydiff7),
                redPen); // oben
            graphic_auspuff.DrawLine(
                new SKPoint(xstart6, ymiddle + ydiff6),
                new SKPoint(xstart7, ymiddle + ydiff7),
                redPen); // unten
            // 7
            graphic_auspuff.DrawRect(xstart7, ymiddle - ydiff7,
                                     width7, height7,
                                     redPen);

            #endregion AuspuffGeruest

            #region MaßLinien

            /*
             * LÄNGEN
             */

            // 1
            graphic_auspuff.DrawLine(xstart1, ymiddle + ydiff1,
                                     xstart1, ymiddle + 200,
                                     blackPen); // Begrenzung-Links
            graphic_auspuff.DrawLine(xstart1, ymiddle + 175,
                                     xstart2, ymiddle + 175,
                                     blackDoubleArrowPen); // Maß-Linie

            // ONE-Stage Diffusor
            if (vehicle.Motor.Auslass.Auspuff.DiffusorL1 != 0)
            {
                // TWO-Stage Diffusor
                if (vehicle.Motor.Auslass.Auspuff.DiffusorL2 != 0)
                {
                    // THREE-Stage Diffusor
                    if (vehicle.Motor.Auslass.Auspuff.DiffusorL3 != 0)
                    {
                        // 2
                        graphic_auspuff.DrawLine(xstart2, ymiddle + ydiff2,
                                                 xstart2, ymiddle + 200,
                                                 blackPen); // Begrenzung-links
                        graphic_auspuff.DrawLine(xstart2, ymiddle + 175,
                                                 xstart3, ymiddle + 175,
                                                 blackDoubleArrowPen); // Maß-Linie

                        // 3
                        graphic_auspuff.DrawLine(xstart4, ymiddle + ydiff3,
                                                 xstart4, ymiddle + 200,
                                                 blackPen); // Begrenzung-Mitte
                        graphic_auspuff.DrawLine(xstart4, ymiddle + 175,
                                                 xstart5, ymiddle + 175,
                                                 blackDoubleArrowPen); // Maß-Linie
                        graphic_auspuff.DrawLine(xstart4, ymiddle + 175,
                                                 xstart5, ymiddle + 175,
                                                 blackDoubleArrowPen); // Maß-Linie

                        // 4
                        graphic_auspuff.DrawLine(xstart6, ymiddle + ydiff4,
                                                 xstart6, ymiddle + 200,
                                                 blackPen); // Begrenzung-Mitte
                        graphic_auspuff.DrawLine(xstart4, ymiddle + 175,
                                                 xstart5, ymiddle + 175,
                                                 blackDoubleArrowPen); // Maß-Linie
                    }
                    else
                    {
                        // 2
                        graphic_auspuff.DrawLine(xstart2, ymiddle + ydiff2,
                                                 xstart2, ymiddle + 200,
                                                 blackPen); // Begrenzung-links
                        graphic_auspuff.DrawLine(xstart2, ymiddle + 175,
                                                 xstart3, ymiddle + 175,
                                                 blackDoubleArrowPen); // Maß-Linie

                        // 3
                        graphic_auspuff.DrawLine(xstart4, ymiddle + ydiff3,
                                                 xstart4, ymiddle + 200,
                                                 blackPen); // Begrenzung-Mitte
                        graphic_auspuff.DrawLine(xstart4, ymiddle + 175,
                                                 xstart5, ymiddle + 175,
                                                 blackDoubleArrowPen); // Maß-Linie
                        graphic_auspuff.DrawLine(xstart4, ymiddle + 175,
                                                 xstart5, ymiddle + 175,
                                                 blackDoubleArrowPen); // Maß-Linie
                    }
                }
                else
                {
                    // 2
                    graphic_auspuff.DrawLine(xstart2, ymiddle + ydiff2,
                                             xstart2, ymiddle + 200,
                                             blackPen); // Begrenzung-links
                    graphic_auspuff.DrawLine(xstart2, ymiddle + 175,
                                             xstart3, ymiddle + 175,
                                             blackDoubleArrowPen); // Maß-Linie
                }
            }

            // 5
            graphic_auspuff.DrawLine(xstart5, ymiddle + ydiff5,
                                     xstart5, ymiddle + 200,
                                     blackPen); // Begrenzung-Links
            graphic_auspuff.DrawLine(xstart5, ymiddle + 175,
                                     xstart6, ymiddle + 175,
                                     blackDoubleArrowPen); // Maß-Linie
            // 6
            graphic_auspuff.DrawLine(xstart6, ymiddle + ydiff6,
                                     xstart6, ymiddle + 200,
                                     blackPen); // Begrenzung-Links
            graphic_auspuff.DrawLine(xstart6, ymiddle + 175,
                                     xstart7, ymiddle + 175,
                                     blackDoubleArrowPen); // Maß-Linie
            // 7
            graphic_auspuff.DrawLine(xstart7, ymiddle + ydiff7,
                                     xstart7, ymiddle + 200,
                                     blackPen); // Begrenzung-Links
            graphic_auspuff.DrawLine(xstart7 + width7, ymiddle + ydiff7,
                                     xstart7 + width7, ymiddle + 200,
                                     blackPen); // Begrenzung-Rechts
            graphic_auspuff.DrawLine(xstart7, ymiddle + 175,
                                     xstart7 + width7, ymiddle + 175,
                                     blackDoubleArrowPen); // Maß-Linie
            // konus
            graphic_auspuff.DrawLine(xstart2, ymiddle + 175,
                                     xstart5, ymiddle + 175,
                                     blackDoubleArrowPen); // Maß-Linie

            /*
             * Durchmesser
             */
            // Header
            graphic_auspuff.DrawLine(xstart1, ymiddle - ydiff1,
                                     xstart1 - 50, ymiddle - ydiff1,
                                     blackPen); // Begrenzung-oben
            graphic_auspuff.DrawLine(xstart1, ymiddle + ydiff1,
                                     xstart1 - 50, ymiddle + ydiff1,
                                     blackPen); // Begrenzung-unten
            graphic_auspuff.DrawLine(xstart1 - 35, ymiddle - ydiff1,
                                     xstart1 - 35, ymiddle + ydiff1,
                                     blackDoubleArrowPen); // Maß-Linie
            // Stringer
            graphic_auspuff.DrawLine(xstart7 + width7, ymiddle - ydiff7,
                                     xstart7 + width7 + 50, ymiddle - ydiff7,
                                     blackPen); // Begrenzung-oben
            graphic_auspuff.DrawLine(xstart7 + width7, ymiddle + ydiff7,
                                     xstart7 + width7 + 50, ymiddle + ydiff7,
                                     blackPen); // Begrenzung-unten
            graphic_auspuff.DrawLine(xstart7 + width7 + 35, ymiddle - ydiff7,
                                     xstart7 + width7 + 35, ymiddle + ydiff7,
                                     blackDoubleArrowPen); // Maß-Linie

            #endregion MaßLinien

            #region WerteBezeichnung

            /*
             * übergebene Werte werden genommen da genauer
             */

            // Längen
            graphic_auspuff.DrawText(/*kruemmerL.ToString()*/"LK", xstart1 + (width1 / 2) - 15, ymiddle + 200, SKTextAlign.Left, labelFont, blackPen);

            // ONE-Stage Diffusor
            if (vehicle.Motor.Auslass.Auspuff.DiffusorL1 != 0)
            {
                // TWO-Stage Diffusor
                if (vehicle.Motor.Auslass.Auspuff.DiffusorL2 != 0)
                {
                    // THREE-Stage Diffusor
                    if (vehicle.Motor.Auslass.Auspuff.DiffusorL3 != 0)
                    {
                        graphic_auspuff.DrawText(/*konus1L.ToString()*/"LD1", xstart2 + (width2 / 2) - 15, ymiddle + 150, SKTextAlign.Left, labelFont, blackPen);
                        graphic_auspuff.DrawText(/*konus2L.ToString()*/"LD2", xstart3 + (width3 / 2) - 15, ymiddle + 150, SKTextAlign.Left, labelFont, blackPen);
                        graphic_auspuff.DrawText(/*konus3L.ToString()*/"LD3", xstart4 + (width4 / 2) - 15, ymiddle + 150, SKTextAlign.Left, labelFont, blackPen);
                    }
                    else
                    {
                        graphic_auspuff.DrawText(/*konus1L.ToString()*/"LD1", xstart2 + (width2 / 2) - 15, ymiddle + 150, SKTextAlign.Left, labelFont, blackPen);
                        graphic_auspuff.DrawText(/*konus2L.ToString()*/"LD2", xstart3 + (width3 / 2) - 15, ymiddle + 150, SKTextAlign.Left, labelFont, blackPen);
                    }
                }
                else
                {
                    graphic_auspuff.DrawText(/*konus1L.ToString()*/"LD", xstart2 + (width2 / 2) - 15, ymiddle + 200, SKTextAlign.Left, labelFont, blackPen);
                }
            }

            graphic_auspuff.DrawText(/*mittelteilL.ToString()*/"LM", xstart5 + (width5 / 2) - 15, ymiddle + 200, SKTextAlign.Left, labelFont, blackPen);
            graphic_auspuff.DrawText(/*gegenkonusL.ToString()*/"LG", xstart6 + (width6 / 2) - 15, ymiddle + 200, SKTextAlign.Left, labelFont, blackPen);
            graphic_auspuff.DrawText(/*endrohrL.ToString()*/"LE", xstart7 + (width7 / 2) - 15, ymiddle + 200, SKTextAlign.Left, labelFont, blackPen);

            // Durchmesser
            graphic_auspuff.DrawText(
                "D1",
                xstart1 - 90, ymiddle - 15,
                SKTextAlign.Left, labelFont, blackPen);
            graphic_auspuff.DrawText(
                "D2",
                xstart7 + width7 + 65, ymiddle - 15,
                SKTextAlign.Left, labelFont, blackPen);
            // graphic_auspuff.DrawString("D3", drawFont, drawBrushblack, Xstart7 + width7
            // + 65, Ymiddle - 15); graphic_auspuff.DrawString("D4", drawFont,
            // drawBrushblack, Xstart7 + width7 + 65, Ymiddle - 15);

            #endregion WerteBezeichnung

            #region TeilBezeichnung

            graphic_auspuff.DrawText(
                "Krümmer",
                xstart1 + 20, ymiddle,
                SKTextAlign.Left, labelFont, blackPen);
            graphic_auspuff.DrawText(
                "Konus",
                xstart2 + 20, ymiddle,
                SKTextAlign.Left, labelFont, blackPen);
            graphic_auspuff.DrawText(
                "Mittelteil",
                xstart5 + 20, ymiddle,
                SKTextAlign.Left, labelFont, blackPen);
            graphic_auspuff.DrawText(
                "Gegenkonus",
                xstart6 + 20, ymiddle,
                SKTextAlign.Left, labelFont, blackPen);
            graphic_auspuff.DrawText(
                "Endrohr",
                xstart7 + 20, ymiddle,
                SKTextAlign.Left, labelFont, blackPen);

            #endregion TeilBezeichnung

            return bmp_auspuff;
        }

        /// <summary>
        /// Berechnet Abgasgeschwindigkeit.
        /// </summary>
        /// <param name="abgastemperatur">Die Abgastemperatur in °C.</param>
        /// <returns>Abasgeschwindigkeit in m/s.</returns>
        public static double GetGasGeschwindigkeit(double abgastemperatur)
        {
            double abgasgeschwindigkeit = 331 + (0.6 * abgastemperatur);
            abgasgeschwindigkeit = Math.Round(abgasgeschwindigkeit, 2);

            return abgasgeschwindigkeit;
        }

        /// <summary>
        /// Berechnet den Krümmer Durchmesser.
        /// </summary>
        /// <param name="auslassflaeche">Auslassfläche in cm².</param>
        /// <param name="percentage">
        /// Flächenvergrößerung in %, normalerweise im Bereich von 10% bis 20%.
        /// </param>
        /// <returns>Krümmerdurchmesser in cm.</returns>
        public static double GetKruemmerDurchmesser(double auslassflaeche, int percentage = 10)
        {
            // Vergrößerung um mehr als 100%
            double factor = 1 + (percentage / 100);

            // radius mal 2
            double durchmesser = Math.Sqrt(auslassflaeche * factor) / Math.Sqrt(Math.PI) * 2;
            durchmesser = Math.Round(durchmesser, 2);

            return durchmesser;
        }

        /// <summary>
        /// Berechnet die Krümmerlänge.
        /// </summary>
        /// <param name="kruemmerdurchmesser">The kruemmerdurchmesser.</param>
        /// <param name="drehmomentfaktor">The drehmomentfaktor.</param>
        /// <param name="auslassLaenge">The auslass laenge.</param>
        /// <returns>KrümmerLänge in cm.</returns>
        public static double GetKruemmerLaenge(double kruemmerdurchmesser, double drehmomentfaktor, double auslassLaenge)
        {
            // von mm in cm /10
            double kruemmerlaenge = (drehmomentfaktor * kruemmerdurchmesser) - auslassLaenge /*/ 10*/;
            kruemmerlaenge = Math.Round(kruemmerlaenge, 2);

            return kruemmerlaenge;
        }

        /// <summary>
        /// Berechnet Resonanzlänge.
        /// </summary>
        /// <param name="auslassSteuerwinkel">The auslass steuerwinkel.</param>
        /// <param name="abgasTemperatur">The abgas temperatur.</param>
        /// <param name="resonanzDrehzahl">The resonanz drehzahl.</param>
        /// <returns>Resonanzlänge in cm.</returns>
        public static double GetResonanzLaenge(double auslassSteuerwinkel, double abgasTemperatur, double resonanzDrehzahl)
        {
            // von mm in cm /10
            double resonanzlaenge = auslassSteuerwinkel * abgasTemperatur / (360 * 2 * resonanzDrehzahl / 60) * 1000;
            resonanzlaenge = Math.Round(resonanzlaenge, 2);

            return resonanzlaenge;
        }

        /// <summary>
        /// Berechnet wie lange der Fahrzeug-Kanal offen ist.
        /// </summary>
        /// <param name="auslassSteuerzeit">The auslass steuerzeit.</param>
        /// <param name="drehzahl">The drehzahl.</param>
        /// <returns>Zeit in s.</returns>
        public static double GetVehiclePortDuration(double auslassSteuerzeit, double drehzahl)
        {
            double zeit = 60 * auslassSteuerzeit / drehzahl * 360;

            return zeit;
        }
    }
}