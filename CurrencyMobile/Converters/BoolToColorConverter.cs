using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace CurrencyMobile.Converters
{
    public class BoolToColorConverter : BaseValueConverter
    {
        public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b && parameter is string choices)
            {
                var parts = choices.Split(';');
                if (parts.Length >= 2)
                {
                    return Color.FromArgb(b ? parts[0] : parts[1]);
                }
            }
            return Colors.Black;
        }

        public override object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // No need for back conversion in this case
            return false;
        }
    }
}