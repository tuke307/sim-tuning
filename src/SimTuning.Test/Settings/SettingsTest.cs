// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Test.Settings
{
    using SimTuning.Core;
    using SimTuning.Data;
    using Xunit;

    /// <summary>
    /// Verifies the Phase-8b design-time fallback parity across the Settings layer. In the
    /// headless xUnit host there is no MAUI Essentials platform implementation, so
    /// <c>Preferences.Default</c> throws <c>NotImplementedInReferenceAssemblyException</c>;
    /// <see cref="GeneralSettings" />, <see cref="UnitSettings" /> and
    /// <see cref="DatabaseSettings" /> must all tolerate that and return their documented
    /// defaults instead of crashing. On a real device these try/catch branches never fire.
    /// </summary>
    public class SettingsTest
    {
        [Fact]
        public void GeneralSettingsReturnsDefaultsInHeadlessHost()
        {
            Assert.Equal("DynoAccelerationAudio.wav", GeneralSettings.AudioAccelerationFile);
            Assert.Equal("DynoAusrollenAudio.wav", GeneralSettings.AudioRolloutFile);
            Assert.Equal("DataExport.zip", GeneralSettings.DataExportArchive);
            Assert.Equal("DataExport.json", GeneralSettings.DataExportFile);
            Assert.Equal("simtuning_log_.log", GeneralSettings.LogFile);
            Assert.Equal("releasenotes.txt", GeneralSettings.ReleaseNotesFile);
        }

        [Fact]
        public void GeneralSettingsFilePathsResolveWithoutPlatformHost()
        {
            // Each *FilePath composes DatabaseSettings.FileDirectory (which already falls back to
            // the temp dir) with the file name — must not throw and must end in the file name.
            Assert.EndsWith("DynoAccelerationAudio.wav", GeneralSettings.AudioAccelerationFilePath);
            Assert.EndsWith("DynoAusrollenAudio.wav", GeneralSettings.AudioRolloutFilePath);
            Assert.EndsWith("DataExport.zip", GeneralSettings.DataExportArchivePath);
            Assert.EndsWith("DataExport.json", GeneralSettings.DataExportFilePath);
        }

        [Fact]
        public void GeneralSettingsSetIsToleratedInHeadlessHost()
        {
            // Set must be a no-op (swallowed) rather than throwing.
            GeneralSettings.AudioAccelerationFile = "other.wav";
            Assert.Equal("DynoAccelerationAudio.wav", GeneralSettings.AudioAccelerationFile);
        }

        [Fact]
        public void UnitSettingsReturnsDefaultsInHeadlessHost()
        {
            Assert.Equal(2, UnitSettings.RoundingAccuracy);
            Assert.True(UnitSettings.RoundOnUnitChange);
        }
    }
}
