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

        private decimal _amountPln;
        public decimal AmountPln
        {
            get => _amountPln;
            set 
            {
                // Safe handling of decimal parsing
                try
                {
                    decimal newValue = value;
                    if (SetProperty(ref _amountPln, Math.Round(newValue, 4)))
                    {
                        // Auto-calculate foreign amount when PLN amount changes
                        if (!_isCalculating && _amountPln > 0)
                        {
                            CalculateAmountAsync("pln_to_foreign").ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception)
                {
                    // If there's an error converting to decimal, just set to 0
                    _amountPln = 0;
                    OnPropertyChanged(nameof(AmountPln));
                }
            }
        }

        private decimal _amountForeign;
        public decimal AmountForeign
        {
            get => _amountForeign;
            set 
            {
                try
                {
                    decimal newValue = value;
                    if (SetProperty(ref _amountForeign, Math.Round(newValue, 4)))
                    {
                        // Auto-calculate PLN amount when foreign amount changes
                        if (!_isCalculating && _amountForeign > 0)
                        {
                            CalculateAmountAsync("foreign_to_pln").ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception)
                {
                    // If there's an error converting to decimal, just set to 0
                    _amountForeign = 0;
                    OnPropertyChanged(nameof(AmountForeign));
                }
            }
        }
        
        // Flag to prevent calculation loops
        private bool _isCalculating = false;

        [ObservableProperty]
        private bool _isBuying = true;

        [ObservableProperty]
        private decimal _currentRate;

        [ObservableProperty]
        private BuySellDto _buySellRate = new();
        
        // This computed property will return the appropriate rate (Ask or Bid) based on the trade direction
        public decimal TradeRate => IsBuying ? BuySellRate.Ask : BuySellRate.Bid;
        
        // This property creates the appropriate text for the trade button
        public string TradeButtonText => $"{(IsBuying ? "Buy" : "Sell")} {SelectedCurrency}";
        
        // This property creates the appropriate text for the trade amount label
        public string TradeAmountLabel => $"{(IsBuying ? "Amount to Buy" : "Amount to Sell")} {SelectedCurrency}";
        
        // This property gets the balance for the selected currency
        public decimal CurrencyBalance
        {
            get
            {
                if (CurrentBalance?.Balances == null || string.IsNullOrEmpty(SelectedCurrency))
                    return 0;
                    
                if (CurrentBalance.Balances.TryGetValue(SelectedCurrency, out decimal balance))
                    return balance;
                    
                return 0;
            }
        }

        [ObservableProperty]
        private TradeResultDto _lastTrade = new();

        private AccountDto _currentBalance = new();
        public AccountDto CurrentBalance
        {
            get => _currentBalance;
            set
            {
                if (SetProperty(ref _currentBalance, value))
                {
                    // Notify that CurrencyBalance property has changed
                    OnPropertyChanged(nameof(CurrencyBalance));
                }
            }
        }

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _isError;

        [ObservableProperty]
        private bool _isSuccess;

        [ObservableProperty]
        private string _successMessage = string.Empty;

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
        private Task CalculateAmountAsync(string direction)
        {
            if (string.IsNullOrEmpty(SelectedCurrency))
                return Task.CompletedTask;
                
            try
            {
                _isCalculating = true;
                decimal rate = IsBuying ? BuySellRate.Ask : BuySellRate.Bid;
                
                // Use the current rate if buy/sell rate isn't available
                if (rate <= 0)
                    rate = CurrentRate;
                
                // Ensure we have a valid rate
                if (rate <= 0)
                {
                    _isCalculating = false;
                    return Task.CompletedTask;
                }
                
                if (direction == "pln_to_foreign")
                {
                    if (AmountPln <= 0) 
                    {
                        // If PLN amount is zero, set foreign amount to zero as well
                        if (AmountForeign != 0)
                        {
                            AmountForeign = 0;
                        }
                        return Task.CompletedTask;
                    }
                    
                    try
                    {
                        // When buying/selling use the correct rate (ask for buying, bid for selling)
                        decimal foreignAmount = AmountPln / rate;
                        AmountForeign = Math.Round(foreignAmount, 4);
                    }
                    catch (Exception)
                    {
                        // In case of any errors, set to zero
                        AmountForeign = 0;
                    }
                }
                else
                {
                    if (AmountForeign <= 0)
                    {
                        // If foreign amount is zero, set PLN amount to zero as well
                        if (AmountPln != 0)
                        {
                            AmountPln = 0;
                        }
                        return Task.CompletedTask;
                    }
                    
                    try
                    {
                        decimal plnAmount = AmountForeign * rate;
                        AmountPln = Math.Round(plnAmount, 4);
                    }
                    catch (Exception)
                    {
                        // In case of any errors, set to zero
                        AmountPln = 0;
                    }
                }
            }
            catch (Exception)
            {
                // If any uncaught exception occurs, reset both values
                AmountPln = 0;
                AmountForeign = 0;
            }
            finally
            {
                _isCalculating = false;
            }
            
            return Task.CompletedTask;
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
        
        partial void OnIsBuyingChanged(bool value)
        {
            // Reset values when switching between buy/sell
            AmountPln = 0;
            AmountForeign = 0;
            
            // Notify that derived properties may have changed
            OnPropertyChanged(nameof(TradeRate));
            OnPropertyChanged(nameof(TradeButtonText));
            OnPropertyChanged(nameof(TradeAmountLabel));
        }
        
        partial void OnBuySellRateChanged(BuySellDto value)
        {
            // Notify that TradeRate may have changed
            OnPropertyChanged(nameof(TradeRate));
        }
        
        partial void OnSelectedCurrencyChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                // Reload rates for the new currency
                LoadRatesCommand.Execute(null);
                
                // Update derived property texts
                OnPropertyChanged(nameof(TradeButtonText));
                OnPropertyChanged(nameof(TradeAmountLabel));
                OnPropertyChanged(nameof(CurrencyBalance));
            }
        }
    }
}