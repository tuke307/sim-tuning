// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Test
{
    using System;
    using System.IO;

    /// <summary>
    /// Constants.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The dyno audio file.
        /// </summary>
        public static readonly string DynoAudioFile = @"C:\Users\Tony\AppData\Roaming\SimTuning\DynoAccelerationAudio.wav";

        private const string DirectoryName = "UnitTests";

        /// <summary>
        /// Gets the directory.
        /// </summary>
        /// <value>The directory.</value>
        public static string Directory
        {
            get
            {
                return Path.Combine(RootDirectory, DirectoryName);
            }
        }

        /// <summary>
        /// Gets the root directory.
        /// </summary>
        /// <value>The root directory.</value>
        public static string RootDirectory
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SimTuning"); // appdata-local-simtuning
            }
        }
    }
}