using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using Finexa_App.ViewModels;
using System;

namespace Finexa_App.Views
{
    public sealed partial class ShellPage : Page
    {
        private DispatcherTimer _timer;

        public ShellViewModel ViewModel { get; }

         public ShellPage()
         {
             ViewModel = App.Services.GetRequiredService<ShellViewModel>();
             this.InitializeComponent();
 
             // Default selection
             NavigationMenu.SelectedItem = NavigationMenu.MenuItems[0];
 
             // Start real-time clock on status bar
             StartClock();

             // Auto verify navigation
             this.Loaded += ShellPage_Loaded;
         }

         private void ShellPage_Loaded(object sender, RoutedEventArgs e)
         {
             RunAutoVerification();
         }

         private void RunAutoVerification()
         {
             var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
             System.Threading.Tasks.Task.Run(async () =>
             {
                 await System.Threading.Tasks.Task.Delay(3000);
                 
                 dispatcherQueue.TryEnqueue(async () =>
                 {
                     Serilog.Log.Information("=== STARTING AUTOMATED SIDEBAR NAVIGATION VERIFICATION ===");
                     
                     var items = new System.Collections.Generic.List<NavigationViewItem>();
                     foreach (var menuObj in NavigationMenu.MenuItems)
                     {
                         if (menuObj is NavigationViewItem item)
                         {
                             items.Add(item);
                         }
                     }
                     if (NavigationMenu.SettingsItem is NavigationViewItem settings)
                     {
                         items.Add(settings);
                     }
 
                     foreach (var item in items)
                     {
                         string tag = item.Tag?.ToString() ?? "settings";
                         if (tag == "theme" || tag == "logout") continue;
                         
                         Serilog.Log.Information("Auto-selecting item: {Content} (Tag: {Tag})", item.Content, tag);
                         NavigationMenu.SelectedItem = item;
                         
                         await System.Threading.Tasks.Task.Delay(1000);
                     }
                     
                     // Reset to Dashboard
                     if (NavigationMenu.MenuItems.Count > 0)
                     {
                         NavigationMenu.SelectedItem = NavigationMenu.MenuItems[0];
                     }
                     
                     Serilog.Log.Information("=== AUTOMATED SIDEBAR NAVIGATION VERIFICATION COMPLETED ===");
                 });
             });
         }

        private void StartClock()
        {
            UpdateSystemTime();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) => UpdateSystemTime();
            _timer.Start();
        }

        private void UpdateSystemTime()
        {
            if (SystemTimeTextBlock != null)
            {
                SystemTimeTextBlock.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy  -  hh:mm:ss tt");
            }
        }

        private async void NavigationMenu_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            Type? targetPageType = null;
            string headerText = string.Empty;

            try
            {
                if (args.IsSettingsSelected)
                {
                    targetPageType = typeof(SettingsPage);
                    headerText = "Settings";
                }
                else if (args.SelectedItemContainer is NavigationViewItem item)
                {
                    string tag = item.Tag?.ToString() ?? string.Empty;
                    headerText = item.Content?.ToString() ?? "Module";

                    switch (tag)
                    {
                        case "dashboard":
                            targetPageType = typeof(DashboardPage);
                            break;
                        case "sales":
                            targetPageType = typeof(InvoicesPage);
                            break;
                        case "purchases":
                            targetPageType = typeof(PurchasesPage);
                            break;
                        case "parties":
                            targetPageType = typeof(PartiesPage);
                            break;
                        case "inventory":
                            targetPageType = typeof(InventoryPage);
                            break;
                        case "expenses":
                            targetPageType = typeof(ExpensesPage);
                            break;
                        case "payments":
                            targetPageType = typeof(PaymentsPage);
                            break;
                        case "ledger":
                            targetPageType = typeof(LedgerPage);
                            break;
                        case "theme":
                            Serilog.Log.Information("Theme button clicked. Command execution started.");
                            await ViewModel.ToggleThemeCommand.ExecuteAsync(null);
                            Serilog.Log.Information("Theme command execution completed.");
                            return;
                        case "logout":
                            Serilog.Log.Information("Logout button clicked. Command execution started.");
                            ViewModel.LogoutCommand.Execute(null);
                            Serilog.Log.Information("Logout command execution completed.");
                            return;
                    }
                }

                if (targetPageType != null)
                {
                    Serilog.Log.Information("Navigation requested for page: {PageName} (Header: {Header})", targetPageType.Name, headerText);
                    
                    // Log Navigation Started
                    Serilog.Log.Information("Navigation started: {PageName}", targetPageType.Name);
                    
                    bool navigated = ContentFrame.Navigate(targetPageType);
                    
                    if (navigated)
                    {
                        sender.Header = headerText;
                        // Log Navigation Completed
                        Serilog.Log.Information("Navigation completed successfully: {PageName}", targetPageType.Name);
                    }
                    else
                    {
                        // Log Navigation Failed
                        Serilog.Log.Warning("Navigation failed (Navigate returned false): {PageName}", targetPageType.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log Exception Details
                Serilog.Log.Fatal(ex, "Navigation failed with exception for target page type: {PageName}", targetPageType?.Name ?? "Unknown");
                System.IO.File.WriteAllText("c:\\Users\\MSI 1\\Desktop\\Antigravity\\Finexa\\crash_navigation.txt", ex.ToString());
                throw; // Do not swallow the exception!
            }
        }

        private void ShowPlaceholder(string moduleName)
        {
            ContentFrame.Navigate(typeof(PlaceholderPage), moduleName);
        }

        private async void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.ToggleThemeCommand.ExecuteAsync(null);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.LogoutCommand.Execute(null);
        }
    }
}
