using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Finexa.Domain.Entities;
using Finexa_App.ViewModels;
using System;

namespace Finexa_App.Views
{
    public sealed partial class InvoiceEditorPage : Page
    {
        public InvoiceEditorViewModel ViewModel { get; }

        public InvoiceEditorPage()
        {
            ViewModel = App.Services.GetRequiredService<InvoiceEditorViewModel>();
            this.InitializeComponent();
            this.DataContext = this;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            int? invoiceId = null;
            if (e.Parameter is int id)
            {
                invoiceId = id;
            }

            await ViewModel.InitializeAsync(invoiceId);
            
            // Forces UI update of properties
            this.Bindings.Update();
        }

        public static bool HasErrorMessage(string msg)
        {
            return !string.IsNullOrEmpty(msg);
        }

        private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is InvoiceItem item)
            {
                ViewModel.RemoveItemCommand.Execute(item);
            }
        }

        private void ItemGridCell_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && tb.DataContext is InvoiceItem item)
            {
                // Force update bound entity values
                ViewModel.RecalculateItemTotals(item);
                ViewModel.RecalculateInvoiceTotals();
                
                // Redraw item row
                ItemsGrid.CommitEdit();
            }
        }

        private void ShippingCharges_LostFocus(object sender, RoutedEventArgs e)
        {
            ViewModel.RecalculateInvoiceTotals();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CancelCommand.Execute(null);
        }
    }
}
