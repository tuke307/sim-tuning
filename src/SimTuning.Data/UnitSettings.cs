// Copyright (c) 2025 tuke productions. All rights reserved.
using Microsoft.Maui.Storage;

namespace SimTuning.Data
{
    /// <summary>
    /// UnitSettings.
    /// </summary>
    public class UnitSettings
    {
        /// <summary>
        /// Gets or sets the rounding accuracy.
        /// </summary>
        /// <value>The rounding accuracy.</value>
        public static int RoundingAccuracy
        {
            get => GetPreference(nameof(RoundingAccuracy), 2);
            set => SetPreference(nameof(RoundingAccuracy), value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether [round on unit change].
        /// </summary>
        /// <value><c>true</c> if [round on unit change]; otherwise, <c>false</c>.</value>
        public static bool RoundOnUnitChange
        {
            get => GetPreference(nameof(RoundOnUnitChange), true);
            set => SetPreference(nameof(RoundOnUnitChange), value);
        }

        /// <summary>
        /// Reads a preference, tolerating the absence of a MAUI Essentials host. On a real
        /// device <see cref="Preferences.Default" /> is always available and this is a plain
        /// pass-through; in design-time/headless hosts the reference assembly throws and the
        /// documented default is returned instead. Parity with
        /// <see cref="DatabaseSettings" />. Phase 8b.
        /// </summary>
        private static T GetPreference<T>(string key, T defaultValue)
        {
            try
            {
                return Preferences.Default.Get(key, defaultValue);
            }
            catch (Exception)
            {
                // Fallback for design-time/headless hosts. Parity with DatabaseSettings. Phase 8b.
                return defaultValue;
            }
        }

        /// <summary>
        /// Writes a preference, tolerating the absence of a MAUI Essentials host. See
        /// <see cref="GetPreference{T}" />. Phase 8b.
        /// </summary>
        private static void SetPreference<T>(string key, T value)
        {
            try
            {
                Preferences.Default.Set(key, value);
            }
            catch (Exception)
            {
                // Ignore at design-time/headless — parity with DatabaseSettings. Phase 8b.
            }
        }
    }
}
