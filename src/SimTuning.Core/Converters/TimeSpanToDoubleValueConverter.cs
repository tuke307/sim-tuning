// Copyright (c) 2025 tuke productions. All rights reserved.
using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace SimTuning.Core.Converters
{
    public class TimeSpanToDoubleValueConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return value is TimeSpan timeSpan ? timeSpan.TotalSeconds : 0d;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return value is double totalSeconds ? TimeSpan.FromSeconds(totalSeconds) : TimeSpan.Zero;
        }
    }
}