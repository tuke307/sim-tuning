// Copyright (c) 2025 tuke productions. All rights reserved.
using SimTuning.Core.Helpers;
using SimTuning.Core.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace SimTuning.Core.ModuleLogic
{
    /// <summary>
    /// Motor Logik.
    /// </summary>
    public static class EngineLogic
    {
        /// <summary>
        /// Berechnet die Kompression.
        /// </summary>
        /// <param name="hubraum">The hubraum.</param>
        /// <param name="brennraum">The brennraum.</param>
        /// <param name="durchmesser">The durchmesser.</param>
        /// <returns></returns>
        public static double GetCompression(double hubraum, double brennraum, double durchmesser)
        {
            double verdichtung = (hubraum + brennraum) / brennraum;
            verdichtung = Math.Round(verdichtung, 1);

            return verdichtung;
        }

        /// <summary>
        /// Berechnet den Bohrungs-Durchmesser des Zylinders.
        /// </summary>
        /// <param name="hubraum">The hubraum.</param>
        /// <param name="hub">The hub.</param>
        /// <returns>Bohrung in mm.</returns>
        public static double GetCylinderHoleDiameter(double hubraum, double hub)
        {
            double durchmesser = 0;

            durchmesser = 2 * Math.Sqrt(hubraum / hub) / Math.Sqrt(Math.PI);
            durchmesser = Math.Round(durchmesser, 2);

            return durchmesser;
        }

        /// <summary>
        /// Berechnet den Hubraum des Zylinders.
        /// </summary>
        /// <param name="bohrungsdurchmesser">The bohrungsdurchmesser.</param>
        /// <param name="hub">The hub.</param>
        /// <returns>Hubraum in ccm.</returns>
        public static double GetDisplacement(double bohrungsdurchmesser, double hub)
        {
            double hubraum = Math.PI * bohrungsdurchmesser / 4 * hub;
            hubraum = Math.Round(hubraum, 1);

            return hubraum;
        }

        /// <summary>
        /// Berechnet den Abstand des Kolbens in einer bestimmten Position(Steuerwinkel)
        /// zum OT(Zylinderoberkante).
        /// </summary>
        /// <param name="pleullaenge">The pleullaenge.</param>
        /// <param name="hubradius">The hubradius.</param>
        /// <param name="deachsierung">The deachsierung.</param>
        /// <param name="kwgrad">The kwgrad.</param>
        /// <returns>Länge vor OT in mm.</returns>
        public static double GetDistanceToOT(double pleullaenge, double hubradius, double deachsierung, double kwgrad)
        {
            // übergebene Gradmaß in Bogenmaß umrechnen
            kwgrad = kwgrad / 180 * Math.PI * 1;

            double mmvorot = Math.Sqrt(Math.Pow(pleullaenge + hubradius, 2) - Math.Pow(deachsierung, 2)) -
                      ((hubradius *
                      Math.Cos(Math.Asin(deachsierung / (pleullaenge + hubradius)) + kwgrad)) +
                      Math.Sqrt(
                      Math.Pow(pleullaenge, 2) -
                      Math.Pow((Math.Sin(Math.Asin(deachsierung / (pleullaenge + hubradius)) + kwgrad) * hubradius) - deachsierung, 2)));

            mmvorot = Math.Round(mmvorot, 2);

            return mmvorot;
        }

        /// <summary>
        /// Berechnet das Einbauspiel.
        /// </summary>
        /// <param name="diameter">The diameter.</param>
        /// <returns>die Einbauspiele.</returns>
        public static GrindingDiametersModel GetGrindingDiameters(double diameter)
        {
            GrindingDiametersModel grindingDiametersModel = new GrindingDiametersModel();

            grindingDiametersModel.Diameter1 = diameter + 0.2;
            grindingDiametersModel.Diameter2 = diameter + 0.4;
            grindingDiametersModel.Diameter3 = diameter + 0.6;
            grindingDiametersModel.Diameter4 = diameter + 0.8;

            return grindingDiametersModel;
        }

        /// <summary>
        /// Berechnet den halben Radius.
        /// </summary>
        /// <param name="hub">The hub.</param>
        /// <param name="pleullaenge">The pleullaenge.</param>
        /// <param name="deachsierung">The deachsierung.</param>
        /// <returns>Hubradius in mm.</returns>
        public static double GetHubRadius(double hub, double pleullaenge, double deachsierung)
        {
            double hubradius = hub *
                        Math.Sqrt(Math.Abs(Math.Pow(4 * deachsierung, 2) + Math.Pow(hub, 2) - (4 * Math.Pow(pleullaenge, 2)))) /
                        Math.Sqrt(Math.Abs((4 * Math.Pow(hub, 2)) - (16 * Math.Pow(pleullaenge, 2))));

            hubradius = Math.Round(hubradius, 2);

            return hubradius;
        }

        /// <summary>
        /// Berechnet den Kolben-Durchmesser.
        /// </summary>
        /// <param name="bohrungsdurchmesser">The bohrungsdurchmesser.</param>
        /// <param name="einbauspiel">The einbauspiel.</param>
        /// <returns>Kolbendurchmesser in mm.</returns>
        public static double GetKolbenDurchmesser(double bohrungsdurchmesser, double einbauspiel)
        {
            double durchmesser = 0;

            durchmesser = bohrungsdurchmesser - (einbauspiel / 100);

            return durchmesser;
        }

        /// <summary>
        /// Berechnet die Kolbengeschwindigkeit.
        /// </summary>
        /// <param name="hub">The hub.</param>
        /// <param name="drehzahl">The drehzahl.</param>
        /// <returns>Kolbengeschwindigkeit in m/s.</returns>
        public static double GetKolbenGeschwindigkeit(double hub, double drehzahl)
        {
            double kolbengeschwindigkeit = 0;

            kolbengeschwindigkeit = hub * drehzahl / 30;
            kolbengeschwindigkeit = Math.Round(kolbengeschwindigkeit, 2);

            return kolbengeschwindigkeit;
        }

        /// <summary>
        /// Berechnet die Differenz von zwei Steuerwinkeln .
        /// </summary>
        /// <param name="inmm">Rückgabeeinheit.</param>
        /// <param name="vorher">The vorher.</param>
        /// <param name="nachher">The nachher.</param>
        /// <param name="pleullaenge">The pleullaenge.</param>
        /// <param name="hubradius">The hubradius.</param>
        /// <param name="deachsierung">The deachsierung.</param>
        /// <returns>Differenz in Grad oder mm.</returns>
        public static double GetPortTimingDifference(bool inmm, double vorher, double nachher, double pleullaenge = 0, double hubradius = 0, double deachsierung = 0)
        {
            // umrechnen in mm
            if (inmm)
            {
                vorher = GetDistanceToOT(pleullaenge, hubradius, deachsierung, vorher);
                nachher = GetDistanceToOT(pleullaenge, hubradius, deachsierung, nachher);
            }

            double differenz = nachher - vorher;
            differenz = Math.Abs(differenz);

            differenz = Math.Round(differenz, 2);

            return differenz;
        }

        /// <summary>
        /// Zeichnet das Steuerdiagramm.
        /// </summary>
        /// <param name="einlass">Einlass-Steuerzeit.</param>
        /// <param name="auslass">Auslass-Steuerzeit.</param>
        /// <param name="ueberstroemer">Überstroemer-Steuerzeit.</param>
        /// <returns>Bild des Steuerdiagramms.</returns>
        public static SKBitmap GetSteuerdiagramm(double einlass, double auslass, double ueberstroemer)
        {
            int radMaß = 500; // quadratisch
            int radius = radMaß / 2;
            int rand = 50; // zu jeder seite hin
            int mitte = (500 + 50 + 50) / 2;

            SKBitmap bmp_rad = new SKBitmap(radMaß + (rand * 2), radMaß + (rand * 2));
            SKCanvas graphic_rad = new SKCanvas(bmp_rad);
            graphic_rad.Clear();
            // SKBitmap bmp_complet = new SKBitmap(radMaß + 100, radMaß + 100); SKCanvas
            // graphic_complet = new SKCanvas(bmp_complet);

            SKPaint blackPen = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                StrokeWidth = 4,
                IsAntialias = true,
                TextSize = 22,
            };
            SKPaint redPen = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.DarkRed,
                StrokeWidth = 4,
                IsAntialias = true,
            };
            SKPaint bluePen = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.DarkBlue,
                StrokeWidth = 4,
                IsAntialias = true,
            };

            SKPaint greenPen = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.DarkGreen,
                StrokeWidth = 4,
                IsAntialias = true,
            };

            SKPoint koordinaten;

            // Steuerwinkel Kreis
            graphic_rad.DrawCircle(
                new SKPoint(mitte, mitte),
                radius,
                blackPen);

            // Dash auf schwarzen pinsel erzeugen
            blackPen.PathEffect = SKPathEffect.CreateDash(new float[] { 5, 5 }, 5);
            blackPen.StrokeWidth = 3;

            // UT und OT
            graphic_rad.DrawLine(
                new SKPoint(mitte, 0),
                new SKPoint(mitte, bmp_rad.Height), blackPen);

            // Hilfslinien 270° und 90°
            graphic_rad.DrawLine(
                new SKPoint(0, mitte),
                new SKPoint(bmp_rad.Width, mitte), blackPen);

            // Einlass öffnet
            koordinaten = Functions.GetPointOnCircle(GetSteuerwinkelOeffnet(steuerzeitEinlass: einlass), radius);
            graphic_rad.DrawLine(
                new SKPoint(mitte, mitte),
                new SKPoint(koordinaten.X + rand, koordinaten.Y + rand),
                redPen);

            // Einlass schließt
            koordinaten = Functions.GetPointOnCircle(GetSteuerwinkelSchließt(steuerzeitEinlass: einlass), radius);
            graphic_rad.DrawLine(
                new SKPoint(mitte, mitte),
                new SKPoint(koordinaten.X + rand, koordinaten.Y + rand),
                redPen);

            // Auslass öffnet
            koordinaten = Functions.GetPointOnCircle(GetSteuerwinkelOeffnet(steuerzeitAuslass: auslass), radius);
            graphic_rad.DrawLine(
                new SKPoint(mitte, mitte),
                new SKPoint(koordinaten.X + rand, koordinaten.Y + rand),
                bluePen);

            // Auslass schließt
            koordinaten = Functions.GetPointOnCircle(GetSteuerwinkelSchließt(steuerzeitAuslass: auslass), radius);
            graphic_rad.DrawLine(
                new SKPoint(mitte, mitte),
                new SKPoint(koordinaten.X + rand, koordinaten.Y + rand),
                bluePen);

            // Überströmer öffnet
            koordinaten = Functions.GetPointOnCircle(GetSteuerwinkelOeffnet(steuerzeitUeberstroemer: ueberstroemer), radius);
            graphic_rad.DrawLine(
                new SKPoint(mitte, mitte),
                new SKPoint(koordinaten.X + rand, koordinaten.Y + rand),
                greenPen);

            // Überströmer schließt
            koordinaten = Functions.GetPointOnCircle(GetSteuerwinkelSchließt(steuerzeitUeberstroemer: ueberstroemer), radius);
            graphic_rad.DrawLine(
                new SKPoint(mitte, mitte),
                new SKPoint(koordinaten.X + rand, koordinaten.Y + rand),
                greenPen);

            // drehen
            Functions.RotateBitmap(bmp_rad, 270).CopyTo(bmp_rad);

            // gedrehete Bitmap erneut in Canvas einfügen
            graphic_rad = new SKCanvas(bmp_rad);

            // Textbezeichnung hinzufügen
            blackPen.PathEffect = null;
            graphic_rad.DrawText("OT", mitte + 10, 30, blackPen);
            graphic_rad.DrawText("UT", mitte - 40, 580, blackPen);

            return bmp_rad;
        }

        /// <summary>
        /// Berechnet die Steuerwinkel.
        /// </summary>
        /// <param name="vorherSteuerzeit">The vorher steuerzeit.</param>
        /// <param name="nachherSteuerzeit">The nachher steuerzeit.</param>
        /// <param name="kolbenoberkante">if set to <c>true</c> [kolbenoberkante].</param>
        /// <param name="kolbenunterkante">
        /// if set to <c>true</c> [kolbenunterkante].
        /// </param>
        public static (double, double, double, double) GetSteuerwinkel(double vorherSteuerzeit, double nachherSteuerzeit, bool kolbenoberkante, bool kolbenunterkante)
        {
            double steuerwinkelOeffnetVorher = 0;
            double steuerwinkelSchließtVorher = 0;
            double steuerwinkelOeffnetNachher = 0;
            double steuerwinkelSchließtNachher = 0;

            // auslass, überströmer
            if (kolbenunterkante)
            {
                // öffnet vorher
                steuerwinkelOeffnetVorher = 180 - (vorherSteuerzeit / 2);

                // schließt vorher
                steuerwinkelSchließtVorher = 180 + (vorherSteuerzeit / 2);

                // öffnet vorher
                steuerwinkelOeffnetNachher = 180 - (nachherSteuerzeit / 2);

                // schließt vorher
                steuerwinkelSchließtNachher = 180 + (nachherSteuerzeit / 2);
            }
            // einlass
            else if (kolbenoberkante)
            {
                // öffnet nachher
                steuerwinkelOeffnetVorher = 360 - (vorherSteuerzeit / 2);

                // schließt nachher
                steuerwinkelSchließtVorher = 0 + (vorherSteuerzeit / 2);

                // öffnet nachher
                steuerwinkelOeffnetNachher = 360 - (nachherSteuerzeit / 2);

                // schließt nachher
                steuerwinkelSchließtNachher = 0 + (nachherSteuerzeit / 2);
            }

            return (steuerwinkelOeffnetVorher, steuerwinkelSchließtVorher, steuerwinkelOeffnetNachher, steuerwinkelSchließtNachher);
        }

        /// <summary>
        /// Berechnet den Öffnungs-Steuerwinkel.
        /// </summary>
        /// <param name="steuerzeitEinlass">Einlass Steuerzeit.</param>
        /// <param name="steuerzeitAuslass">Auslass steuerzeit.</param>
        /// <param name="steuerzeitUeberstroemer">Überströmer Steuerzeit.</param>
        /// <returns>Steuerwinkel.</returns>
        public static double GetSteuerwinkelOeffnet(double steuerzeitEinlass = 0, double steuerzeitAuslass = 0, double steuerzeitUeberstroemer = 0)
        {
            double steuerwinkel_oeffnet = 0;

            if (steuerzeitEinlass != 0)
            {
                steuerwinkel_oeffnet = 360 - (steuerzeitEinlass / 2);
            }
            else if (steuerzeitAuslass != 0)
            {
                steuerwinkel_oeffnet = 180 - (steuerzeitAuslass / 2);
            }
            else if (steuerzeitUeberstroemer != 0)
            {
                steuerwinkel_oeffnet = 180 - (steuerzeitUeberstroemer / 2);
            }

            steuerwinkel_oeffnet = Math.Round(steuerwinkel_oeffnet, 2);

            return steuerwinkel_oeffnet;
        }

        /// <summary>
        /// Berechnet den Schließungs-Steuerwinkel.
        /// </summary>
        /// <param name="steuerzeitEinlass">The steuerzeit einlass.</param>
        /// <param name="steuerzeitAuslass">The steuerzeit auslass.</param>
        /// <param name="steuerzeitUeberstroemer">The steuerzeit ueberstroemer.</param>
        /// <returns></returns>
        public static double GetSteuerwinkelSchließt(double steuerzeitEinlass = 0, double steuerzeitAuslass = 0, double steuerzeitUeberstroemer = 0)
        {
            double steuerwinkel_schließt = 0;

            if (steuerzeitEinlass != 0)
            {
                steuerwinkel_schließt = steuerzeitEinlass / 2;
            }
            else if (steuerzeitAuslass != 0)
            {
                steuerwinkel_schließt = 180 + (steuerzeitAuslass / 2);
            }
            else if (steuerzeitUeberstroemer != 0)
            {
                steuerwinkel_schließt = 180 + (steuerzeitUeberstroemer / 2);
            }

            steuerwinkel_schließt = Math.Round(steuerwinkel_schließt, 2);

            return steuerwinkel_schließt;
        }

        /// <summary>
        /// Berechnet den abzunehmenden Bereich am Zylinderkopf.
        /// </summary>
        /// <param name="hubraum">Hubraum.</param>
        /// <param name="brennraum">Brennraum.</param>
        /// <param name="durchmesser">Zylinder-Durchmesser.</param>
        /// <param name="zielVerdichtung">Zielverdichtung.</param>
        /// <returns>abzunehmender Bereich in mm.</returns>
        public static double GetToDecreasingLength(double hubraum, double brennraum, double durchmesser, double zielVerdichtung)
        {
            double abdrehen_mm = 4 * ((brennraum * zielVerdichtung) - brennraum - hubraum) / (Math.PI * ((Math.Pow(durchmesser, 2) * zielVerdichtung) - Math.Pow(durchmesser, 2)));
            abdrehen_mm = Math.Round(abdrehen_mm, 2);

            return abdrehen_mm;
        }

        /// <summary>
        /// Vorauslasses the specified steuerwinkel auslass.
        /// </summary>
        /// <param name="auslassSW">The steuerwinkel auslass.</param>
        /// <param name="ueberstroemerSW">The steuerwinkel ueberstroemer.</param>
        /// <returns>Vorauslass in °KW.</returns>
        public static double GetVorauslass(double auslassSW, double ueberstroemerSW)
        {
            double vorauslass = (auslassSW - ueberstroemerSW) / 2;

            vorauslass = Math.Round(vorauslass, 2);

            return vorauslass;
        }
    }
}