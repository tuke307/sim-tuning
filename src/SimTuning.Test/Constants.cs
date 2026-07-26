// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Test
{
    using System;
    using System.IO;

    /// <summary>
    /// Test-path constants. Phase 15: rooted under the system temp directory (cross-platform,
    /// always writable, OS-cleaned) and the directory is created on access, so the artifact-
    /// writing tests no longer fail with DirectoryNotFoundException off-Windows. The previous
    /// hardcoded Windows audio-file path was removed (AudioLogicTest now generates a synthetic WAV).
    /// </summary>
    public static class Constants
    {
        private const string DirectoryName = "UnitTests";

        /// <summary>
        /// Gets the directory used by tests that write artifacts (e.g. PNGs). Created on access.
        /// </summary>
        public static string Directory
        {
            get
            {
                string dir = Path.Combine(RootDirectory, DirectoryName);
                System.IO.Directory.CreateDirectory(dir);

                return dir;
            }
        }

        /// <summary>
        /// Gets the root directory under the system temp path.
        /// </summary>
        public static string RootDirectory => Path.Combine(Path.GetTempPath(), "SimTuning");
    }
}
