using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace CurrencyMobile.Converters
{
    public class RateToValueConverter : BaseValueConverter
    {
        public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is decimal rate && parameter is string currencyCode)
            {
                // Return a tuple of (currency, formattedRate)
                return (currencyCode, rate);
            }
            return ("???", 0m);
        }

        public override object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Not needed
            return 0m;
        }
    }
}