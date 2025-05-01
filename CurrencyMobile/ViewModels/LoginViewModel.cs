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
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;
        
        [ObservableProperty]
        private string _confirmPassword = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _isError;

        [ObservableProperty]
        private bool _isRegistration;

        public LoginViewModel(ICurrencyServiceClient serviceClient)
        {
            Title = "Login";
            _serviceClient = serviceClient;
        }

        [RelayCommand]
        private void ToggleRegistration()
        {
            Console.WriteLine($"Toggle registration called. Current IsRegistration: {IsRegistration}");
            
            // Toggle the registration flag
            IsRegistration = !IsRegistration;
            
            Console.WriteLine($"IsRegistration after toggle: {IsRegistration}");
            
            // Update title
            Title = IsRegistration ? "Register" : "Login";
            
            // Clear any previous errors when toggling
            IsError = false;
            
            // Always clear confirm password when toggling
            if (!IsRegistration)
            {
                // Only clear confirm password when switching to login
                ConfirmPassword = string.Empty;
            }
        }

        [RelayCommand]
        private async Task SubmitAsync()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Please enter an email address";
                IsError = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter a password";
                IsError = true;
                return;
            }

            if (IsRegistration)
            {
                // Check password length
                if (Password.Length < 6)
                {
                    ErrorMessage = "Password must be at least 6 characters long";
                    IsError = true;
                    return;
                }
                
                // Check if passwords match
                if (Password != ConfirmPassword)
                {
                    ErrorMessage = "Passwords do not match";
                    IsError = true;
                    return;
                }
            }

            try
            {
                IsBusy = true;
                IsError = false;
                
                if (IsRegistration)
                {
                    // Register a new user
                    var newUser = await _serviceClient.RegisterUserAsync(Email, Password);
                    
                    // Save the user data
                    await SecureStorage.Default.SetAsync("user_id", newUser.Id);
                    await SecureStorage.Default.SetAsync("user_email", newUser.Email);
                    await SecureStorage.Default.SetAsync("user_name", newUser.Username);
                    
                    // Navigate to the main page
                    await Shell.Current.GoToAsync("//balance");
                }
                else
                {
                    // Login existing user
                    var user = await _serviceClient.AuthenticateAsync(Email, Password);
                    
                    // Save the user data
                    await SecureStorage.Default.SetAsync("user_id", user.Id);
                    await SecureStorage.Default.SetAsync("user_email", user.Email);
                    await SecureStorage.Default.SetAsync("user_name", user.Username);
                    
                    // Navigate to the main page
                    await Shell.Current.GoToAsync("//balance");
                }
            }
            catch (Exception ex)
            {
                string action = IsRegistration ? "Registration" : "Login";
                ErrorMessage = $"{action} failed: {ex.Message}";
                IsError = true;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}