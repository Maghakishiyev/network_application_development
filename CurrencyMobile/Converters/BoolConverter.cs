using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace CurrencyMobile.Converters
{
    public class BoolConverter : BaseValueConverter
    {
        public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Converts various types to booleans:
            // - Integers: 0 → false, otherwise → true
            // - Strings: null/empty → false, otherwise → true
            // - Collections: empty → false, otherwise → true
            if (value == null)
                return false;

            if (value is int intValue)
                return intValue != 0;
                
            if (value is string strValue)
                return !string.IsNullOrEmpty(strValue);
                
            if (value is System.Collections.ICollection collection)
                return collection.Count > 0;
                
            return true;
        }

        public override object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is bool b && b;
        }
    }
}