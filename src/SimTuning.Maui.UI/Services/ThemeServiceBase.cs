// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Maui.UI.Themes;
using SimTuning.Maui.UI.Themes.Base;
using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace SimTuning.Maui.UI.Services
{
    public class ThemeServiceBase : IThemeService
    {
        public BaseTheme CurrentRuntimeTheme { get; private set; }

        public void UpdateTheme(BaseTheme themeMode)
        {
            switch (ColorSettings.Theme)
            {
                case (int)BaseTheme.Inherit:
                    if (themeMode == BaseTheme.Dark)
                        goto case BaseTheme.Dark;
                    else
                        goto case BaseTheme.Light;
                case (int)BaseTheme.Dark:
                    SetTheme(BaseTheme.Dark);
                    break;

                case (int)BaseTheme.Light:
                    SetTheme(BaseTheme.Light);
                    break;

                default:
                    break;
            }
        }

        private void SetTheme(BaseTheme themeMode)
        {
            if (CurrentRuntimeTheme == themeMode)
            {
                return;
            }

            CurrentRuntimeTheme = themeMode;
        }
    }
}