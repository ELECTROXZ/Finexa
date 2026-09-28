using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finexa.Core.Services;
using Finexa.Domain.Entities;
using Finexa.Infrastructure.Database;
using Finexa_App.Services;
using Finexa_App.Views;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Finexa_App.ViewModels
{
    public partial class InvoiceEditorViewModel : ObservableObject
    {
        private readonly IInvoiceService _invoiceService;
        private readonly INavigationService _navigationService;
        private readonly IAuthService _authService;
        private readonly FinexaDbContext _dbContext;

        [ObservableProperty]
        private Invoice _invoice = new();

        [ObservableProperty]
        private ObservableCollection<InvoiceItem> _invoiceItems = new();

        [ObservableProperty]
        private ObservableCollection<Party> _parties = new();

        [ObservableProperty]
        private ObservableCollection<Product> _products = new();

        [ObservableProperty]
        private Party? _selectedParty;

        [ObservableProperty]
        private Product? _selectedProductToAdd;

        [ObservableProperty]
        private bool _isEditMode;

        [ObservableProperty]
        private string _title = "Create New Invoice";

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        partial void OnErrorMessageChanged(string value)
        {
            OnPropertyChanged(nameof(HasError));
        }

        [ObservableProperty]
        private bool _isSaving;

        public InvoiceEditorViewModel(
            IInvoiceService invoiceService,
            INavigationService navigationService,
            IAuthService authService,
            FinexaDbContext dbContext)
        {
            _invoiceService = invoiceService;
            _navigationService = navigationService;
            _authService = authService;
            _dbContext = dbContext;
        }

        public async Task InitializeAsync(int? invoiceId = null)
        {
            ErrorMessage = string.Empty;
            
            // Load dropdown resources
            var activeParties = await _dbContext.Parties.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();
            Parties.Clear();
            foreach (var p in activeParties) Parties.Add(p);

            var activeProducts = await _dbContext.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();
            Products.Clear();
            foreach (var prod in activeProducts) Products.Add(prod);

            if (invoiceId.HasValue)
            {
                IsEditMode = true;
                Title = "Edit Sales Invoice";
                
                var loaded = await _invoiceService.GetByIdAsync(invoiceId.Value);
                if (loaded != null)
                {
                    Invoice = loaded;
                    SelectedParty = Parties.FirstOrDefault(p => p.Id == loaded.PartyId);

                    InvoiceItems.Clear();
                    foreach (var item in loaded.InvoiceItems)
                    {
                        InvoiceItems.Add(item);
                    }
                }
                else
                {
                    ErrorMessage = "Failed to load invoice details.";
                }
            }
            else
            {
                IsEditMode = false;
                Title = "Create New Invoice";
                Invoice = new Invoice
                {
                    InvoiceDate = DateTime.Today,
                    InvoiceNumber = await _invoiceService.GenerateNextInvoiceNumberAsync(),
                    PaymentStatus = "Unpaid",
                    PaymentMethod = "Cash",
                    IsDraft = false
                };
                SelectedParty = null;
                InvoiceItems.Clear();
            }
        }

        partial void OnSelectedPartyChanged(Party? value)
        {
            if (value != null)
            {
                Invoice.PartyId = value.Id;
            }
        }

        [RelayCommand]
        private void AddProductItem()
        {
            if (SelectedProductToAdd == null) return;

            var existing = InvoiceItems.FirstOrDefault(ii => ii.ProductId == SelectedProductToAdd.Id);
            if (existing != null)
            {
                existing.Quantity += 1;
                RecalculateItemTotals(existing);
                // Trigger refresh on grid
                var index = InvoiceItems.IndexOf(existing);
                InvoiceItems[index] = existing;
            }
            else
            {
                var newItem = new InvoiceItem
                {
                    ProductId = SelectedProductToAdd.Id,
                    Product = SelectedProductToAdd,
                    Quantity = 1,
                    UnitPrice = SelectedProductToAdd.SellingPrice,
                    DiscountPercent = 0,
                    DiscountAmount = 0,
                    TaxPercent = SelectedProductToAdd.TaxRate, // Default Product GST Rate
                    TaxAmount = 0,
                    SubTotal = 0,
                    TotalAmount = 0
                };
                RecalculateItemTotals(newItem);
                InvoiceItems.Add(newItem);
            }

            SelectedProductToAdd = null;
            RecalculateInvoiceTotals();
        }

        [RelayCommand]
        private void RemoveItem(InvoiceItem item)
        {
            if (item != null)
            {
                InvoiceItems.Remove(item);
                RecalculateInvoiceTotals();
            }
        }

        public void RecalculateItemTotals(InvoiceItem item)
        {
            decimal grossAmount = item.Quantity * item.UnitPrice;
            item.DiscountAmount = grossAmount * (item.DiscountPercent / 100);
            item.SubTotal = grossAmount - item.DiscountAmount;
            item.TaxAmount = item.SubTotal * (item.TaxPercent / 100);
            item.TotalAmount = item.SubTotal + item.TaxAmount;
        }

        public void RecalculateInvoiceTotals()
        {
            decimal subTotal = 0;
            decimal discount = 0;
            decimal tax = 0;

            foreach (var item in InvoiceItems)
            {
                subTotal += item.SubTotal;
                discount += item.DiscountAmount;
                tax += item.TaxAmount;
            }

            Invoice.SubTotal = subTotal;
            Invoice.DiscountAmount = discount;
            Invoice.TaxAmount = tax;
            
            decimal grossTotal = subTotal + tax + Invoice.ShippingCharges;
            decimal roundedTotal = Math.Round(grossTotal, 0);
            Invoice.RoundOff = roundedTotal - grossTotal;
            Invoice.TotalAmount = roundedTotal;

            // Trigger UI property changes
            OnPropertyChanged(nameof(Invoice));
        }

        [RelayCommand]
        private async Task SaveInvoiceAsync()
        {
            if (IsSaving) return;

            ErrorMessage = string.Empty;

            // Validation
            if (SelectedParty == null)
            {
                ErrorMessage = "Customer is required.";
                return;
            }
            if (InvoiceItems.Count == 0)
            {
                ErrorMessage = "At least one product item must be added.";
                return;
            }

            foreach (var item in InvoiceItems)
            {
                if (item.Quantity <= 0)
                {
                    ErrorMessage = $"Quantity for product {item.Product?.Name} must be greater than zero.";
                    return;
                }
                if (item.UnitPrice <= 0)
                {
                    ErrorMessage = $"Unit Price for product {item.Product?.Name} must be greater than zero.";
                    return;
                }
            }

            IsSaving = true;

            try
            {
                Invoice.InvoiceItems = InvoiceItems.ToList();
                string username = _authService.CurrentUser?.Username ?? "admin";

                if (IsEditMode)
                {
                    await _invoiceService.UpdateAsync(Invoice, username);
                }
                else
                {
                    await _invoiceService.CreateAsync(Invoice, username);
                }

                _navigationService.Navigate(typeof(InvoicesPage));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to save invoice: {ex.Message}";
                Log.Error(ex, "Error occurred while saving invoice.");
            }
            finally
            {
                IsSaving = false;
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            _navigationService.Navigate(typeof(InvoicesPage));
        }
    }
}
