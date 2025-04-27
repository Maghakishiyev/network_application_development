using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CurrencyMobile.Models;
using CurrencyMobile.Services;
using Microsoft.Maui.Controls;

namespace CurrencyMobile.ViewModels
{
    public partial class BalanceViewModel : BaseViewModel
    {
        private readonly ICurrencyServiceClient _serviceClient;

        [ObservableProperty]
        private ObservableCollection<CurrencyBalance> _balances = new();

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private bool _isError;

        public BalanceViewModel(ICurrencyServiceClient serviceClient)
        {
            Title = "Wallet";
            _serviceClient = serviceClient;
        }

        [RelayCommand]
        private async Task LoadBalancesAsync()
        {
            if (IsRefreshing)
                return;

            try
            {
                IsRefreshing = true;
                IsError = false;

                var userId = await SecureStorage.Default.GetAsync("user_id");
                if (string.IsNullOrEmpty(userId))
                {
                    await Shell.Current.GoToAsync("//login");
                    return;
                }

                var account = await _serviceClient.GetAccountAsync(userId);
                var balanceItems = new ObservableCollection<CurrencyBalance>();

                foreach (var balance in account.Balances)
                {
                    balanceItems.Add(new CurrencyBalance
                    {
                        CurrencyCode = balance.Key,
                        Amount = balance.Value,
                        IsPrimary = balance.Key == "PLN"
                    });
                }

                // Sort with PLN first, then others alphabetically
                var sorted = balanceItems
                    .OrderByDescending(b => b.IsPrimary)
                    .ThenBy(b => b.CurrencyCode);

                Balances = new ObservableCollection<CurrencyBalance>(sorted);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load balances: {ex.Message}";
                IsError = true;
            }
            finally
            {
                IsRefreshing = false;
            }
        }
    }

    // Helper class for displaying balances in the UI
    public partial class CurrencyBalance : ObservableObject
    {
        [ObservableProperty]
        private string _currencyCode;

        [ObservableProperty]
        private decimal _amount;

        [ObservableProperty]
        private bool _isPrimary;

        public string FormattedAmount => $"{Amount:N2} {CurrencyCode}";
    }
}