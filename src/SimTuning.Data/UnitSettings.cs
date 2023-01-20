// Copyright (c) 2021 tuke productions. All rights reserved.
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
            get
            {
                return Preferences.Default.Get(nameof(RoundingAccuracy), 2);
            }

            set
            {
                Preferences.Default.Set(nameof(RoundingAccuracy), value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [round on unit change].
        /// </summary>
        /// <value><c>true</c> if [round on unit change]; otherwise, <c>false</c>.</value>
        public static bool RoundOnUnitChange
        {
            get
            {
                return Preferences.Default.Get(nameof(RoundOnUnitChange), true);
            }

            set
            {
                Preferences.Default.Set(nameof(RoundOnUnitChange), value);
            }
        }
    }
}