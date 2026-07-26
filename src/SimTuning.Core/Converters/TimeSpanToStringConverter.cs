// Copyright (c) 2025 tuke productions. All rights reserved.
using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace SimTuning.Core.Converters
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        private const string DEFAULT_FORMAT = @"mm\:ss";

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            var format = parameter as string ?? DEFAULT_FORMAT;
            return value is TimeSpan timeSpan ? timeSpan.ToString(format) : string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            throw new NotImplementedException();
        }
    }
}