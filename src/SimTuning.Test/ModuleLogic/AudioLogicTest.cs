// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Test
{
    using SimTuning.Core.ModuleLogic;
    using Xunit;

    /// <summary>
    /// AudioLogicTest.
    /// </summary>
    public class AudioLogicTest
    {
        /// <summary>
        /// Spectrograms the creation test.
        /// </summary>
        /// <param name="fftSize">Size of the FFT.</param>
        [InlineData(8192)] // 2^13
        [InlineData(16384)] // 2^14
        [InlineData(32768)] // 2^15
        [InlineData(65536)] // 2^16
        public void SpectrogramCreationTest(int fftSize)
        {
            AudioLogic.CalculateSpectrogram(
                audioFile: SimTuning.Test.Constants.DynoAudioFile,
                fftSize: fftSize);

            // Phase 17: GetBitmap() (image rendering) was removed as dead surface; the kept
            // API is the raw FFT data. Assert the spectrogram produced columns.
            // NOTE: this method is still missing [Theory] and relies on a Windows-only path
            // (Constants.DynoAudioFile) — both are Phase 15's scope.
            Assert.NotEmpty(AudioLogic.SpectrogramAudio!.GetFFTs());
        }
    }
}
