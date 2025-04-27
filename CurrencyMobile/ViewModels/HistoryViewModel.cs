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
    public partial class HistoryViewModel : BaseViewModel
    {
        private readonly ICurrencyServiceClient _serviceClient;

        [ObservableProperty]
        private ObservableCollection<TransactionViewModel> _transactions = new();

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private bool _isError;

        public HistoryViewModel(ICurrencyServiceClient serviceClient)
        {
            Title = "Transaction History";
            _serviceClient = serviceClient;
        }

        [RelayCommand]
        private async Task LoadTransactionsAsync()
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

                var transactions = await _serviceClient.GetTransactionsAsync(userId);
                var viewModels = new ObservableCollection<TransactionViewModel>();

                foreach (var tx in transactions)
                {
                    viewModels.Add(new TransactionViewModel(tx));
                }

                Transactions = viewModels;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load transactions: {ex.Message}";
                IsError = true;
            }
            finally
            {
                IsRefreshing = false;
            }
        }
    }

    // Helper class for displaying transactions in the UI
    public partial class TransactionViewModel : ObservableObject
    {
        [ObservableProperty]
        private Transaction _transaction;

        public TransactionViewModel(Transaction transaction)
        {
            Transaction = transaction;
        }

        public string Id => Transaction.Id;
        public string Type => Transaction.Type;
        public string CurrencyCode => Transaction.CurrencyCode;
        public decimal Amount => Transaction.Amount;
        public DateTime Timestamp => Transaction.Timestamp;

        public string FormattedAmount => $"{Amount:N2} {CurrencyCode}";
        public string FormattedDate => Timestamp.ToString("yyyy-MM-dd HH:mm");
        public string TransactionType => Type == "buy" ? "Bought" : "Sold";
        public string Description => $"{TransactionType} {FormattedAmount}";
    }
}