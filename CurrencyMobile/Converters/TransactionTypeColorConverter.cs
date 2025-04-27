using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace CurrencyMobile.Converters
{
    public class TransactionTypeColorConverter : BaseValueConverter
    {
        public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string type)
            {
                return type.ToLower() switch
                {
                    "buy" => Color.FromArgb("#10B981"),  // Green for buying
                    "sell" => Color.FromArgb("#EF4444"),  // Red for selling
                    _ => Colors.Gray
                };
            }
            return Colors.Gray;
        }

        public override object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Not needed
            return null;
        }
    }
}