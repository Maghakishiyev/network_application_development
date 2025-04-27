using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CurrencyMobile.Models;
using CurrencyMobile.Services;
using Microsoft.Maui.Controls;

namespace CurrencyMobile.ViewModels
{
    public partial class TradeViewModel : BaseViewModel
    {
        private readonly ICurrencyServiceClient _serviceClient;

        [ObservableProperty]
        private ObservableCollection<string> _availableCurrencies = new();

        [ObservableProperty]
        private string _selectedCurrency = "USD";

        [ObservableProperty]
        private decimal _amountPln;

        [ObservableProperty]
        private decimal _amountForeign;

        [ObservableProperty]
        private bool _isBuying = true;

        [ObservableProperty]
        private decimal _currentRate;

        [ObservableProperty]
        private BuySellDto _buySellRate;

        [ObservableProperty]
        private TradeResultDto _lastTrade;

        [ObservableProperty]
        private AccountDto _currentBalance;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private bool _isError;

        [ObservableProperty]
        private bool _isSuccess;

        [ObservableProperty]
        private string _successMessage;

        public TradeViewModel(ICurrencyServiceClient serviceClient)
        {
            Title = "Buy & Sell Currency";
            _serviceClient = serviceClient;

            // Add common currencies
            AvailableCurrencies = new ObservableCollection<string>
            {
                "PLN", "USD", "EUR", "GBP", "CHF", "JPY", "CAD", "AUD"
            };
        }

        [RelayCommand]
        private async Task LoadRatesAsync()
        {
            if (string.IsNullOrEmpty(SelectedCurrency))
                return;

            try
            {
                IsBusy = true;
                IsError = false;

                CurrentRate = await _serviceClient.GetCurrentRateAsync(SelectedCurrency);

                if (SelectedCurrency != "PLN")
                {
                    BuySellRate = await _serviceClient.GetBuySellRateAsync(SelectedCurrency);
                }
                else
                {
                    BuySellRate = new BuySellDto { Bid = 1.0m, Ask = 1.0m };
                }

                // Reload balance
                await LoadBalanceAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load rates: {ex.Message}";
                IsError = true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task LoadBalanceAsync()
        {
            try
            {
                var userId = await SecureStorage.Default.GetAsync("user_id");
                if (string.IsNullOrEmpty(userId))
                {
                    await Shell.Current.GoToAsync("//login");
                    return;
                }

                IsBusy = true;
                CurrentBalance = await _serviceClient.GetAccountAsync(userId);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load balance: {ex.Message}";
                IsError = true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task CalculateAmountAsync(string direction)
        {
            if (string.IsNullOrEmpty(SelectedCurrency))
                return;

            if (direction == "pln_to_foreign")
            {
                if (AmountPln <= 0) return;
                AmountForeign = AmountPln / CurrentRate;
            }
            else
            {
                if (AmountForeign <= 0) return;
                AmountPln = AmountForeign * CurrentRate;
            }
        }

        [RelayCommand]
        private async Task ExecuteTradeAsync()
        {
            var userId = await SecureStorage.Default.GetAsync("user_id");
            if (string.IsNullOrEmpty(userId))
            {
                await Shell.Current.GoToAsync("//login");
                return;
            }

            try
            {
                IsBusy = true;
                IsError = false;
                IsSuccess = false;

                if (IsBuying)
                {
                    // We're buying foreign currency with PLN
                    if (AmountPln <= 0)
                    {
                        ErrorMessage = "Please enter a valid PLN amount";
                        IsError = true;
                        return;
                    }

                    // Check if we have enough PLN
                    if (CurrentBalance?.Balances.TryGetValue("PLN", out var plnBalance) == true 
                        && plnBalance < AmountPln && SelectedCurrency != "PLN")
                    {
                        ErrorMessage = $"Insufficient PLN balance. You have {plnBalance} PLN";
                        IsError = true;
                        return;
                    }

                    LastTrade = await _serviceClient.BuyCurrencyAsync(userId, SelectedCurrency, AmountPln);
                    SuccessMessage = $"Successfully bought {LastTrade.AmountForeign:N2} {LastTrade.CurrencyCode} for {LastTrade.AmountPln:N2} PLN";
                }
                else
                {
                    // We're selling foreign currency for PLN
                    if (AmountForeign <= 0)
                    {
                        ErrorMessage = "Please enter a valid amount to sell";
                        IsError = true;
                        return;
                    }

                    // Check if we have enough foreign currency
                    if (CurrentBalance?.Balances.TryGetValue(SelectedCurrency, out var foreignBalance) == true 
                        && foreignBalance < AmountForeign)
                    {
                        ErrorMessage = $"Insufficient {SelectedCurrency} balance. You have {foreignBalance} {SelectedCurrency}";
                        IsError = true;
                        return;
                    }

                    LastTrade = await _serviceClient.SellCurrencyAsync(userId, SelectedCurrency, AmountForeign);
                    SuccessMessage = $"Successfully sold {LastTrade.AmountForeign:N2} {LastTrade.CurrencyCode} for {LastTrade.AmountPln:N2} PLN";
                }

                IsSuccess = true;
                
                // Reset amounts
                AmountPln = 0;
                AmountForeign = 0;
                
                // Reload balance
                await LoadBalanceAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Trade failed: {ex.Message}";
                IsError = true;
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        partial void OnSelectedCurrencyChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                LoadRatesCommand.Execute(null);
            }
        }
        
        partial void OnIsBuyingChanged(bool value)
        {
            // Reset values when switching between buy/sell
            AmountPln = 0;
            AmountForeign = 0;
        }
    }
}