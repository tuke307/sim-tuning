// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Core
{
    using Microsoft.Maui.Storage;
    using SimTuning.Data;
    using System.IO;

    /// <summary>
    /// Allgemeine-SimTuning Konstanten.
    /// </summary>
    public static class GeneralSettings
    {
        /// <summary>
        /// Gets or sets the audio file.
        /// </summary>
        /// <value>The audio file.</value>
        public static string AudioAccelerationFile
        {
            get => GetPreference(nameof(AudioAccelerationFile), "DynoAccelerationAudio.wav");
            set => SetPreference(nameof(AudioAccelerationFile), value);
        }

        /// <summary>
        /// Gets the audio file path.
        /// </summary>
        /// <value>The audio file path.</value>
        public static string AudioAccelerationFilePath
        {
            get
            {
                return Path.Combine(DatabaseSettings.FileDirectory, AudioAccelerationFile);
            }
        }

        /// <summary>
        /// Gets or sets the audio file.
        /// </summary>
        /// <value>The audio file.</value>
        public static string AudioRolloutFile
        {
            get => GetPreference(nameof(AudioRolloutFile), "DynoAusrollenAudio.wav");
            set => SetPreference(nameof(AudioRolloutFile), value);
        }

        /// <summary>
        /// Gets the audio file path.
        /// </summary>
        /// <value>The audio file path.</value>
        public static string AudioRolloutFilePath
        {
            get
            {
                return Path.Combine(DatabaseSettings.FileDirectory, AudioRolloutFile);
            }
        }

        /// <summary>
        /// Gets or sets the data export archive.
        /// </summary>
        /// <value>The data export archive.</value>
        public static string DataExportArchive
        {
            get => GetPreference(nameof(DataExportArchive), "DataExport.zip");
            set => SetPreference(nameof(DataExportArchive), value);
        }

        /// <summary>
        /// Gets the data export archive path.
        /// </summary>
        /// <value>The data export archive path.</value>
        public static string DataExportArchivePath
        {
            get
            {
                return Path.Combine(DatabaseSettings.FileDirectory, DataExportArchive);
            }
        }

        /// <summary>
        /// Gets or sets the data export file.
        /// </summary>
        /// <value>The data export file.</value>
        public static string DataExportFile
        {
            get => GetPreference(nameof(DataExportFile), "DataExport.json");
            set => SetPreference(nameof(DataExportFile), value);
        }

        /// <summary>
        /// Gets the data export file path.
        /// </summary>
        /// <value>The data export file path.</value>
        public static string DataExportFilePath
        {
            get
            {
                return Path.Combine(DatabaseSettings.FileDirectory, DataExportFile);
            }
        }

        /// <summary>
        /// Gets or sets the log file.
        /// </summary>
        /// <value>The log file.</value>
        public static string LogFile
        {
            get => GetPreference(nameof(LogFile), "simtuning_log_.log");
            set => SetPreference(nameof(LogFile), value);
        }

        /// <summary>
        /// Gets the log file path.
        /// </summary>
        /// <value>The log file path.</value>
        public static string LogFilePath
        {
            get
            {
                return Path.Combine(DatabaseSettings.FileDirectory, LogFile);
            }
        }

        /// <summary>
        /// Gets or sets the release notes file.
        /// </summary>
        /// <value>The release notes file.</value>
        public static string ReleaseNotesFile
        {
            get => GetPreference(nameof(ReleaseNotesFile), "releasenotes.txt");
            set => SetPreference(nameof(ReleaseNotesFile), value);
        }

        /// <summary>
        /// Gets the release notes file path.
        /// </summary>
        /// <value>The release notes file path.</value>
        public static string ReleaseNotesFilePath
        {
            get
            {
                return Path.Combine(DatabaseSettings.FileDirectory, ReleaseNotesFile);
            }
        }

        /// <summary>
        /// Reads a preference, tolerating the absence of a MAUI Essentials host. On a real
        /// device <see cref="Preferences.Default" /> is always available and this is a plain
        /// pass-through; in design-time/headless hosts (unit tests, XAML designer) the
        /// reference assembly throws <c>NotImplementedInReferenceAssemblyException</c>, so the
        /// documented default is returned instead. Parity with
        /// <see cref="SimTuning.Data.DatabaseSettings" />. Phase 8b.
        /// </summary>
        private static string GetPreference(string key, string defaultValue)
        {
            try
            {
                return Preferences.Default.Get(key, defaultValue);
            }
            catch (Exception)
            {
                // Fallback for design-time/headless hosts where Preferences has no platform
                // implementation. Parity with DatabaseSettings.GetPreference. Phase 8b.
                return defaultValue;
            }
        }

        /// <summary>
        /// Writes a preference, tolerating the absence of a MAUI Essentials host. See
        /// <see cref="GetPreference" />. Phase 8b.
        /// </summary>
        private static void SetPreference(string key, string value)
        {
            try
            {
                Preferences.Default.Set(key, value);
            }
            catch (Exception)
            {
                // Ignore at design-time/headless — parity with DatabaseSettings.SetPreference. Phase 8b.
            }
        }
    }
}
