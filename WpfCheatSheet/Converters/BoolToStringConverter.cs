﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfCheatSheet.Converters
{
    sealed class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (bool)value ? "Y" : "";
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}