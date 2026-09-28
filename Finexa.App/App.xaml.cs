using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using Finexa.Core.Services;
using Finexa.Services.Themes;
using Finexa.Services.Auth;
using Finexa.Infrastructure.Database;
using Finexa.Infrastructure.Logging;
using Finexa_App.Services;
using Finexa_App.ViewModels;
using Finexa_App.Views;
using Serilog;

using Finexa.Services.Invoice;
using Finexa.Services.Party;
using Finexa.Reporting;

namespace Finexa_App
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;

        public static IServiceProvider Services { get; private set; } = null!;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("c:\\Users\\MSI 1\\Desktop\\Antigravity\\Finexa\\crash_init.txt", ex.ToString());
                throw;
            }

            try
            {
                // Initialize Logging
                LoggerConfig.ConfigureLogging();

                // Set up DI Container
                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);
                Services = serviceCollection.BuildServiceProvider();
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("c:\\Users\\MSI 1\\Desktop\\Antigravity\\Finexa\\crash_constructor.txt", ex.ToString());
                throw;
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Database Context
            services.AddDbContext<FinexaDbContext>();

            // Services
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddSingleton<IAuthService, AuthService>();
            services.AddTransient<IInvoiceService, InvoiceService>();
            services.AddTransient<IInvoiceExportService, InvoiceExportService>();
            services.AddTransient<IPartyService, PartyService>();
            services.AddTransient<IPartyImportExportService, PartyImportExportService>();

            // ViewModels
            services.AddTransient<LoginViewModel>();
            services.AddTransient<ShellViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<InvoicesViewModel>();
            services.AddTransient<InvoiceEditorViewModel>();
            services.AddTransient<PartiesViewModel>();
            services.AddTransient<PartyDetailsViewModel>();

            // Pages
            services.AddTransient<DashboardPage>();
            services.AddTransient<InvoicesPage>();
            services.AddTransient<InvoiceEditorPage>();
            services.AddTransient<LoginPage>();
            services.AddTransient<ShellPage>();
            services.AddTransient<PurchasesPage>();
            services.AddTransient<PartiesPage>();
            services.AddTransient<PartyProfilePage>();
            services.AddTransient<InventoryPage>();
            services.AddTransient<ExpensesPage>();
            services.AddTransient<PaymentsPage>();
            services.AddTransient<LedgerPage>();
            services.AddTransient<SettingsPage>();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            Log.Information("Application is launching.");

            try
            {
                // Run EF migrations and seed base data
                using (var scope = Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<FinexaDbContext>();
                    await DbInitializer.InitializeAsync(db);
                }
                Log.Information("Step 1: Database successfully initialized.");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "An error occurred while migrating/initializing the database.");
            }

            try
            {
                Log.Information("Step 2: Initializing ThemeSelectorService.");
                var themeService = Services.GetRequiredService<IThemeSelectorService>();
                await themeService.InitializeAsync();
                Log.Information("Step 3: ThemeSelectorService initialized. Current theme: {Theme}", themeService.CurrentTheme);
                ApplyThemeBrushes(themeService.CurrentTheme);
                Log.Information("Step 4: Custom theme brushes applied.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize ThemeSelectorService.");
            }

            // Create and activate MainWindow
            try
            {
                Log.Information("Step 5: Creating MainWindow.");
                _window = new MainWindow();
                App.Current.Resources["ActiveWindow"] = _window;
                Log.Information("Step 6: Activating MainWindow.");
                _window.Activate();
                Log.Information("Step 7: MainWindow activated successfully.");
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("c:\\Users\\MSI 1\\Desktop\\Antigravity\\Finexa\\crash_window.txt", ex.ToString());
                Log.Fatal(ex, "Crash during MainWindow creation/activation.");
                throw;
            }
        }

        public static void ApplyThemeBrushes(string themeName)
        {
            try
            {
                string path = themeName switch
                {
                    "Light" => "ms-appx:///Themes/LightTheme.xaml",
                    "Dark" => "ms-appx:///Themes/DarkTheme.xaml",
                    "AMOLED" => "ms-appx:///Themes/AmoledTheme.xaml",
                    _ => "ms-appx:///Themes/DarkTheme.xaml"
                };

                var dictUri = new Uri(path);
                var activeDict = new ResourceDictionary { Source = dictUri };

                foreach (var brushKey in activeDict.Keys)
                {
                    App.Current.Resources[brushKey] = activeDict[brushKey];
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load/apply custom theme brushes.");
            }
        }
    }
}
