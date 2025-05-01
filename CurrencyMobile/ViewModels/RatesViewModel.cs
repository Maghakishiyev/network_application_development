using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CurrencyMobile.Models;
using CurrencyMobile.Services;
using Microsoft.Maui.Controls;

namespace CurrencyMobile.ViewModels
{
    public partial class RatesViewModel : BaseViewModel
    {
        private readonly ICurrencyServiceClient _serviceClient;

        [ObservableProperty]
        private ObservableCollection<RateViewModel> _historicalRates = new();

        [ObservableProperty]
        private ObservableCollection<string> _availableCurrencies = new();

        [ObservableProperty]
        private string _selectedCurrency = "USD";

        [ObservableProperty]
        private DateTime _startDate = DateTime.Now.AddDays(-30);

        [ObservableProperty]
        private DateTime _endDate = DateTime.Now;

        [ObservableProperty]
        private decimal _currentRate;

        [ObservableProperty]
        private BuySellDto _buySellRate = new();

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _isError;

        public RatesViewModel(ICurrencyServiceClient serviceClient)
        {
            Title = "Exchange Rates";
            _serviceClient = serviceClient;

            // Add common currencies
            AvailableCurrencies = new ObservableCollection<string>
            {
                "USD", "EUR", "GBP", "CHF", "JPY", "CAD", "AUD"
            };
        }

        [RelayCommand]
        private async Task LoadCurrentRateAsync()
        {
            if (string.IsNullOrEmpty(SelectedCurrency))
                return;

            try
            {
                IsBusy = true;
                IsError = false;

                CurrentRate = await _serviceClient.GetCurrentRateAsync(SelectedCurrency);
                BuySellRate = await _serviceClient.GetBuySellRateAsync(SelectedCurrency);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load current rate: {ex.Message}";
                IsError = true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task LoadHistoricalRatesAsync()
        {
            if (string.IsNullOrEmpty(SelectedCurrency) || StartDate > EndDate)
                return;

            try
            {
                IsBusy = true;
                IsError = false;

                var rates = await _serviceClient.GetHistoricalRatesAsync(SelectedCurrency, StartDate, EndDate);
                var rateViewModels = rates.Select(r => new RateViewModel(r)).ToList();
                HistoricalRates = new ObservableCollection<RateViewModel>(rateViewModels);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load historical rates: {ex.Message}";
                IsError = true;
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        // Helper class for displaying rates in the UI
        public partial class RateViewModel : ObservableObject
        {
            [ObservableProperty]
            private DateTime _date;
            
            [ObservableProperty]
            private decimal _mid;
            
            public RateViewModel(RateDto dto)
            {
                Date = dto.Date;
                Mid = dto.Mid;
            }
        }

        partial void OnSelectedCurrencyChanged(string value)
        {
            // When currency changes, reload data
            if (!string.IsNullOrEmpty(value))
            {
                LoadCurrentRateCommand.Execute(null);
                LoadHistoricalRatesCommand.Execute(null);
            }
        }
        
        partial void OnStartDateChanged(DateTime value)
        {
            // When start date changes, reload historical data
            if (!string.IsNullOrEmpty(SelectedCurrency))
            {
                LoadHistoricalRatesCommand.Execute(null);
            }
        }
        
        partial void OnEndDateChanged(DateTime value)
        {
            // When end date changes, reload historical data
            if (!string.IsNullOrEmpty(SelectedCurrency))
            {
                LoadHistoricalRatesCommand.Execute(null);
            }
        }
    }
}