using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CurrencyMobile.Services;
using Microsoft.Maui.Controls;

namespace CurrencyMobile.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly ICurrencyServiceClient _serviceClient;

        [ObservableProperty]
        private string _userId;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private bool _isError;

        public LoginViewModel(ICurrencyServiceClient serviceClient)
        {
            Title = "Login";
            _serviceClient = serviceClient;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                ErrorMessage = "Please enter a User ID";
                IsError = true;
                return;
            }

            try
            {
                IsBusy = true;
                IsError = false;
                
                // Try to get the account as a validation that the user exists
                var account = await _serviceClient.GetAccountAsync(UserId);
                
                // Save the user ID for other views
                await SecureStorage.Default.SetAsync("user_id", UserId);
                
                // Navigate to the main page
                await Shell.Current.GoToAsync("//balance");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login failed: {ex.Message}";
                IsError = true;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}