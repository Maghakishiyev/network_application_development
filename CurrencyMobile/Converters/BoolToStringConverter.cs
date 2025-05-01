using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace CurrencyMobile.Converters
{
    public class BoolToStringConverter : BaseValueConverter
    {
        public string TrueValue { get; set; } = "True";
        public string FalseValue { get; set; } = "False";
        
        public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                // If parameter is provided, use it to split for true/false values
                if (parameter is string choices)
                {
                    var parts = choices.Split('|');
                    if (parts.Length >= 2)
                    {
                        return b ? parts[0] : parts[1];
                    }
                }
                
                // Otherwise use the properties
                return b ? TrueValue : FalseValue;
            }
            return value?.ToString() ?? string.Empty;
        }

        public override object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                // If parameter is provided
                if (parameter is string choices)
                {
                    var parts = choices.Split('|');
                    if (parts.Length >= 2)
                    {
                        return s == parts[0];
                    }
                }
                
                // Otherwise compare with properties
                return s == TrueValue;
            }
            return false;
        }
    }
}