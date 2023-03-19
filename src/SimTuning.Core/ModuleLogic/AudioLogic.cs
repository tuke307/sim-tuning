// Copyright (c) 2021 tuke productions. All rights reserved.
namespace SimTuning.Core.ModuleLogic
{
    using Dbscan;
    using SimTuning.Core.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class AudioLogic
    {
        #region variables

        private static double _epsilon;
        private static string _audioFile;
        private static int _fftSize;
        private static double _intensity;
        private static int _maxFreq;
        private static int _minFreq;
        private static int _sampleRate;
        private static int _stepSize;

        /// <summary>
        /// Punkte der Drehzahl.
        /// </summary>
        public static List<DataPoint> RotationalSpeedPoints { get; private set; }

        /// <summary>
        /// 1. Liste = Custer,
        /// 2. Liste = Punkte des Custers.
        /// </summary>
        public static List<List<DataPoint>> ClusterPoints { get; private set; }

        /// <summary>
        /// Gets the spectrogram audio.
        /// </summary>
        /// <value>The spectrogram audio.</value>
        public static Spectrogram.Spectrogram SpectrogramAudio { get; private set; }

        /// <summary>
        /// Gets fFT-Array-elements pro 1 Hertz.
        /// </summary>
        private static double HzPerFFT
        {
            get => SpectrogramAudio.HzPerPx;
        }

        /// <summary>
        /// Gets wieviel reihen ein Herz wiederspiegeln. HORIZONTAL; anzahl spalten pro sekunde.
        /// </summary>
        private static double SegmentsPerSecond
        {
            get => SpectrogramAudio.SecPerPx;
        }

        /// <summary>
        /// Gets spectrogram Data, Liste = Spalten, [] = Zeilen, float = Stärke der Frequenz.
        /// </summary>
        private static List<double[]> SpecData
        {
            get => SpectrogramAudio?.GetFFTs();
        }

        #endregion variables

        /// <summary>
        /// Bildet aus den Spectrogram Daten X-Y Punkte.
        /// </summary>
        /// <param name="intensity">if set to <c>true</c> [areas].</param>
        public static void CalculateRotationalSpeed(double intensity = 0.75)
        {
            _intensity = intensity;

            DefineHotPoints();

            for (int j = 0; j < RotationalSpeedPoints.Count; j++)
            {
                RotationalSpeedPoints[j] = RotationalSpeedPoints[j].ToRotionalSpeed();
            }
        }

        /// <summary>
        /// Definieren von Clustern aus Gesamtpunkten.
        /// </summary>
        /// <param name="epsilon">if set to <c>true</c> [areas].</param>
        public static void CalculateClusters(double epsilon = 8)
        {
            _epsilon = epsilon;

            ClusterPoints = new List<List<DataPoint>>();

            var clusters = Dbscan.CalculateClusters(
                data: RotationalSpeedPoints,
                epsilon: _epsilon,
                minimumPointsPerCluster: 5);

            foreach (var cluster in clusters.Clusters)
                ClusterPoints.Add(cluster.Objects.ToList());
        }

        /// <summary>
        /// Definieren des Frequenz-Spectrogram mit bestimmten Parametern.
        /// </summary>
        /// <param name="audioFile"></param>
        /// <param name="fftSize"></param>
        /// <param name="minFreq"></param>
        /// <param name="maxFreq"></param>
        /// <param name="intensity"></param>
        /// <returns>Bild des Spectrograms.</returns>
        public static void CalculateSpectrogram(
            string audioFile,
            int fftSize = 16384,
            int minFreq = 25,
            int maxFreq = 250,
            double intensity = 1)
        {
            _audioFile = audioFile;

            _intensity = intensity;

            _minFreq = minFreq;

            _maxFreq = maxFreq;

            _fftSize = fftSize;

            (_sampleRate, double[] _audio) = Spectrogram.WavFile.ReadMono(_audioFile);

            _stepSize = _audio.Length / 1000;

            // load audio and process FFT
            SpectrogramAudio = new Spectrogram.Spectrogram(
                                                    sampleRate: _sampleRate,
                                                    fftSize: _fftSize,
                                                    stepSize: _stepSize,
                                                    minFreq: _minFreq,
                                                    maxFreq: _maxFreq);

            SpectrogramAudio.Add(_audio, true);
        }

        /// <summary>
        /// Definiert die Punkte die Punkte die für eine Analyse verwertbar sind.
        /// </summary>
        private static void DefineHotPoints()
        {
            RotationalSpeedPoints = new List<DataPoint>();
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
                RotationalSpeedPoints.AddRange(hotDataPoints);
            }
        }

        /// <summary>
        /// Umrechnen von Hertz in Drehzahl.
        /// </summary>
        /// <param name="dataPoint"></param>
        private static DataPoint ToRotionalSpeed(this DataPoint dataPoint)
        {
            // X (horizontal) = in ms
            // Y (vertikal) = 1U/min; 1Hz = 60 U/min bei Einzylider Motoren
            DataPoint converted = new DataPoint(
                Math.Round(dataPoint.X * SegmentsPerSecond * 1000, 4),
                Math.Round(dataPoint.Y * HzPerFFT * 60, 4));

            return converted;
        }
    }
}