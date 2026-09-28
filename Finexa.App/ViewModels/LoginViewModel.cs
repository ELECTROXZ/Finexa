using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finexa.Core.Services;
using Finexa_App.Services;
using Finexa_App.Views;

namespace Finexa_App.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public LoginViewModel(IAuthService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Username and password cannot be empty.";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            // Small delay for smooth transition experience
            await Task.Delay(600);

            bool success = await _authService.LoginAsync(Username, Password);
            IsLoading = false;

            if (success)
            {
                _navigationService.Navigate(typeof(ShellPage));
            }
            else
            {
                ErrorMessage = "Invalid username or password.";
            }
        }
    }
}
