// Copyright (c) 2021 tuke productions. All rights reserved.
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
            get
            {
                return Preferences.Default.Get(nameof(AudioAccelerationFile), "DynoAccelerationAudio.wav");
            }

            set
            {
                Preferences.Default.Set(nameof(AudioAccelerationFile), value);
            }
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
            get
            {
                return Preferences.Default.Get(nameof(AudioRolloutFile), "DynoAusrollenAudio.wav");
            }

            set
            {
                Preferences.Default.Set(nameof(AudioRolloutFile), value);
            }
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
            get
            {
                return Preferences.Default.Get(nameof(DataExportArchive), "DataExport.zip");
            }

            set
            {
                Preferences.Default.Set(nameof(DataExportArchive), value);
            }
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
            get
            {
                return Preferences.Default.Get(nameof(DataExportFile), "DataExport.json");
            }

            set
            {
                Preferences.Default.Set(nameof(DataExportFile), value);
            }
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
            get
            {
                return Preferences.Default.Get(nameof(LogFile), "simtuning_log_.log");
            }

            set
            {
                Preferences.Default.Set(nameof(LogFile), value);
            }
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
            get
            {
                return Preferences.Default.Get(nameof(ReleaseNotesFile), "releasenotes.txt");
            }

            set
            {
                Preferences.Default.Set(nameof(ReleaseNotesFile), value);
            }
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
    }
}