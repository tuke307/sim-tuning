// Copyright (c) 2021 tuke productions. All rights reserved.
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace SimTuning.Maui.UI.Components
{
    public class CustomViewCell : ViewCell
    {
        /// <summary>
        /// The SelectedBackgroundColor property.
        /// </summary>
        public static readonly BindableProperty SelectedBackgroundColorProperty =
            BindableProperty.Create("SelectedBackgroundColor", typeof(Color), typeof(CustomViewCell), Colors.Transparent);

        /// <summary>
        /// Gets or sets the SelectedBackgroundColor.
        /// </summary>
        public Color SelectedBackgroundColor
        {
            get { return (Color)GetValue(SelectedBackgroundColorProperty); }
            set { SetValue(SelectedBackgroundColorProperty, value); }
        }
    }
}