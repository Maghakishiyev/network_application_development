using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace CurrencyMobile.Converters
{
    // Base interfaces for converters with correct nullable annotations
    public interface IValueConverterBase
    {
        object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture);
        object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture);
    }
    
    // Base class for converters to inherit from
    public abstract class BaseValueConverter : IValueConverter
    {
        public virtual object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
        public virtual object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
    }
}