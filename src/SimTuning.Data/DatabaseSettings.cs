// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Data
{
    using Microsoft.Maui.Storage;
    using System.IO;

#pragma warning disable CS8305
    using FileSystem = Microsoft.Maui.Storage.FileSystem;
    using Preferences = Microsoft.Maui.Storage.Preferences;
#pragma warning restore CS8305

    /// <summary>
    /// Database configuration and path management.
    /// </summary>
    public static class DatabaseSettings
    {
        /// <summary>
        /// The database name.
        /// </summary>
        public const string DatabaseName = "simtuning.db";

        private static string _databasePath;
        private static string _fileDirectory;

        /// <summary>
        /// Gets or sets the database path.
        /// </summary>
        public static string DatabasePath
        {
            get
            {
                if (string.IsNullOrEmpty(_databasePath))
                {
                    _databasePath = Path.Combine(FileDirectory, DatabaseName);
                }

                return GetPreference(nameof(DatabasePath), _databasePath);
            }
            set
            {
                _databasePath = value;
                SetPreference(nameof(DatabasePath), value);
            }
        }

        /// <summary>
        /// Gets or sets the file directory.
        /// </summary>
        public static string FileDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_fileDirectory))
                {
                    _fileDirectory = GetAppDataDirectory();
                    EnsureDirectoryExists(_fileDirectory);
                }

                return GetPreference(nameof(FileDirectory), _fileDirectory);
            }
            set
            {
                _fileDirectory = value;
                SetPreference(nameof(FileDirectory), value);
            }
        }

        private static string GetAppDataDirectory()
        {
            try
            {
                return FileSystem.AppDataDirectory;
            }
            catch
            {
                // Fallback for design-time when FileSystem is not available
                return Path.GetTempPath();
            }
        }

        private static void EnsureDirectoryExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static string GetPreference(string key, string defaultValue)
        {
            try
            {
                return Preferences.Default.Get(key, defaultValue);
            }
            catch
            {
                // Fallback for design-time when Preferences is not available
                return defaultValue;
            }
        }

        private static void SetPreference(string key, string value)
        {
            try
            {
                Preferences.Default.Set(key, value);
            }
            catch
            {
                // Ignore at design-time when Preferences is not available
            }
        }
    }
}
