using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Extensions.DependencyInjection;
using Finexa_App.ViewModels;
using System;

namespace Finexa_App.Views
{
    public sealed partial class DashboardPage : Page
    {
        public DashboardViewModel ViewModel { get; }

        public DashboardPage()
        {
            ViewModel = App.Services.GetRequiredService<DashboardViewModel>();
            this.InitializeComponent();

            this.Loaded += DashboardPage_Loaded;
        }

        private async void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await ViewModel.LoadDashboardDataAsync();
            }
            catch (Exception)
            {
                // Load error handled and logged by EF/DI
            }
        }

        // Static Formatting Helpers for compiled data templates
        public static string FormatCurrency(decimal amount)
        {
            return $"₹{amount:N2}";
        }

        public static string FormatAmount(decimal amount, string type)
        {
            return type == "Credit" ? $"+ ₹{amount:N2}" : $"- ₹{amount:N2}";
        }

        public static Brush GetTransactionColor(string type)
        {
            return type == "Credit" 
                ? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 76, 175, 80)) 
                : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 244, 67, 54)); 
        }

        public static Brush GetTransactionBg(string type)
        {
            return type == "Credit" 
                ? new SolidColorBrush(Windows.UI.Color.FromArgb(40, 76, 175, 80)) 
                : new SolidColorBrush(Windows.UI.Color.FromArgb(40, 244, 67, 54)); 
        }

        public static string GetTransactionGlyph(string type)
        {
            return type == "Credit" ? "\uE1FD" : "\uE110";
        }

        public static string FormatStock(decimal stock, string unit)
        {
            string unitStr = string.IsNullOrEmpty(unit) ? "Pcs" : unit;
            return $"{stock:N0} {unitStr}";
        }

        public static string FormatMinStock(decimal minStock)
        {
            return $"Min: {minStock:N0}";
        }
    }
}
