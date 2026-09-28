using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.Extensions.DependencyInjection;
using Finexa.Domain.Entities;
using Finexa_App.ViewModels;
using System;
using System.Threading.Tasks;

namespace Finexa_App.Views
{
    public sealed partial class InvoicesPage : Page
    {
        public InvoicesViewModel ViewModel { get; }

        public InvoicesPage()
        {
            ViewModel = App.Services.GetRequiredService<InvoicesViewModel>();
            this.InitializeComponent();
            
            this.Loaded += InvoicesPage_Loaded;
            
            // Listen for Invoices collection changes to toggle EmptyStateView
            ViewModel.Invoices.CollectionChanged += Invoices_CollectionChanged;
        }

        private async void InvoicesPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.InitializeAsync();
            UpdateEmptyStateVisibility();
        }

        private void Invoices_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateEmptyStateVisibility();
        }

        private void UpdateEmptyStateVisibility()
        {
            if (ViewModel.Invoices.Count == 0 && !ViewModel.IsLoading)
            {
                EmptyStateView.Visibility = Visibility.Visible;
                InvoicesGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmptyStateView.Visibility = Visibility.Collapsed;
                InvoicesGrid.Visibility = Visibility.Visible;
            }
        }

        public static bool IsInvoiceSelected(Invoice? selected)
        {
            return selected != null;
        }

        private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                _ = ViewModel.LoadInvoicesAsync();
            }
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.PageIndex = 0;
            _ = ViewModel.LoadInvoicesAsync();
        }

        private void DateFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (DateFilterCombo.SelectedValue is string val)
            {
                if (val == "Custom")
                {
                    CustomDatePanel.Visibility = Visibility.Visible;
                }
                else
                {
                    CustomDatePanel.Visibility = Visibility.Collapsed;
                    ViewModel.StartDate = null;
                    ViewModel.EndDate = null;
                    ViewModel.PageIndex = 0;
                    _ = ViewModel.LoadInvoicesAsync();
                }
            }
        }

        private void CustomDate_Changed(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            ViewModel.PageIndex = 0;
            _ = ViewModel.LoadInvoicesAsync();
        }

        private void PageSize_Changed(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.PageIndex = 0;
            _ = ViewModel.LoadInvoicesAsync();
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ResetFiltersCommand.Execute(null);
        }

        private void ShowTrash_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.PageIndex = 0;
            _ = ViewModel.LoadInvoicesAsync();
        }

        private void InvoicesGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (ViewModel.SelectedInvoice != null)
            {
                ViewModel.EditInvoiceCommand.Execute(null);
            }
        }

        private void InvoicesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Triggers property validation bindings
            this.Bindings.Update();
        }

        // Action Handlers
        private async void AddPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedInvoice == null) return;
            
            // Pre-fill maximum outstanding invoice amount
            PaymentAmountBox.Value = (double)(ViewModel.SelectedInvoice.TotalAmount - ViewModel.SelectedInvoice.InvoicePayments.Sum(p => p.Amount));
            if (PaymentAmountBox.Value <= 0) PaymentAmountBox.Value = 0.00;

            PaymentMethodCombo.SelectedIndex = 0;
            PaymentRefBox.Text = string.Empty;
            PaymentNotesBox.Text = string.Empty;

            await PaymentDialog.ShowAsync();
        }

        private async void PaymentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            decimal amt = (decimal)PaymentAmountBox.Value;
            string method = PaymentMethodCombo.SelectedValue?.ToString() ?? "Cash";
            string rRef = PaymentRefBox.Text;
            string notes = PaymentNotesBox.Text;

            if (amt <= 0)
            {
                args.Cancel = true;
                return;
            }

            var parameter = (amt, method, rRef, notes);
            await ViewModel.AddPaymentCommand.ExecuteAsync(parameter);
        }

        private void ContextEdit_Click(object sender, RoutedEventArgs e) => ViewModel.EditInvoiceCommand.Execute(null);
        private void ContextDuplicate_Click(object sender, RoutedEventArgs e) => _ = ViewModel.DuplicateInvoiceCommand.ExecuteAsync(null);
        private void ContextPrint_Click(object sender, RoutedEventArgs e) => _ = ViewModel.PrintInvoiceCommand.ExecuteAsync(null);
        private void ContextDelete_Click(object sender, RoutedEventArgs e) => ExecuteDeleteWithBanner();

        private async void ExecuteDeleteWithBanner()
        {
            await ViewModel.DeleteInvoiceCommand.ExecuteAsync(null);
            UndoInfoBar.IsOpen = true;
            // Automatically close banner after 8 seconds
            await Task.Delay(8000);
            UndoInfoBar.IsOpen = false;
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            UndoInfoBar.IsOpen = false;
        }

        private async void ExportPdf_Click(object sender, RoutedEventArgs e) => await ViewModel.ExportPdfCommand.ExecuteAsync(null);
        private async void ExportExcel_Click(object sender, RoutedEventArgs e) => await ViewModel.ExportExcelCommand.ExecuteAsync(null);
        private async void ExportCsv_Click(object sender, RoutedEventArgs e) => await ViewModel.ExportCsvCommand.ExecuteAsync(null);

        // UI Helpers
        public static string FormatCurrency(decimal amount)
        {
            return $"₹{amount:N2}";
        }

        public static Brush GetStatusColor(string status)
        {
            return status switch
            {
                "Paid" => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 76, 175, 80)),      // Green
                "Partial" => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 152, 0)),   // Orange
                "Unpaid" => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 244, 67, 54)),    // Red
                _ => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 158, 158, 158))         // Grey
            };
        }

        public static Brush GetStatusBg(string status)
        {
            return status switch
            {
                "Paid" => new SolidColorBrush(Windows.UI.Color.FromArgb(40, 76, 175, 80)),
                "Partial" => new SolidColorBrush(Windows.UI.Color.FromArgb(40, 255, 152, 0)),
                "Unpaid" => new SolidColorBrush(Windows.UI.Color.FromArgb(40, 244, 67, 54)),
                _ => new SolidColorBrush(Windows.UI.Color.FromArgb(40, 158, 158, 158))
            };
        }
    }
}
