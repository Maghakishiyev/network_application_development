using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using CurrencyMobile.Models;

namespace CurrencyMobile.Converters
{
    public class AccountBalanceConverter : BaseValueConverter
    {
        public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is AccountDto account && parameter is string currencyCode)
            {
                if (account.Balances.TryGetValue(currencyCode, out var balance))
                {
                    // Return a tuple of (amount, currency)
                    return (balance, currencyCode);
                }
                // No balance for this currency
                return (0m, currencyCode);
            }
            return (0m, "???");
        }

        public override object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Not needed
            return null;
        }
    }
}