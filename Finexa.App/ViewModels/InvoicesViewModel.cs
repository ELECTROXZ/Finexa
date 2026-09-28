using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
using Windows.Storage.Pickers;
using WinRT.Interop;
using Serilog;

namespace Finexa_App.ViewModels
{
    public partial class InvoicesViewModel : ObservableObject
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IInvoiceExportService _exportService;
        private readonly INavigationService _navigationService;
        private readonly IAuthService _authService;
        private readonly FinexaDbContext _dbContext;

        [ObservableProperty]
        private ObservableCollection<Invoice> _invoices = new();

        [ObservableProperty]
        private ObservableCollection<Party> _parties = new();

        [ObservableProperty]
        private Invoice? _selectedInvoice;

        // Filtering Properties
        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _selectedPaymentStatus = "All";

        [ObservableProperty]
        private string _selectedDateFilter = "All";

        [ObservableProperty]
        private DateTimeOffset? _startDate;

        [ObservableProperty]
        private DateTimeOffset? _endDate;

        [ObservableProperty]
        private Party? _selectedParty;

        [ObservableProperty]
        private bool _showTrash;

        // Sorting Properties
        [ObservableProperty]
        private string _sortBy = "InvoiceDate";

        [ObservableProperty]
        private bool _sortDescending = true;

        // Pagination Properties
        [ObservableProperty]
        private int _pageIndex = 0;

        [ObservableProperty]
        private int _pageSize = 25;

        [ObservableProperty]
        private int _totalCount = 0;

        [ObservableProperty]
        private int _totalPages = 0;

        [ObservableProperty]
        private string _pageDisplay = "Page 1 of 1";

        [ObservableProperty]
        private bool _hasPreviousPage;

        [ObservableProperty]
        private bool _hasNextPage;

        [ObservableProperty]
        private bool _isLoading;

        // Undo Delete state
        private Invoice? _lastDeletedInvoice;

        public InvoicesViewModel(
            IInvoiceService invoiceService,
            IInvoiceExportService exportService,
            INavigationService navigationService,
            IAuthService authService,
            FinexaDbContext dbContext)
        {
            _invoiceService = invoiceService;
            _exportService = exportService;
            _navigationService = navigationService;
            _authService = authService;
            _dbContext = dbContext;
        }

        public async Task InitializeAsync()
        {
            // Load Parties for filtering list
            var partyList = await _dbContext.Parties
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();

            Parties.Clear();
            foreach (var p in partyList)
            {
                Parties.Add(p);
            }

            await LoadInvoicesAsync();
        }

