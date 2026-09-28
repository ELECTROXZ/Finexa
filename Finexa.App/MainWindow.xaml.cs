using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Finexa_App.Services;
using Finexa_App.Views;
using Finexa.Core.Services;
using Finexa.Services.Themes;

namespace Finexa_App
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            // Bind Navigation Service Frame
            var navService = App.Services.GetRequiredService<INavigationService>();
            navService.Frame = RootFrame;

            // Subscribe to Theme changes
            var themeService = App.Services.GetRequiredService<IThemeSelectorService>();
            if (themeService is ThemeSelectorService selector)
            {
                selector.ThemeChanged += OnThemeChanged;
                ApplyTheme(selector.CurrentTheme);
            }

            // Start by navigating to the LoginPage
            navService.Navigate(typeof(LoginPage));
        }

        private void ApplyTheme(string themeName)
        {
            App.ApplyThemeBrushes(themeName);

            if (Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = themeName switch
                {
                    "Light" => ElementTheme.Light,
                    "Dark" => ElementTheme.Dark,
                    "AMOLED" => ElementTheme.Dark, // AMOLED uses dark brushes with customized pure black styling
                    _ => ElementTheme.Default
                };
            }
        }

        private void OnThemeChanged(object? sender, string newTheme)
        {
            ApplyTheme(newTheme);
        }
    }
}
