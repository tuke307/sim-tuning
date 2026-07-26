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
            // Phase 6: the former ~460-LOC body is split into Calculate + ComputeGeometry
            // + four Draw helpers. Behavior is identical (same draw calls, same stage
            // branches); only the structure changed. SKPaint disposal is intentionally
            // left as-is (only the font is disposed) to match the historical behavior.
            Calculate(ref vehicle);
            AuspuffGeometry geo = ComputeGeometry(vehicle);

            SKBitmap bmp = new SKBitmap(geo.PicWidth, geo.PicHeight);
            SKCanvas canvas = new SKCanvas(bmp);

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

            DrawFramework(canvas, geo, redPen);
            DrawDimensionLines(canvas, geo, blackPen, blackDoubleArrowPen);
            DrawValueLabels(canvas, geo, labelFont, blackPen);
            DrawPartLabels(canvas, geo, labelFont, blackPen);

            return bmp;
        }

        /// <summary>
        /// Berechnet die Auspuffabmessungen und schreibt sie in das Model.
        /// </summary>
        /// <param name="vehicle">Das Vehicle, dessen Auspuff berechnet wird.</param>
        private static void Calculate(ref Data.Models.VehiclesModel vehicle)
        {
            vehicle.Motor.Auslass.Auspuff.ResonanzL = GetResonanzLaenge(vehicle.Motor.Auslass.SteuerzeitSZ.GetValueOrDefault(), vehicle.Motor.Auslass.Auspuff.AbgasT.GetValueOrDefault(), vehicle.Motor.ResonanzU.GetValueOrDefault());

            // KRÜMMER
            vehicle.Motor.Auslass.Auspuff.KruemmerD = vehicle.Motor.Auslass.DurchmesserD.GetValueOrDefault(); /*Get_KruemmerDurchmesser(vehicleflaeche);*/
            vehicle.Motor.Auslass.Auspuff.KruemmerL = GetKruemmerLaenge(vehicle.Motor.Auslass.DurchmesserD.GetValueOrDefault(), vehicle.Motor.Auslass.Auspuff.KruemmerF.GetValueOrDefault(), vehicle.Motor.Auslass.LaengeL.GetValueOrDefault());

            // MITTELTEIL
            vehicle.Motor.Auslass.Auspuff.MittelteilD = Math.Round(Math.Sqrt(vehicle.Motor.Auslass.FlaecheA.GetValueOrDefault() * 4 / Math.PI) * vehicle.Motor.Auslass.Auspuff.MittelteilF.GetValueOrDefault(), 2);

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
                    vehicle.Motor.Auslass.Auspuff.DiffusorD1 = Math.Round((2 * Math.Tan(vehicle.Motor.Auslass.Auspuff.KruemmerW.GetValueOrDefault() * Math.PI / 360) * vehicle.Motor.Auslass.Auspuff.KruemmerL.GetValueOrDefault()) + vehicle.Motor.Auslass.Auspuff.KruemmerD.GetValueOrDefault(), 2);
                    vehicle.Motor.Auslass.Auspuff.DiffusorL1 = Math.Round((vehicle.Motor.Auslass.Auspuff.MittelteilD.GetValueOrDefault() - vehicle.Motor.Auslass.Auspuff.DiffusorD1.GetValueOrDefault()) / (2 * Math.Tan(vehicle.Motor.Auslass.Auspuff.DiffusorW1.GetValueOrDefault() * (2 * Math.PI / 360))), 2);
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
            vehicle.Motor.Auslass.Auspuff.GegenkonusL = Math.Round((vehicle.Motor.Auslass.Auspuff.MittelteilD.GetValueOrDefault() - vehicle.Motor.Auslass.Auspuff.EndrohrD.GetValueOrDefault()) / (2 * Math.Tan(vehicle.Motor.Auslass.Auspuff.GegenKonusW.GetValueOrDefault() * Math.PI / 180)), 2);
            vehicle.Motor.Auslass.Auspuff.GegenkonusD = vehicle.Motor.Auslass.Auspuff.MittelteilD.GetValueOrDefault();

            // MITTELTEIL
            vehicle.Motor.Auslass.Auspuff.MittelteilL = Math.Round(vehicle.Motor.Auslass.Auspuff.ResonanzL.GetValueOrDefault() - (vehicle.Motor.Auslass.Auspuff.KruemmerL.GetValueOrDefault() + vehicle.Motor.Auslass.Auspuff.DiffusorL1.GetValueOrDefault() + vehicle.Motor.Auslass.Auspuff.GegenkonusL.GetValueOrDefault() / 2), 2);
        }

        /// <summary>
        /// Berechnet die Bildgeometrie (Teil-Längen/-Höhen, Startpositionen,
        /// Diffusor-Stufen) aus dem berechneten Model.
        /// </summary>
        /// <param name="vehicle">Das berechnete Vehicle.</param>
        /// <returns>Die Geometrie für das Zeichnen.</returns>
        private static AuspuffGeometry ComputeGeometry(Data.Models.VehiclesModel vehicle)
        {
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

            // Die Diffusor-/Krümmer-/...-Felder sind double?. Vor Phase 6 standen
            // Berechnung (Zuweisung dieser Felder aus nicht-null double-Rückgaben)
            // und dieser Cast im SELBEN Methodenrumpf, sodass die Nullable-Flow-
            // Analyse die Werte als "not null" einstufte und CS8629 unterdrückte.
            // Durch das Aufspalten in Calculate() + ComputeGeometry() geht diese
            // methodenübergreifende Analyse verloren — die Casts wären nun false
            // positives. Zur Laufzeit sind alle Felder garantiert nicht-null,
            // weil Auspuff() stets erst Calculate() aufruft. Daher hier lokal
            // unterdrückt (identische Cast-Ausdrücke wie zuvor => identisches IL).
#pragma warning disable CS8629
            // HORIZONTAL, länge der Teile
            int width1 = (int)vehicle.Motor.Auslass.Auspuff.KruemmerL;
            int width2 = (int)vehicle.Motor.Auslass.Auspuff.DiffusorL1;
            int width3 = (int)vehicle.Motor.Auslass.Auspuff.DiffusorL2;
            int width4 = (int)vehicle.Motor.Auslass.Auspuff.DiffusorL3;
            int width5 = (int)vehicle.Motor.Auslass.Auspuff.MittelteilL;
            int width6 = (int)vehicle.Motor.Auslass.Auspuff.GegenkonusL;
            int width7 = (int)(vehicle.Motor.Auslass.Auspuff.EndrohrL ?? 0);

            // VERTIKAL, (Anfangs)Höhe der Teile
            int height1 = (int)vehicle.Motor.Auslass.Auspuff.KruemmerD * 2;
            int height2 = (int)vehicle.Motor.Auslass.Auspuff.DiffusorD1 * 2;
            int height3 = (int)vehicle.Motor.Auslass.Auspuff.DiffusorD2 * 2;
            int height4 = (int)vehicle.Motor.Auslass.Auspuff.DiffusorD3 * 2;
            int height5 = (int)vehicle.Motor.Auslass.Auspuff.MittelteilD * 2;
            int height6 = (int)vehicle.Motor.Auslass.Auspuff.GegenkonusD * 2;
            int height7 = (int)(vehicle.Motor.Auslass.Auspuff.EndrohrD ?? 0) * 2;
#pragma warning restore CS8629

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

            // Diffusor-Stufen einmal berechnen (die ursprünglich dreifach
            // geschachtelten if/else-Zweige prüfen exakt diese Reihenfolge).
            bool hasD1 = vehicle.Motor.Auslass.Auspuff.DiffusorL1 != 0;
            bool hasD2 = hasD1 && vehicle.Motor.Auslass.Auspuff.DiffusorL2 != 0;
            bool hasD3 = hasD2 && vehicle.Motor.Auslass.Auspuff.DiffusorL3 != 0;

            return new AuspuffGeometry(
                pic_width, pic_height,
                width1, width2, width3, width4, width5, width6, width7,
                height1, height2, height3, height4, height5, height6, height7,
                xstart1, xstart2, xstart3, xstart4, xstart5, xstart6, xstart7,
                ymiddle,
                ydiff1, ydiff2, ydiff3, ydiff4, ydiff5, ydiff6, ydiff7,
                hasD1, hasD2, hasD3);
        }

        /// <summary>
        /// Zeichnet das Auspuffgerüst (Teilumrisse) gemäß Diffusor-Stufen.
        /// </summary>
        /// <param name="g">Die Zeichenfläche.</param>
        /// <param name="geo">Die Bildgeometrie.</param>
        /// <param name="redPen">Der Stift für die Umrisse.</param>
        private static void DrawFramework(SKCanvas g, in AuspuffGeometry geo, SKPaint redPen)
        {
            int width5 = geo.Width5;
            int width7 = geo.Width7;
            int height5 = geo.Height5;
            int height7 = geo.Height7;
            int xstart1 = geo.XStart1;
            int xstart2 = geo.XStart2;
            int xstart3 = geo.XStart3;
            int xstart4 = geo.XStart4;
            int xstart5 = geo.XStart5;
            int xstart6 = geo.XStart6;
            int xstart7 = geo.XStart7;
            int ymiddle = geo.YMiddle;
            int ydiff1 = geo.YDiff1;
            int ydiff2 = geo.YDiff2;
            int ydiff3 = geo.YDiff3;
            int ydiff4 = geo.YDiff4;
            int ydiff5 = geo.YDiff5;
            int ydiff6 = geo.YDiff6;
            int ydiff7 = geo.YDiff7;
            bool hasD1 = geo.HasD1;
            bool hasD2 = geo.HasD2;
            bool hasD3 = geo.HasD3;

            // 1
            g.DrawLine(
                new SKPoint(xstart1, ymiddle - ydiff1),
                new SKPoint(xstart2, ymiddle - ydiff2),
                redPen); // oben
            g.DrawLine(
                new SKPoint(xstart1, ymiddle + ydiff1),
                new SKPoint(xstart2, ymiddle + ydiff2),
                redPen); // unten
            g.DrawLine(
                new SKPoint(xstart1, ymiddle - ydiff1),
                new SKPoint(xstart1, ymiddle + ydiff1),
                redPen); // Links

            // ONE-Stage Diffusor
            if (hasD1)
            {
                // TWO-Stage Diffusor
                if (hasD2)
                {
                    // THREE-Stage Diffusor
                    if (hasD3)
                    {
                        // THREE-Stages malen
                        g.DrawLine(xstart4, ymiddle - ydiff4,
                                                 xstart5, ymiddle - ydiff5,
                                                 redPen); // oben
                        g.DrawLine(xstart4, ymiddle + ydiff4,
                                                 xstart5, ymiddle + ydiff5,
                                                 redPen); // unten
                        g.DrawLine(xstart4, ymiddle - ydiff4,
                                                 xstart4, ymiddle + ydiff4,
                                                 redPen); // links
                    }
                    else // TWO-Stages malen
                    {
                        g.DrawLine(
                            new SKPoint(xstart3, ymiddle - ydiff3),
                            new SKPoint(xstart4, ymiddle - ydiff4),
                            redPen); // oben
                        g.DrawLine(
                            new SKPoint(xstart3, ymiddle + ydiff3),
                            new SKPoint(xstart4, ymiddle + ydiff4),
                            redPen); // unten
                        g.DrawLine(
                            new SKPoint(xstart3, ymiddle - ydiff3),
                            new SKPoint(xstart3, ymiddle + ydiff3),
                            redPen); // links
                    }
                }
                else // ONE-Stage malen
                {
                    // 2
                    g.DrawLine(
                        new SKPoint(xstart2, ymiddle - ydiff2),
                        new SKPoint(xstart5, ymiddle - ydiff5),
                        redPen); // oben
                    g.DrawLine(
                        new SKPoint(xstart2, ymiddle + ydiff2),
                        new SKPoint(xstart5, ymiddle + ydiff5),
                        redPen); // unten
                    g.DrawLine(
                        new SKPoint(xstart2, ymiddle - ydiff2),
                        new SKPoint(xstart2, ymiddle + ydiff2),
                        redPen); // links
                }
            }

            // 5
            g.DrawRect(xstart5, ymiddle - ydiff5,
                                     width5, height5,
                                     redPen);
            // 6
            g.DrawLine(
                new SKPoint(xstart6, ymiddle - ydiff6),
                new SKPoint(xstart7, ymiddle - ydiff7),
                redPen); // oben
            g.DrawLine(
                new SKPoint(xstart6, ymiddle + ydiff6),
                new SKPoint(xstart7, ymiddle + ydiff7),
                redPen); // unten
            // 7
            g.DrawRect(xstart7, ymiddle - ydiff7,
                                     width7, height7,
                                     redPen);
        }

        /// <summary>
        /// Zeichnet die Maßlinien (Längen und Durchmesser).
        /// </summary>
        /// <param name="g">Die Zeichenfläche.</param>
        /// <param name="geo">Die Bildgeometrie.</param>
        /// <param name="blackPen">Der einfache schwarze Stift.</param>
        /// <param name="blackDoubleArrowPen">Der Pfeilstift.</param>
        private static void DrawDimensionLines(SKCanvas g, in AuspuffGeometry geo, SKPaint blackPen, SKPaint blackDoubleArrowPen)
        {
            int width7 = geo.Width7;
            int xstart1 = geo.XStart1;
            int xstart2 = geo.XStart2;
            int xstart3 = geo.XStart3;
            int xstart4 = geo.XStart4;
            int xstart5 = geo.XStart5;
            int xstart6 = geo.XStart6;
            int xstart7 = geo.XStart7;
            int ymiddle = geo.YMiddle;
            int ydiff1 = geo.YDiff1;
            int ydiff2 = geo.YDiff2;
            int ydiff3 = geo.YDiff3;
            int ydiff4 = geo.YDiff4;
            int ydiff5 = geo.YDiff5;
            int ydiff6 = geo.YDiff6;
            int ydiff7 = geo.YDiff7;
            bool hasD1 = geo.HasD1;
            bool hasD2 = geo.HasD2;
            bool hasD3 = geo.HasD3;

            /*
             * LÄNGEN
             */

            // 1
            g.DrawLine(xstart1, ymiddle + ydiff1,
                                     xstart1, ymiddle + 200,
                                     blackPen); // Begrenzung-Links
            g.DrawLine(xstart1, ymiddle + 175,
                                     xstart2, ymiddle + 175,
                                     blackDoubleArrowPen); // Maß-Linie

            // ONE-Stage Diffusor
            if (hasD1)
            {
                // TWO-Stage Diffusor
                if (hasD2)
                {
                    // THREE-Stage Diffusor
                    if (hasD3)
                    {
                        // 2
                        g.DrawLine(xstart2, ymiddle + ydiff2,
                                                 xstart2, ymiddle + 200,
                                                 blackPen); // Begrenzung-links
                        g.DrawLine(xstart2, ymiddle + 175,
                                                 xstart3, ymiddle + 175,
                                                 blackDoubleArrowPen); // Maß-Linie

                        // 3
                        g.DrawLine(xstart4, ymiddle + ydiff3,
                                                 xstart4, ymiddle + 200,
                                                 blackPen); // Begrenzung-Mitte
                        g.DrawLine(xstart4, ymiddle + 175,
                                                 xstart5, ymiddle + 175,
                                                 blackDoubleArrowPen); // Maß-Linie
                        g.DrawLine(xstart4, ymiddle + 175,
                                                 xstart5, ymiddle + 175,
                                                 blackDoubleArrowPen); // Maß-Linie

                        // 4
                        g.DrawLine(xstart6, ymiddle + ydiff4,
                                                 xstart6, ymiddle + 200,
                                                 blackPen); // Begrenzung-Mitte
                        g.DrawLine(xstart4, ymiddle + 175,
                                                 xstart5, ymiddle + 175,
                                                 blackDoubleArrowPen); // Maß-Linie
                    }
                    else
                    {
                        // 2
                        g.DrawLine(xstart2, ymiddle + ydiff2,
                                                 xstart2, ymiddle + 200,
                                                 blackPen); // Begrenzung-links
                        g.DrawLine(xstart2, ymiddle + 175,
                                                 xstart3, ymiddle + 175,
                                                 blackDoubleArrowPen); // Maß-Linie

                        // 3
                        g.DrawLine(xstart4, ymiddle + ydiff3,
                                                 xstart4, ymiddle + 200,
                                                 blackPen); // Begrenzung-Mitte
                        g.DrawLine(xstart4, ymiddle + 175,
                                                 xstart5, ymiddle + 175,
                                                 blackDoubleArrowPen); // Maß-Linie
                        g.DrawLine(xstart4, ymiddle + 175,
                                                 xstart5, ymiddle + 175,
                                                 blackDoubleArrowPen); // Maß-Linie
                    }
                }
                else
                {
                    // 2
                    g.DrawLine(xstart2, ymiddle + ydiff2,
                                             xstart2, ymiddle + 200,
                                             blackPen); // Begrenzung-links
                    g.DrawLine(xstart2, ymiddle + 175,
                                             xstart3, ymiddle + 175,
                                             blackDoubleArrowPen); // Maß-Linie
                }
            }

            // 5
            g.DrawLine(xstart5, ymiddle + ydiff5,
                                     xstart5, ymiddle + 200,
                                     blackPen); // Begrenzung-Links
            g.DrawLine(xstart5, ymiddle + 175,
                                     xstart6, ymiddle + 175,
                                     blackDoubleArrowPen); // Maß-Linie
            // 6
            g.DrawLine(xstart6, ymiddle + ydiff6,
                                     xstart6, ymiddle + 200,
                                     blackPen); // Begrenzung-Links
            g.DrawLine(xstart6, ymiddle + 175,
                                     xstart7, ymiddle + 175,
                                     blackDoubleArrowPen); // Maß-Linie
            // 7
            g.DrawLine(xstart7, ymiddle + ydiff7,
                                     xstart7, ymiddle + 200,
                                     blackPen); // Begrenzung-Links
            g.DrawLine(xstart7 + width7, ymiddle + ydiff7,
                                     xstart7 + width7, ymiddle + 200,
                                     blackPen); // Begrenzung-Rechts
            g.DrawLine(xstart7, ymiddle + 175,
                                     xstart7 + width7, ymiddle + 175,
                                     blackDoubleArrowPen); // Maß-Linie
            // konus
            g.DrawLine(xstart2, ymiddle + 175,
                                     xstart5, ymiddle + 175,
                                     blackDoubleArrowPen); // Maß-Linie

            /*
             * Durchmesser
             */
            // Header
            g.DrawLine(xstart1, ymiddle - ydiff1,
                                     xstart1 - 50, ymiddle - ydiff1,
                                     blackPen); // Begrenzung-oben
            g.DrawLine(xstart1, ymiddle + ydiff1,
                                     xstart1 - 50, ymiddle + ydiff1,
                                     blackPen); // Begrenzung-unten
            g.DrawLine(xstart1 - 35, ymiddle - ydiff1,
                                     xstart1 - 35, ymiddle + ydiff1,
                                     blackDoubleArrowPen); // Maß-Linie
            // Stringer
            g.DrawLine(xstart7 + width7, ymiddle - ydiff7,
                                     xstart7 + width7 + 50, ymiddle - ydiff7,
                                     blackPen); // Begrenzung-oben
            g.DrawLine(xstart7 + width7, ymiddle + ydiff7,
                                     xstart7 + width7 + 50, ymiddle + ydiff7,
                                     blackPen); // Begrenzung-unten
            g.DrawLine(xstart7 + width7 + 35, ymiddle - ydiff7,
                                     xstart7 + width7 + 35, ymiddle + ydiff7,
                                     blackDoubleArrowPen); // Maß-Linie
        }

        /// <summary>
        /// Zeichnet die Wertbezeichnungen (LK/LD/LM/LG/LE, D1/D2).
        /// </summary>
        /// <param name="g">Die Zeichenfläche.</param>
        /// <param name="geo">Die Bildgeometrie.</param>
        /// <param name="labelFont">Die Schrift.</param>
        /// <param name="blackPen">Der schwarze Stift.</param>
        private static void DrawValueLabels(SKCanvas g, in AuspuffGeometry geo, SKFont labelFont, SKPaint blackPen)
        {
            int width1 = geo.Width1;
            int width2 = geo.Width2;
            int width3 = geo.Width3;
            int width4 = geo.Width4;
            int width5 = geo.Width5;
            int width6 = geo.Width6;
            int width7 = geo.Width7;
            int xstart1 = geo.XStart1;
            int xstart2 = geo.XStart2;
            int xstart3 = geo.XStart3;
            int xstart4 = geo.XStart4;
            int xstart5 = geo.XStart5;
            int xstart6 = geo.XStart6;
            int xstart7 = geo.XStart7;
            int ymiddle = geo.YMiddle;
            bool hasD1 = geo.HasD1;
            bool hasD2 = geo.HasD2;
            bool hasD3 = geo.HasD3;

            /*
             * übergebene Werte werden genommen da genauer
             */

            // Längen
            g.DrawText(/*kruemmerL.ToString()*/"LK", xstart1 + (width1 / 2) - 15, ymiddle + 200, SKTextAlign.Left, labelFont, blackPen);

            // ONE-Stage Diffusor
            if (hasD1)
            {
                // TWO-Stage Diffusor
                if (hasD2)
                {
                    // THREE-Stage Diffusor
                    if (hasD3)
                    {
                        g.DrawText(/*konus1L.ToString()*/"LD1", xstart2 + (width2 / 2) - 15, ymiddle + 150, SKTextAlign.Left, labelFont, blackPen);
                        g.DrawText(/*konus2L.ToString()*/"LD2", xstart3 + (width3 / 2) - 15, ymiddle + 150, SKTextAlign.Left, labelFont, blackPen);
                        g.DrawText(/*konus3L.ToString()*/"LD3", xstart4 + (width4 / 2) - 15, ymiddle + 150, SKTextAlign.Left, labelFont, blackPen);
                    }
                    else
                    {
                        g.DrawText(/*konus1L.ToString()*/"LD1", xstart2 + (width2 / 2) - 15, ymiddle + 150, SKTextAlign.Left, labelFont, blackPen);
                        g.DrawText(/*konus2L.ToString()*/"LD2", xstart3 + (width3 / 2) - 15, ymiddle + 150, SKTextAlign.Left, labelFont, blackPen);
                    }
                }
                else
                {
                    g.DrawText(/*konus1L.ToString()*/"LD", xstart2 + (width2 / 2) - 15, ymiddle + 200, SKTextAlign.Left, labelFont, blackPen);
                }
            }

            g.DrawText(/*mittelteilL.ToString()*/"LM", xstart5 + (width5 / 2) - 15, ymiddle + 200, SKTextAlign.Left, labelFont, blackPen);
            g.DrawText(/*gegenkonusL.ToString()*/"LG", xstart6 + (width6 / 2) - 15, ymiddle + 200, SKTextAlign.Left, labelFont, blackPen);
            g.DrawText(/*endrohrL.ToString()*/"LE", xstart7 + (width7 / 2) - 15, ymiddle + 200, SKTextAlign.Left, labelFont, blackPen);

            // Durchmesser
            g.DrawText(
                "D1",
                xstart1 - 90, ymiddle - 15,
                SKTextAlign.Left, labelFont, blackPen);
            g.DrawText(
                "D2",
                xstart7 + width7 + 65, ymiddle - 15,
                SKTextAlign.Left, labelFont, blackPen);
            // g.DrawString("D3", drawFont, drawBrushblack, Xstart7 + width7
            // + 65, Ymiddle - 15); g.DrawString("D4", drawFont,
            // drawBrushblack, Xstart7 + width7 + 65, Ymiddle - 15);
        }

        /// <summary>
        /// Zeichnet die Teilbezeichnungen (Krümmer/Konus/Mittelteil/...).
        /// </summary>
        /// <param name="g">Die Zeichenfläche.</param>
        /// <param name="geo">Die Bildgeometrie.</param>
        /// <param name="labelFont">Die Schrift.</param>
        /// <param name="blackPen">Der schwarze Stift.</param>
        private static void DrawPartLabels(SKCanvas g, in AuspuffGeometry geo, SKFont labelFont, SKPaint blackPen)
        {
            int xstart1 = geo.XStart1;
            int xstart2 = geo.XStart2;
            int xstart5 = geo.XStart5;
            int xstart6 = geo.XStart6;
            int xstart7 = geo.XStart7;
            int ymiddle = geo.YMiddle;

            g.DrawText(
                "Krümmer",
                xstart1 + 20, ymiddle,
                SKTextAlign.Left, labelFont, blackPen);
            g.DrawText(
                "Konus",
                xstart2 + 20, ymiddle,
                SKTextAlign.Left, labelFont, blackPen);
            g.DrawText(
                "Mittelteil",
                xstart5 + 20, ymiddle,
                SKTextAlign.Left, labelFont, blackPen);
            g.DrawText(
                "Gegenkonus",
                xstart6 + 20, ymiddle,
                SKTextAlign.Left, labelFont, blackPen);
            g.DrawText(
                "Endrohr",
                xstart7 + 20, ymiddle,
                SKTextAlign.Left, labelFont, blackPen);
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

        /// <summary>
        /// Bildgeometrie für die Auspuffvisualisierung. Positional, damit die
        /// Draw-Helfer die Felder originalsgetreu an lokale Variablen binden können.
        /// </summary>
#pragma warning disable SA1313 // Parameter names are the public property names of a positional record; PascalCase is intentional.
        private readonly record struct AuspuffGeometry(
            int PicWidth,
            int PicHeight,
            int Width1,
            int Width2,
            int Width3,
            int Width4,
            int Width5,
            int Width6,
            int Width7,
            int Height1,
            int Height2,
            int Height3,
            int Height4,
            int Height5,
            int Height6,
            int Height7,
            int XStart1,
            int XStart2,
            int XStart3,
            int XStart4,
            int XStart5,
            int XStart6,
            int XStart7,
            int YMiddle,
            int YDiff1,
            int YDiff2,
            int YDiff3,
            int YDiff4,
            int YDiff5,
            int YDiff6,
            int YDiff7,
            bool HasD1,
            bool HasD2,
            bool HasD3);
#pragma warning restore SA1313
    }
}
