// Copyright (c) 2021 tuke productions. All rights reserved.
namespace SimTuning.Core.ModuleLogic
{
    using LiveChartsCore;
    using NAudio.Wave;
    using SkiaSharp;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// AudioLogic.
    /// </summary>
    public static class AudioLogic
    {
        #region variables

        /// <summary>
        /// Gets or sets tODO: nutzer soll dies einstellen können.
        /// Math.Round(Math.Sqrt(Convert.ToDouble(AudioLogic.SpectrogramAudio.DisplaySettings.freqHigh
        /// - AudioLogic.SpectrogramAudio.DisplaySettings.freqLow)) / 2);.
        /// </summary>
        private static double _areaAbstand;

        private static string _audioFile;
        private static Spectrogram.Colormap _colormap;
        private static int _fftSize;
        private static double _intensity;
        private static int _maxFreq;
        private static int _minFreq;
        private static int _stepSize;
        private static int _targetWidthPx;
        private static SKBitmap bmp;
        // private static int sampleRate;

        /// <summary>
        /// Gets array Handling.
        /// 1. Liste = Graphen, 2. Liste = Punkte.
        /// </summary>
        public static List<List<ISeries>> DrehzahlPoints { get; private set; }

        /// <summary>
        /// Gets the spectrogram audio.
        /// </summary>
        /// <value>The spectrogram audio.</value>
        public static Spectrogram.Spectrogram SpectrogramAudio { get; private set; }

        /// <summary>
        /// Gets the FFT beginn.
        /// </summary>
        // private static double FftBeginn { get =>
        // Math.Round(Convert.ToDouble(AudioLogic.SpectrogramAudio.FreqMin) / HzPerFFT); }

        ///// <summary>
        ///// Gets the FFT ende
        ///// </summary>
        // private static double FftEnde { get =>
        // Math.Round(Convert.ToDouble(AudioLogic.SpectrogramAudio.FreqMax) / HzPerFFT); }

        /// <summary>
        /// Gets fFT-Array-elements pro 1 Hertz.
        /// </summary>
        private static double HzPerFFT
        {
            get => AudioLogic.SpectrogramAudio.HzPerPx;
        }

        /// <summary>
        /// Gets wieviel reihen ein Herz wiederspiegeln. HORIZONTAL; anzahl spalten pro
        /// sekunde.
        /// </summary>
        private static double SegmentsPerSecond
        {
            get => AudioLogic.SpectrogramAudio.SecPerPx;
        }

        /// <summary>
        /// Gets spectrogram Data, Liste = Spalten, [] = Zeilen, float = Stärke der
        /// Frequenz.
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

            HotPoints();

            if (areas)
            {
                PointsToAreas();
            }

            PointsToRotionalSpeed();
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
            Spectrogram.Colormap colormap = Spectrogram.Colormap.viridis,
            int targetWidthPx = 1000)
        {
            _audioFile = audioFile;

            _intensity = intensity;

            _minFreq = minFreq;

            _maxFreq = maxFreq;

            _colormap = colormap;

            _fftSize = fftSize;

            _targetWidthPx = targetWidthPx;

            Spectogram();

            return bmp;
        }

        /// <summary>
        /// Definiert die Punkte die Punkte die für eine Analyse verwertbar sind.
        /// </summary>
        private static void HotPoints()
        {
            DrehzahlPoints = new List<List<ISeries>>();
            List<ISeries> temp_data = new List<ISeries>();

            for (int col = 0; col < SpecData.Count; col++)
            {
                // nur für ausgewählten bereich
                List<ISeries> temp_points = new List<ISeries>();

                for (int row = /*(int)FftBeginn*/0; row < /*FftEnde*/ SpecData[col].Length; row++)
                {
                    // Holen der intensivsten Punkte
                    double value = SpecData[col][row];
                    value *= _intensity;
                    value = Math.Min(value, 255);

                    if (value == 255)
                    {
                        // row speichern
                        //temp_points.Add(new ISeries(col, row));
                    }
                }

                // gespeicherte Punkte(wenn gefunden) der Liste hinzufügen
                if (temp_points.Count > 0)
                {
                    temp_data.AddRange(temp_points);
                }
            }

            if (temp_data.Count > 0)
            {
                DrehzahlPoints.Add(temp_data);
            }
        }

        /// <summary>
        /// Definieren von Bereichen aus Gesamtpunkten.
        /// </summary>
        private static void PointsToAreas()
        {
            List<ISeries> unfiltered_data = new List<ISeries>();
            unfiltered_data.AddRange(DrehzahlPoints[0]);
            DrehzahlPoints.Clear();

            // erste area hinzufügen
            DrehzahlPoints.Add(new List<ISeries>());
            // Dummy punkt hinzufügen
            //DrehzahlPoints[0].Add(new ISeries(0, 0));

            foreach (var point in unfiltered_data)
            {
                // punkt vergleichen mit vorhandenen punkten in areas
                for (int area = 0; area < DrehzahlPoints.Count; area++)
                {
                    // jeder punkt in area
                    foreach (var areaPoint in DrehzahlPoints[area])
                    {
                        // Horizontal
                        //double differenz_horizontal = Math.Abs(point.X - areaPoint.X);

                        // Vertikal
                        //double differenz_vertikal = Math.Abs(point.Y - areaPoint.Y);

                        // in vorhandene area hinzufügen
                        //if (differenz_vertikal <= _areaAbstand && differenz_horizontal <= _areaAbstand)
                        //{
                        //    DrehzahlPoints[area].Add(point);

                        //    goto nextPoint;
                        //}
                        //// wenn auch in letzter Area nichts gefunden werden konnte
                        //else if (area == DrehzahlPoints.Count - 1)
                        //{
                        //    // neue area beginnen
                        //    DrehzahlPoints.Add(new List<ISeries>() { point });

                        //    goto nextPoint;
                        //}
                    }
                }

                nextPoint:
                continue;
            }

            // Dummy punkt löschen
            DrehzahlPoints[0].Remove(DrehzahlPoints[0][0]);
        }

        /// <summary>
        /// Umrechnen von Hertz in Drehzahl.
        /// </summary>
        private static void PointsToRotionalSpeed()
        {
            List<List<ISeries>> temp_plot_data = new List<List<ISeries>>();

            // anzahl graphen
            for (int area = 0; area < DrehzahlPoints.Count; area++)
            {
                List<ISeries> temp_list = new List<ISeries>();

                // Punkt in graphen
                for (int col = 0; col < DrehzahlPoints[area].Count; col++)
                {
                    // Y(vertikal) = 1U/min; 1Hz = 60 U/min bei Einzylider Motoren
                    // X(horizontal) = 1s
                    //temp_list.Add(new ISeries(Math.Round(DrehzahlPoints[area][col].X * SegmentsPerSecond, 4), Math.Round(DrehzahlPoints[area][col].Y * HzPerFFT * 60, 4)));
                }

                temp_plot_data.Add(temp_list);
            }

            DrehzahlPoints.Clear();
            DrehzahlPoints.AddRange(temp_plot_data);
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
            SpectrogramAudio.SetColormap(_colormap);
            bmp = SpectrogramAudio.GetBitmap(intensity: _intensity);
        }
    }
}