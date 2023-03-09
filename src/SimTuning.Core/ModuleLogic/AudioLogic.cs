// Copyright (c) 2021 tuke productions. All rights reserved.
namespace SimTuning.Core.ModuleLogic
{
    using LiveChartsCore;
    using LiveChartsCore.Defaults;
    using LiveChartsCore.Kernel;
    using LiveChartsCore.Kernel.Sketches;
    using LiveChartsCore.SkiaSharpView;
    using NAudio.Wave;
    using SimTuning.Core.Models;
    using SkiaSharp;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnitsNet;

    /// <summary>
    /// AudioLogic.
    /// </summary>
    public static class AudioLogic
    {
        #region variables

        /// <summary>
        /// Gets or sets tODO: nutzer soll dies einstellen können. Math.Round(Math.Sqrt(Convert.ToDouble(AudioLogic.SpectrogramAudio.DisplaySettings.freqHigh
        /// - AudioLogic.SpectrogramAudio.DisplaySettings.freqLow)) / 2);.
        /// </summary>
        private static double _areaAbstand;

        private static string _audioFile;
        private static int _fftSize;
        private static double _intensity;
        private static int _maxFreq;
        private static int _minFreq;
        private static int _stepSize;
        private static int _targetWidthPx;
        private static SKBitmap bmp;
        // private static int sampleRate;

        /// <summary>
        /// Gets array Handling. Punkte der Drehzahl.
        /// </summary>
        public static List<DataPoint> AccelerationPoints { get; private set; }

        /// <summary>
        /// Gets array Handling.
        /// 1. Liste = Graphen, 2. Liste = Punkte.
        /// </summary>
        public static List<List<DataPoint>> AreaAccelerationPoints { get; private set; }

        public static List<List<DataPoint>> RotSpeedAccPoints { get; private set; }

        /// <summary>
        /// Gets the spectrogram audio.
        /// </summary>
        /// <value>The spectrogram audio.</value>
        public static Spectrogram.Spectrogram SpectrogramAudio { get; private set; }

        /// <summary>
        /// Gets the FFT beginn.
        /// </summary>
        // private static double FftBeginn { get => Math.Round(Convert.ToDouble(AudioLogic.SpectrogramAudio.FreqMin) / HzPerFFT); }

        ///// <summary>
        ///// Gets the FFT ende
        ///// </summary>
        // private static double FftEnde { get => Math.Round(Convert.ToDouble(AudioLogic.SpectrogramAudio.FreqMax) / HzPerFFT); }

        /// <summary>
        /// Gets fFT-Array-elements pro 1 Hertz.
        /// </summary>
        private static double HzPerFFT
        {
            get => AudioLogic.SpectrogramAudio.HzPerPx;
        }

        /// <summary>
        /// Gets wieviel reihen ein Herz wiederspiegeln. HORIZONTAL; anzahl spalten pro sekunde.
        /// </summary>
        private static double SegmentsPerSecond
        {
            get => AudioLogic.SpectrogramAudio.SecPerPx;
        }

        /// <summary>
        /// Gets spectrogram Data, Liste = Spalten, [] = Zeilen, float = Stärke der Frequenz.
        /// </summary>
        private static List<double[]> SpecData
        {
            get => AudioLogic.SpectrogramAudio?.GetFFTs();
        }

        #endregion variables

        /// <summary>
        /// Bildet aus den Spectrogram Daten X-Y Punkte.
        /// </summary>
        /// <param name="areas">if set to <c>true</c> [areas].</param>
        public static void GetDrehzahlGraph(bool areas = false, double intensity = 0.75, double areaAbstand = 8)
        {
            _intensity = intensity;
            _areaAbstand = areaAbstand;

            DefineHotPoints();

            if (areas)
            {
                PointsToAreas();

                foreach (var areaAccelerationPoint in AreaAccelerationPoints)
                    foreach (var accelerationPoint in areaAccelerationPoint)
                        accelerationPoint.ToRotionalSpeed();
            }
            else
            {
                foreach (var accelerationPoint in AccelerationPoints)
                    accelerationPoint.ToRotionalSpeed();
            }
        }

        /// <summary>
        /// Definieren des Frequenz-Spectrogram mit bestimmten Parametern.
        /// </summary>
        /// <param name="audioFile"></param>
        /// <param name="fftSize"></param>
        /// <param name="minFreq"></param>
        /// <param name="maxFreq"></param>
        /// <param name="intensity"></param>
        /// <param name="colormap"></param>
        /// <param name="targetWidthPx"></param>
        /// <returns>Bild des Spectrograms.</returns>
        public static SKBitmap GetSpectrogram(
            string audioFile,
            int fftSize = 16384,
            int minFreq = 25,
            int maxFreq = 250,
            double intensity = 0.75,
            int targetWidthPx = 1000)
        {
            _audioFile = audioFile;

            _intensity = intensity;

            _minFreq = minFreq;

            _maxFreq = maxFreq;

            _fftSize = fftSize;

            _targetWidthPx = targetWidthPx;

            Spectogram();

            return bmp;
        }

