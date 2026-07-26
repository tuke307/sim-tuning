// Copyright (c) 2025 tuke productions. All rights reserved.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spectrogram
{
    /// <summary>
    /// Frequency spectrogram generator. Computes windowed FFT magnitude columns over an
    /// audio signal and exposes the raw FFT data — the surface SimTuning's <c>AudioLogic</c>
    /// uses to extract rotational-speed hot points.
    /// </summary>
    /// <remarks>
    /// Phase 17 removed the dead image/colormap/SFF/Mel/peak surface (the only consumer is
    /// <c>AudioLogic</c>). The upstream Spectrogram NuGet (swharden) was evaluated and rejected:
    /// it depends on <c>System.Drawing.Common</c> (Windows-only, unsupported on iOS/MacCatalyst
    /// in .NET 6+), is image-rendering focused (no <c>GetFFTs()</c> raw-data API), and has been
    /// unmaintained since 2022. This vendored copy was already adapted (SkiaSharp-free, exposed
    /// raw FFTs, custom <c>WavFile</c>) for cross-platform use, so it is kept and trimmed.
    /// </remarks>
    public class Spectrogram
    {
        private readonly List<double[]> ffts = new List<double[]>();

        private readonly List<double> newAudio = new List<double>();

        private readonly Settings settings;

        /// <summary>
        /// Gets the number of FFT columns processed so far (horizontal axis width).
        /// </summary>
        public int Width => ffts.Count;

        /// <summary>
        /// Gets the number of frequency bins per column (vertical axis height).
        /// </summary>
        public int Height => settings.Height;

        /// <summary>
        /// Gets the Hertz represented by each vertical pixel (frequency resolution).
        /// </summary>
        public double HzPerPx => settings.HzPerPixel;

        /// <summary>
        /// Gets the seconds represented by each horizontal column (time resolution).
        /// </summary>
        public double SecPerPx => settings.StepLengthSec;

        /// <summary>
        /// Initializes a new instance of the <see cref="Spectrogram" /> class.
        /// </summary>
        /// <param name="sampleRate">The audio sample rate, in Hz.</param>
        /// <param name="fftSize">The FFT size (must be a power of 2).</param>
        /// <param name="stepSize">The number of samples between successive FFT columns.</param>
        /// <param name="minFreq">The lowest frequency to keep, in Hz.</param>
        /// <param name="maxFreq">The highest frequency to keep, in Hz.</param>
        public Spectrogram(int sampleRate, int fftSize, int stepSize,
            double minFreq = 0, double maxFreq = double.PositiveInfinity)
        {
            settings = new Settings(sampleRate, fftSize, stepSize, minFreq, maxFreq, offsetHz: 0);
        }

        /// <summary>
        /// Appends audio samples and optionally processes them into FFT columns.
        /// </summary>
        /// <param name="audio">The mono audio samples to add.</param>
        /// <param name="process">If <c>true</c>, process the buffered audio into FFT columns immediately.</param>
        public void Add(double[] audio, bool process = true)
        {
            newAudio.AddRange(audio);
            if (process)
            {
                Process();
            }
        }

        /// <summary>
        /// Processes the buffered audio into FFT magnitude columns. Each column holds the
        /// magnitudes (scaled by FFT size) for the configured [<c>minFreq</c>, <c>maxFreq</c>] band.
        /// </summary>
        /// <returns>The newly produced FFT columns, or <c>null</c> if too little audio is buffered.</returns>
        public double[][] Process()
        {
            int newFftCount = FftsToProcess;
            if (newFftCount < 1)
            {
                return null;
            }

            double[][] newFfts = new double[newFftCount][];

            Parallel.For(0, newFftCount, newFftIndex =>
            {
                System.Numerics.Complex[] buffer = new System.Numerics.Complex[settings.FftSize];
                int sourceIndex = newFftIndex * settings.StepSize;
                for (int i = 0; i < settings.FftSize; i++)
                {
                    buffer[i] = new System.Numerics.Complex(newAudio[sourceIndex + i] * settings.Window[i], 0);
                }

                FftSharp.FFT.Forward(buffer);

                newFfts[newFftIndex] = new double[settings.Height];
                for (int i = 0; i < settings.Height; i++)
                {
                    newFfts[newFftIndex][i] = buffer[settings.FftIndex1 + i].Magnitude / settings.FftSize;
                }
            });

            foreach (var newFft in newFfts)
            {
                ffts.Add(newFft);
            }

            newAudio.RemoveRange(0, newFftCount * settings.StepSize);

            return newFfts;
        }

        /// <summary>
        /// Gets the processed FFT magnitude columns (one <c>double[]</c> per time step).
        /// </summary>
        public List<double[]> GetFFTs()
        {
            return ffts;
        }

        /// <summary>
        /// Gets the number of FFTs that can still be produced from the buffered audio.
        /// </summary>
        private int FftsToProcess => (newAudio.Count - settings.FftSize) / settings.StepSize;
    }
}
