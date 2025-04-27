using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace CurrencyMobile.Converters
{
    public class BoolToStringConverter : BaseValueConverter
    {
        public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b && parameter is string choices)
            {
                var parts = choices.Split(';');
                if (parts.Length >= 2)
                {
                    return b ? parts[0] : parts[1];
                }
            }
            return value?.ToString() ?? string.Empty;
        }

        public override object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string s && parameter is string choices)
            {
                var parts = choices.Split(';');
                if (parts.Length >= 2)
                {
                    return s == parts[0];
                }
            }
            return false;
        }
    }
}