        /// <summary>
        /// Definiert die Punkte die Punkte die für eine Analyse verwertbar sind.
        /// </summary>
        private static void DefineHotPoints()
        {
            AccelerationPoints = new List<DataPoint>();
            List<DataPoint> hotDataPoints = new List<DataPoint>();

            for (int col = 0; col < SpecData.Count; col++)
            {
                List<DataPoint> hotColDataPoints = new List<DataPoint>();

                // nur für ausgewählten bereich
                for (int row = /*(int)FftBeginn*/0; row < /*FftEnde*/ SpecData[col].Length; row++)
                {
                    // Holen der intensivsten Punkte
                    double value = SpecData[col][row];
                    value *= _intensity;
                    value = Math.Min(value, 255);

                    if (value == 255)
                    {
                        // row speichern
                        hotColDataPoints.Add(new DataPoint(col, row));
                    }
                }

                // gespeicherte Punkte(wenn gefunden) der Liste hinzufügen
                if (hotColDataPoints.Count > 0)
                {
                    hotDataPoints.AddRange(hotColDataPoints);
                }
            }

            if (hotDataPoints.Count > 0)
            {
                AccelerationPoints.AddRange(hotDataPoints);
            }
        }

        /// <summary>
        /// Definieren von Bereichen aus Gesamtpunkten.
        /// </summary>
        private static void PointsToAreas()
        {
            AreaAccelerationPoints = new List<List<DataPoint>>();

            // erste area hinzufügen
            AreaAccelerationPoints.Add(new List<DataPoint>());
            // Dummy punkt hinzufügen
            AreaAccelerationPoints[0].Add(new DataPoint(0, 0));

            foreach (var point in AccelerationPoints)
            {
                // punkt vergleichen mit vorhandenen punkten in areas
                for (int area = 0; area < AccelerationPoints.Count; area++)
                {
                    // jeder punkt in area
                    foreach (var areaPoint in AreaAccelerationPoints[area])
                    {
                        // Horizontal
                        double differenz_horizontal = Math.Abs(point.X - areaPoint.X);

                        // Vertikal
                        double differenz_vertikal = Math.Abs(point.Y - areaPoint.Y);

                        // in vorhandene area hinzufügen
                        if (differenz_vertikal <= _areaAbstand && differenz_horizontal <= _areaAbstand)
                        {
                            AreaAccelerationPoints[area].Add(point);

                            goto nextPoint;
                        }
                        // wenn auch in letzter Area nichts gefunden werden konnte
                        else if (area == AreaAccelerationPoints.Count - 1)
                        {
                            // neue area beginnen
                            AreaAccelerationPoints.Add(new List<DataPoint>() { point });

                            goto nextPoint;
                        }
                    }
                }

                nextPoint:
                continue;
            }

            // Dummy punkt löschen
            AreaAccelerationPoints[0].Remove(AreaAccelerationPoints[0][0]);
        }

        /// <summary>
        /// Erstellt das Frequenz-Spectrogram.
        /// </summary>
        private static void Spectogram()
        {
            (int _sampleRate, double[] _audio) = Spectrogram.WavFile.ReadMono(_audioFile);

            int _stepSize = _audio.Length / _targetWidthPx;

            // load audio and process FFT
            SpectrogramAudio = new Spectrogram.Spectrogram(
                                                    sampleRate: _sampleRate,
                                                    fftSize: _fftSize,
                                                    stepSize: _stepSize,
                                                    minFreq: _minFreq,
                                                    maxFreq: _maxFreq);

            SpectrogramAudio.Add(_audio, true);
            bmp = SpectrogramAudio.GetBitmap(intensity: _intensity);
        }

        /// <summary>
        /// Umrechnen von Hertz in Drehzahl.
        /// </summary>
        private static DataPoint ToRotionalSpeed(this DataPoint DataPoint)
        {
            // Y(vertikal) = 1U/min; 1Hz = 60 U/min bei Einzylider Motoren X(horizontal) = 1s
            DataPoint converted = new DataPoint(
                Math.Round(DataPoint.X * SegmentsPerSecond, 4),
                Math.Round(DataPoint.Y * HzPerFFT * 60, 4));

            return converted;
        }
    }
}