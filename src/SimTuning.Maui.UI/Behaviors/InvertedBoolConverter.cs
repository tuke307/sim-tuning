// Copyright (c) 2021 tuke productions. All rights reserved.
using System;
using System.Globalization;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace SimTuning.Maui.UI.Behaviors
{
    public class InvertedBoolConverter : IValueConverter, IMarkupExtension
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}