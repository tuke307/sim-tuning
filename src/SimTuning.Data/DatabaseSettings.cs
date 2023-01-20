// Copyright (c) 2021 tuke productions. All rights reserved.
namespace SimTuning.Data
{
    using Microsoft.Maui.Devices;
    using Microsoft.Maui.Storage;
    using System;
    using System.IO;

    /// <summary>
    /// Konstanten für die Datenbank-Abwicklung.
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
        /// <value>The database path.</value>
        public static string DatabasePath
        {
            get
            {
                if (string.IsNullOrEmpty(_databasePath))
                {
                    _databasePath = Path.Combine(FileDirectory, DatabaseName);
                    DatabasePath = _databasePath;
                }

                return Preferences.Default.Get(nameof(DatabasePath), _databasePath);
            }
            set
            {
                Preferences.Default.Set(nameof(DatabasePath), value);
            }
        }

        /// <summary>
        /// Gets or sets the file directory.
        /// </summary>
        /// <value>The file directory.</value>
        public static string FileDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_fileDirectory))
                {
                    if (DeviceInfo.Idiom == DeviceIdiom.Phone)
                    {
                        // android: "/data/user/0/com.tuke_productions.SimTuning/files/"
                        _fileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    }
                    else
                    {
                        // AppData\Roaming\SimTuning
                        _fileDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SimTuning");
                    }

                    // ist evtl. anfangs nicht da
                    if (!Directory.Exists(_fileDirectory))
                    {
                        Directory.CreateDirectory(_fileDirectory);
                    }

                    FileDirectory = _fileDirectory;
                }

                return Preferences.Default.Get(nameof(FileDirectory), _fileDirectory);
            }

            set
            {
                Preferences.Default.Set(nameof(FileDirectory), value);
            }
        }
    }
}