        [RelayCommand]
        public async Task LoadInvoicesAsync()
        {
            if (IsLoading) return;
            IsLoading = true;

            try
            {
                DateTime? start = StartDate?.Date;
                DateTime? end = EndDate?.Date;

                var (pagedInvoices, total) = await _invoiceService.GetPagedAsync(
                    PageIndex,
                    PageSize,
                    SearchText,
                    SelectedPaymentStatus,
                    SelectedDateFilter,
                    start,
                    end,
                    SelectedParty?.Id,
                    SortBy,
                    SortDescending,
                    ShowTrash
                );

                Invoices.Clear();
                foreach (var inv in pagedInvoices)
                {
                    Invoices.Add(inv);
                }

                TotalCount = total;
                TotalPages = (int)Math.Ceiling((double)total / PageSize);
                if (TotalPages == 0) TotalPages = 1;

                PageDisplay = $"Page {PageIndex + 1} of {TotalPages} (Total: {TotalCount})";
                HasPreviousPage = PageIndex > 0;
                HasNextPage = PageIndex < TotalPages - 1;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load invoices in viewmodel.");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void NewInvoice()
        {
            _navigationService.Navigate(typeof(InvoiceEditorPage));
        }

        [RelayCommand]
        private void EditInvoice()
        {
            if (SelectedInvoice != null)
            {
                _navigationService.Navigate(typeof(InvoiceEditorPage), SelectedInvoice.Id);
            }
        }

        [RelayCommand]
        private async Task DeleteInvoiceAsync()
        {
            if (SelectedInvoice == null) return;

            string username = _authService.CurrentUser?.Username ?? "admin";
            int id = SelectedInvoice.Id;
            string num = SelectedInvoice.InvoiceNumber;

            _lastDeletedInvoice = SelectedInvoice;

            bool success = await _invoiceService.SoftDeleteAsync(id, username);
            if (success)
            {
                Log.Information("Soft-deleted invoice {Num}", num);
                await LoadInvoicesAsync();
            }
        }

        [RelayCommand]
        private async Task UndoDeleteAsync()
        {
            if (_lastDeletedInvoice == null) return;

            string username = _authService.CurrentUser?.Username ?? "admin";
            bool success = await _invoiceService.RestoreAsync(_lastDeletedInvoice.Id, username);
            if (success)
            {
                Log.Information("Restored soft-deleted invoice {Num}", _lastDeletedInvoice.InvoiceNumber);
                _lastDeletedInvoice = null;
                await LoadInvoicesAsync();
            }
        }

        [RelayCommand]
        private async Task RestoreInvoiceAsync()
        {
            if (SelectedInvoice == null) return;

            string username = _authService.CurrentUser?.Username ?? "admin";
            bool success = await _invoiceService.RestoreAsync(SelectedInvoice.Id, username);
            if (success)
            {
                await LoadInvoicesAsync();
            }
        }

        [RelayCommand]
        private async Task DuplicateInvoiceAsync()
        {
            if (SelectedInvoice == null) return;

            string username = _authService.CurrentUser?.Username ?? "admin";
            try
            {
                var duplicated = await _invoiceService.DuplicateAsync(SelectedInvoice.Id, username);
                if (duplicated != null)
                {
                    await LoadInvoicesAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to duplicate invoice.");
            }
        }

        [RelayCommand]
        private async Task PrintInvoiceAsync()
        {
            if (SelectedInvoice == null) return;

            try
            {
                // Fetch full invoice details
                var fullInvoice = await _invoiceService.GetByIdAsync(SelectedInvoice.Id);
                if (fullInvoice == null) return;

                var pdfBytes = await _exportService.GenerateInvoicePdfAsync(fullInvoice, new Setting());
                string tempPath = Path.Combine(Path.GetTempPath(), $"Invoice_{fullInvoice.InvoiceNumber}.pdf");
                await File.WriteAllBytesAsync(tempPath, pdfBytes);

                // Open the PDF for previewing/printing
                Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
                await _invoiceService.LogHistoryAsync(fullInvoice.Id, "Printed", _authService.CurrentUser?.Username ?? "admin", $"Invoice printed/previewed. PDF saved at: {tempPath}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to print/preview invoice.");
            }
        }

        [RelayCommand]
        private async Task ExportPdfAsync()
        {
            if (SelectedInvoice == null) return;

            try
            {
                var fullInvoice = await _invoiceService.GetByIdAsync(SelectedInvoice.Id);
                if (fullInvoice == null) return;

                var picker = new FileSavePicker();
                InitializeFilePicker(picker);
                picker.FileTypeChoices.Add("PDF Document", new List<string> { ".pdf" });
                picker.SuggestedFileName = $"Invoice_{fullInvoice.InvoiceNumber}";

                var file = await picker.PickSaveFileAsync();
                if (file != null)
                {
                    var pdfBytes = await _exportService.GenerateInvoicePdfAsync(fullInvoice, new Setting());
                    await File.WriteAllBytesAsync(file.Path, pdfBytes);
                    await _invoiceService.LogHistoryAsync(fullInvoice.Id, "Exported", _authService.CurrentUser?.Username ?? "admin", $"Invoice exported to PDF: {file.Path}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to export PDF.");
            }
        }

        [RelayCommand]
        private async Task ExportExcelAsync()
        {
            try
            {
                var picker = new FileSavePicker();
                InitializeFilePicker(picker);
                picker.FileTypeChoices.Add("Excel Workbook", new List<string> { ".xlsx" });
                picker.SuggestedFileName = $"Invoices_Export_{DateTime.Today:yyyyMMdd}";

                var file = await picker.PickSaveFileAsync();
                if (file != null)
                {
                    // Get all currently filtered list of invoices (without pagination limits)
                    DateTime? start = StartDate?.Date;
                    DateTime? end = EndDate?.Date;

                    var (allFiltered, _) = await _invoiceService.GetPagedAsync(
                        0,
                        100000, // Large number to fetch all
                        SearchText,
                        SelectedPaymentStatus,
                        SelectedDateFilter,
                        start,
                        end,
                        SelectedParty?.Id,
                        SortBy,
                        SortDescending,
                        ShowTrash
                    );

                    // Fetch detail with parties
                    var excelBytes = await _exportService.ExportInvoicesToExcelAsync(allFiltered);
                    await File.WriteAllBytesAsync(file.Path, excelBytes);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to export Excel.");
            }
        }

        [RelayCommand]
        private async Task ExportCsvAsync()
        {
            try
            {
                var picker = new FileSavePicker();
                InitializeFilePicker(picker);
                picker.FileTypeChoices.Add("CSV Comma Separated Values", new List<string> { ".csv" });
                picker.SuggestedFileName = $"Invoices_Export_{DateTime.Today:yyyyMMdd}";

                var file = await picker.PickSaveFileAsync();
                if (file != null)
                {
                    DateTime? start = StartDate?.Date;
                    DateTime? end = EndDate?.Date;

                    var (allFiltered, _) = await _invoiceService.GetPagedAsync(
                        0,
                        100000,
                        SearchText,
                        SelectedPaymentStatus,
                        SelectedDateFilter,
                        start,
                        end,
                        SelectedParty?.Id,
                        SortBy,
                        SortDescending,
                        ShowTrash
                    );

                    var csvBytes = await _exportService.ExportInvoicesToCsvAsync(allFiltered);
                    await File.WriteAllBytesAsync(file.Path, csvBytes);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to export CSV.");
            }
        }

        [RelayCommand]
        private async Task AddPaymentAsync(object? param)
        {
            if (SelectedInvoice == null) return;
            if (param is not ValueTuple<decimal, string, string, string> data) return;

            var (amount, method, reference, notes) = data;
            string username = _authService.CurrentUser?.Username ?? "admin";
            
            try
            {
                await _invoiceService.AddPaymentAsync(SelectedInvoice.Id, amount, method, reference, notes, username);
                await LoadInvoicesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to record payment in Viewmodel.");
            }
        }

        [RelayCommand]
        private void NextPage()
        {
            if (PageIndex < TotalPages - 1)
            {
                PageIndex++;
                _ = LoadInvoicesAsync();
            }
        }

        [RelayCommand]
        private void PreviousPage()
        {
            if (PageIndex > 0)
            {
                PageIndex--;
                _ = LoadInvoicesAsync();
            }
        }

        [RelayCommand]
        private void ResetFilters()
        {
            SearchText = string.Empty;
            SelectedPaymentStatus = "All";
            SelectedDateFilter = "All";
            StartDate = null;
            EndDate = null;
            SelectedParty = null;
            ShowTrash = false;
            PageIndex = 0;
            _ = LoadInvoicesAsync();
        }

        private void InitializeFilePicker(object picker)
        {
            // WinUI 3 Desktop apps must initialize the picker with window handle
            var activeWindow = App.Current.Resources["ActiveWindow"] as Microsoft.UI.Xaml.Window;
            if (activeWindow != null)
            {
                var hwnd = WindowNative.GetWindowHandle(activeWindow);
                InitializeWithWindow.Initialize(picker, hwnd);
            }
        }
    }
}
