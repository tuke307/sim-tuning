// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Test
{
    using SimTuning.Core.ModuleLogic;
    using System;
    using System.IO;
    using System.Text;
    using Xunit;

    /// <summary>
    /// AudioLogicTest.
    /// </summary>
    public class AudioLogicTest
    {
        /// <summary>
        /// Spectrograms the creation test. Generates a synthetic mono 16-bit PCM WAV (sine wave)
        /// in the temp dir so the test is self-contained and cross-platform — Phase 15 replaced
        /// the previous hardcoded Windows audio-file path + added the missing [Theory].
        /// </summary>
        /// <param name="fftSize">Size of the FFT.</param>
        [Theory]
        [InlineData(8192)] // 2^13
        [InlineData(16384)] // 2^14
        [InlineData(32768)] // 2^15
        [InlineData(65536)] // 2^16
        public void SpectrogramCreationTest(int fftSize)
        {
            const int sampleRate = 44100;

            // Generate enough audio for at least two FFT windows at every fftSize: the largest
            // (65536) needs >1.5 s of data, so the previous fixed 1 s @ 44.1 kHz (44 100 samples)
            // left zero whole windows for it and GetFFTs() came back empty. Phase 15.
            double seconds = Math.Max(1, Math.Ceiling((fftSize * 2.0) / sampleRate));
            string audioFile = Path.Combine(SimTuning.Test.Constants.Directory, $"synthetic_{fftSize}.wav");
            WriteSineWaveWav(audioFile, sampleRate, seconds, frequencyHz: 1000);

            AudioLogic.CalculateSpectrogram(audioFile: audioFile, fftSize: fftSize);

            // The kept spectrogram API exposes the raw FFT magnitude columns (one per time step).
            Assert.NotEmpty(AudioLogic.SpectrogramAudio!.GetFFTs());
            foreach (double[] fft in AudioLogic.SpectrogramAudio.GetFFTs())
            {
                Assert.NotEmpty(fft);
            }
        }

        /// <summary>
        /// Writes a minimal 16-bit mono PCM WAV containing a sine wave — the exact format
        /// <c>Spectrogram.WavFile.ReadMono</c> parses (RIFF + 16-byte PCM fmt + data chunk).
        /// </summary>
        private static void WriteSineWaveWav(string path, int sampleRate, double seconds, double frequencyHz)
        {
            int sampleCount = (int)(sampleRate * seconds);
            short[] samples = new short[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                double t = (double)i / sampleRate;
                samples[i] = (short)(short.MaxValue * 0.8 * Math.Sin(2 * Math.PI * frequencyHz * t));
            }

            const short bitsPerSample = 16;
            const short channels = 1;
            int byteRate = sampleRate * channels * bitsPerSample / 8;
            short blockAlign = (short)(channels * bitsPerSample / 8);
            int dataBytes = sampleCount * blockAlign;

            using FileStream fs = File.Create(path);
            using BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(Encoding.ASCII.GetBytes("RIFF")); // chunk id
            bw.Write(36 + dataBytes);                 // chunk size
            bw.Write(Encoding.ASCII.GetBytes("WAVE")); // format
            bw.Write(Encoding.ASCII.GetBytes("fmt ")); // fmt subchunk id
            bw.Write(16);                             // fmt subchunk size (PCM)
            bw.Write((short)1);                       // audio format = PCM
            bw.Write(channels);
            bw.Write(sampleRate);
            bw.Write(byteRate);
            bw.Write(blockAlign);
            bw.Write(bitsPerSample);
            bw.Write(Encoding.ASCII.GetBytes("data")); // data subchunk id
            bw.Write(dataBytes);
            for (int i = 0; i < sampleCount; i++)
            {
                bw.Write(samples[i]);
            }
        }
    }
}
