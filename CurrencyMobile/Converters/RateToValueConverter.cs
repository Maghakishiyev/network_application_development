using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace CurrencyMobile.Converters
{
    public class RateToValueConverter : BaseValueConverter
    {
        public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is decimal rate && parameter != null)
            {
                string currencyCode;
                
                // Handle string parameters 
                if (parameter is string code)
                {
                    currencyCode = code;
                }
                else
                {
                    // For any other parameter type, just use its string representation
                    currencyCode = parameter.ToString() ?? "???";
                }
                
                // For PLN which is the base currency
                if (currencyCode == "PLN")
                {
                    return (currencyCode, 1.0m);
                }
                
                // For other currencies, return the appropriate tuple
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