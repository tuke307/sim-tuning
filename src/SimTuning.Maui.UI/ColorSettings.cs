// Copyright (c) 2021 tuke productions. All rights reserved.
using System;
using System.Collections.Generic;
using System.Text;

namespace SimTuning.Maui.UI
{
    /// <summary>
    /// ColorSettings.
    /// </summary>
    public static class ColorSettings
    {
        /// <summary>
        /// Gets or sets the PrimaryColor.
        /// TODO: getter geht nicht.
        /// </summary>
        /// <value>The primary.</value>
        public static int Primary
        {
            get
            {
                return Preferences.Default.Get(nameof(Primary), (int)Themes.PrimaryColor.Cyan);
            }

            set
            {
                Preferences.Default.Set(nameof(Primary), value);
            }
        }

        /// <summary>
        /// Gets or sets the SecondaryColor.
        /// </summary>
        /// <value>The secondary.</value>
        public static int Secondary
        {
            get
            {
                return Preferences.Default.Get(nameof(Secondary), (int)Themes.SecondaryColor.Teal);
            }

            set
            {
                Preferences.Default.Set(nameof(Secondary), value);
            }
        }

        /// <summary>
        /// Gets or sets the BaseTheme.
        /// </summary>
        /// <value>The theme.</value>
        public static int Theme
        {
            get
            {
                return Preferences.Default.Get(nameof(Theme), (int)Themes.BaseTheme.Inherit);
            }

            set
            {
                Preferences.Default.Set(nameof(Theme), value);
            }
        }
    }
}