// Copyright (c) 2021 tuke productions. All rights reserved.
using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace SimTuning.Core.Converters
{
    public class TimeSpanToDoubleValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var timeSpan = (TimeSpan)value;
            return timeSpan.TotalSeconds;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var totalSeconds = (double)value;
            return TimeSpan.FromSeconds(totalSeconds);
        }
    }
}