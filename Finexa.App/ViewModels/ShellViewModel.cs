using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finexa.Core.Services;
using Finexa_App.Services;
using Finexa_App.Views;
using System.Threading.Tasks;

namespace Finexa_App.ViewModels
{
    public partial class ShellViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly IThemeSelectorService _themeService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string _userFullName = "Aryan Singh";

        [ObservableProperty]
        private string _userRole = "Administrator";

        [ObservableProperty]
        private string _currentThemeName = "Dark";

        public ShellViewModel(
            IAuthService authService, 
            IThemeSelectorService themeService, 
            INavigationService navigationService)
        {
            _authService = authService;
            _themeService = themeService;
            _navigationService = navigationService;

            if (_authService.CurrentUser != null)
            {
                UserFullName = _authService.CurrentUser.FullName;
                UserRole = _authService.CurrentUser.Role;
            }

            CurrentThemeName = _themeService.CurrentTheme;
        }

        [RelayCommand]
        private void Logout()
        {
            _authService.Logout();
            _navigationService.Navigate(typeof(LoginPage));
        }

        [RelayCommand]
        private async Task ToggleThemeAsync()
        {
            string nextTheme = CurrentThemeName switch
            {
                "Light" => "Dark",
                "Dark" => "AMOLED",
                _ => "Light"
            };

            await _themeService.SetThemeAsync(nextTheme);
            CurrentThemeName = nextTheme;
        }
    }
}
