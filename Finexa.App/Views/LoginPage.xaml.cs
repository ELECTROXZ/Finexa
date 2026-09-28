using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.Extensions.DependencyInjection;
using Finexa_App.ViewModels;

namespace Finexa_App.Views
{
    public sealed partial class LoginPage : Page
    {
        public LoginViewModel ViewModel { get; }

        public LoginPage()
        {
            ViewModel = App.Services.GetRequiredService<LoginViewModel>();
            this.InitializeComponent();
            
            // Subscribe to ViewModel property changes for secure programmatic UI updates
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            
            // Set initial state and apply colors when loaded
            Loaded += (s, e) => {
                ApplyThemeColors();
                UpdateUIState();
            };
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LoginViewModel.IsLoading) || 
                e.PropertyName == nameof(LoginViewModel.ErrorMessage))
            {
                // Ensure UI updates are enqueued on the UI thread
                this.DispatcherQueue.TryEnqueue(UpdateUIState);
            }
        }

        private void ApplyThemeColors()
        {
            // Apply background directly to Page
            if (App.Current.Resources.TryGetValue("FinexaBackgroundBrush", out object bgBrush) && bgBrush is Brush background)
            {
                this.Background = background;
            }

            // Find the Border control programmatically by traversing the layout children
            if (this.Content is Grid rootGrid)
            {
                foreach (var child in rootGrid.Children)
                {
                    if (child is StackPanel mainStack)
                    {
                        foreach (var innerChild in mainStack.Children)
                        {
                            if (innerChild is Border cardBorder)
                            {
                                if (App.Current.Resources.TryGetValue("FinexaCardBrush", out object cardBrush) && cardBrush is Brush cardBg)
                                {
                                    cardBorder.Background = cardBg;
                                }
                                if (App.Current.Resources.TryGetValue("FinexaBorderBrush", out object borderBrush) && borderBrush is Brush cardBdr)
                                {
                                    cardBorder.BorderBrush = cardBdr;
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void UpdateUIState()
        {
            if (ViewModel == null) return;

            // Programmatically update error message visibility
            bool hasError = !string.IsNullOrEmpty(ViewModel.ErrorMessage);
            ErrorTextBlock.Text = ViewModel.ErrorMessage;
            ErrorTextBlock.Visibility = hasError ? Visibility.Visible : Visibility.Collapsed;

            // Programmatically update button content states
            if (ViewModel.IsLoading)
            {
                var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, HorizontalAlignment = HorizontalAlignment.Center };
                panel.Children.Add(new ProgressRing { IsActive = true, Width = 16, Height = 16, Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)) });
                panel.Children.Add(new TextBlock { Text = "Logging in..." });
                LoginButton.Content = panel;
            }
            else
            {
                LoginButton.Content = "Log In";
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                ViewModel.Password = passwordBox.Password;
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (ViewModel.LoginCommand.CanExecute(null))
                {
                    ViewModel.LoginCommand.Execute(null);
                }
            }
        }
    }
}
