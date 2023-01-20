// Copyright (c) 2021 tuke productions. All rights reserved.
using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace SimTuning.Core.Converters
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        private const string DEFAULT_FORMAT = @"mm\:ss";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is string format))
            {
                format = DEFAULT_FORMAT;
            }

            var timeSpan = (TimeSpan)value;
            return timeSpan.ToString(format);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}