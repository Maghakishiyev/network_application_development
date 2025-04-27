using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using CurrencyMobile.Models;

namespace CurrencyMobile.Converters
{
    public class BoolToRateConverter : BaseValueConverter
    {
        public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isBuying && parameter is BuySellDto rates)
            {
                return isBuying ? rates.Ask : rates.Bid;
            }
            return 0m;
        }

        public override object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Not needed for this converter
            return false;
        }
    }
}