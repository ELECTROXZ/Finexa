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
using Microsoft.EntityFrameworkCore;

namespace Finexa_App.ViewModels
{
    public partial class PartiesViewModel : ObservableObject
    {
        private readonly IPartyService _partyService;
        private readonly IPartyImportExportService _importExportService;
        private readonly FinexaDbContext _dbContext;

        [ObservableProperty] private string _searchText = string.Empty;
        [ObservableProperty] private string _selectedPartyType = "All";
        [ObservableProperty] private string _selectedStatusFilter = "All";
        [ObservableProperty] private string _selectedOutstandingFilter = "All";
        [ObservableProperty] private string _selectedGstFilter = "All";
        
        [ObservableProperty] private Party? _selectedParty;
        
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PageDisplay))]
        [NotifyPropertyChangedFor(nameof(HasPreviousPage))]
        [NotifyPropertyChangedFor(nameof(HasNextPage))]
        private int _pageIndex = 0;

        [ObservableProperty] private int _pageSize = 10;
        [ObservableProperty] private int _totalCount = 0;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PageDisplay))]
        [NotifyPropertyChangedFor(nameof(HasNextPage))]
        private int _totalPages = 1;

        [ObservableProperty] private bool _showTrash = false;
        
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsListEmpty))]
        private bool _isLoading = false;

        [ObservableProperty] private string _sortBy = "Name";
        [ObservableProperty] private bool _sortDescending = false;

        public string PageDisplay => $"Page {PageIndex + 1} of {TotalPages}";
        public bool HasPreviousPage => PageIndex > 0;
        public bool HasNextPage => PageIndex < TotalPages - 1;
        public bool IsListEmpty => Parties.Count == 0 && !IsLoading;

        [ObservableProperty] private string _errorMessage = string.Empty;
        [ObservableProperty] private string _undoMessage = string.Empty;
        [ObservableProperty] private bool _isUndoVisible = false;
        private int? _lastDeletedPartyId;

        // Dashboard Summary Metrics
        [ObservableProperty] private int _totalCustomers;
        [ObservableProperty] private int _totalSuppliers;
        [ObservableProperty] private int _activeCustomersCount;
        [ObservableProperty] private decimal _outstandingReceivables;
        [ObservableProperty] private decimal _outstandingPayables;
        [ObservableProperty] private int _newCustomersThisMonth;

        public ObservableCollection<Party> Parties { get; } = new();
        public ObservableCollection<Party> TopCustomers { get; } = new();
        public ObservableCollection<Party> TopSuppliers { get; } = new();

        public List<string> PartyTypes { get; } = new() { "All", "Customer", "Supplier", "Both" };
        public List<string> StatusFilters { get; } = new() { "All", "Active", "Inactive", "Blocked" };
        public List<string> OutstandingFilters { get; } = new() { "All", "Outstanding", "NoOutstanding", "CreditLimitExceeded" };
        public List<string> GstFilters { get; } = new() { "All", "Registered", "NonRegistered" };

        public PartiesViewModel(
            IPartyService partyService, 
            IPartyImportExportService importExportService,
            FinexaDbContext dbContext)
        {
            _partyService = partyService;
            _importExportService = importExportService;
            _dbContext = dbContext;
        }

        [RelayCommand]
        public async Task LoadPartiesAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                var (pagedParties, total) = await _partyService.GetPagedAsync(
                    PageIndex, PageSize, SearchText, SelectedPartyType, SelectedStatusFilter,
                    SelectedOutstandingFilter, SelectedGstFilter, SortBy, SortDescending, ShowTrash);

                Parties.Clear();
                foreach (var party in pagedParties)
                {
                    Parties.Add(party);
                }

                TotalCount = total;
                TotalPages = (int)Math.Ceiling((double)TotalCount / PageSize);
                if (TotalPages == 0) TotalPages = 1;

                await LoadDashboardSummaryAsync();
                OnPropertyChanged(nameof(IsListEmpty));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load parties: {ex.Message}";
                Serilog.Log.Error(ex, ErrorMessage);
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(IsListEmpty));
            }
        }

        private async Task LoadDashboardSummaryAsync()
        {
            try
            {
                // Customer metrics
                TotalCustomers = await _dbContext.Parties.CountAsync(p => (p.Type == "Customer" || p.Type == "Both") && !p.IsDeleted);
                TotalSuppliers = await _dbContext.Parties.CountAsync(p => (p.Type == "Supplier" || p.Type == "Both") && !p.IsDeleted);
                ActiveCustomersCount = await _dbContext.Parties.CountAsync(p => (p.Type == "Customer" || p.Type == "Both") && p.Status == "Active" && !p.IsDeleted);
                
                OutstandingReceivables = (decimal)await _dbContext.Parties
                    .Where(p => (p.Type == "Customer" || p.Type == "Both") && p.OutstandingAmount > 0 && !p.IsDeleted)
                    .SumAsync(p => (double)p.OutstandingAmount);

                OutstandingPayables = (decimal)await _dbContext.Parties
                    .Where(p => (p.Type == "Supplier" || p.Type == "Both") && p.OutstandingAmount < 0 && !p.IsDeleted)
                    .SumAsync(p => (double)Math.Abs((double)p.OutstandingAmount));

                var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                NewCustomersThisMonth = await _dbContext.Parties
                    .CountAsync(p => (p.Type == "Customer" || p.Type == "Both") && p.CreatedAt >= startOfMonth && !p.IsDeleted);

                // Top Customers
                TopCustomers.Clear();
                var topCust = await _dbContext.Parties
                    .Where(p => (p.Type == "Customer" || p.Type == "Both") && p.OutstandingAmount > 0 && !p.IsDeleted)
                    .OrderByDescending(p => (double)p.OutstandingAmount)
                    .Take(5)
                    .ToListAsync();
                foreach (var c in topCust) TopCustomers.Add(c);

                // Top Suppliers
                TopSuppliers.Clear();
                var topSupp = await _dbContext.Parties
                    .Where(p => (p.Type == "Supplier" || p.Type == "Both") && p.OutstandingAmount < 0 && !p.IsDeleted)
                    .OrderBy(p => (double)p.OutstandingAmount) // most negative balance (meaning we owe them the most)
                    .Take(5)
                    .ToListAsync();
                foreach (var s in topSupp) TopSuppliers.Add(s);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to load dashboard metrics summary.");
            }
        }

        [RelayCommand]
        public async Task DeletePartyAsync(Party party)
        {
            if (party == null) return;
            try
            {
                bool result = await _partyService.SoftDeleteAsync(party.Id, "admin");
                if (result)
                {
                    _lastDeletedPartyId = party.Id;
                    UndoMessage = $"Party '{party.Name}' was moved to Trash.";
                    IsUndoVisible = true;
                    await LoadPartiesAsync();
                    
                    // Auto-hide undo banner after 8 seconds
                    _ = Task.Delay(8000).ContinueWith(_ => { IsUndoVisible = false; }, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to delete party: {ex.Message}";
                Serilog.Log.Error(ex, ErrorMessage);
            }
        }

        [RelayCommand]
        public async Task UndoDeleteAsync()
        {
            if (_lastDeletedPartyId == null) return;
            try
            {
                bool result = await _partyService.RestoreAsync(_lastDeletedPartyId.Value, "admin");
                if (result)
                {
                    IsUndoVisible = false;
                    _lastDeletedPartyId = null;
                    await LoadPartiesAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to restore party: {ex.Message}";
                Serilog.Log.Error(ex, ErrorMessage);
            }
        }

        [RelayCommand]
        public async Task RestorePartyAsync(Party party)
        {
            if (party == null) return;
            try
            {
                await _partyService.RestoreAsync(party.Id, "admin");
                await LoadPartiesAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to restore party: {ex.Message}";
                Serilog.Log.Error(ex, ErrorMessage);
            }
        }

        [RelayCommand]
        public async Task DuplicatePartyAsync(Party party)
        {
            if (party == null) return;
            try
            {
                await _partyService.DuplicateAsync(party.Id, "admin");
                await LoadPartiesAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to duplicate party: {ex.Message}";
                Serilog.Log.Error(ex, ErrorMessage);
            }
        }

        [RelayCommand]
        public void NextPage()
        {
            if (PageIndex < TotalPages - 1)
            {
                PageIndex++;
                _ = LoadPartiesAsync();
            }
        }

        [RelayCommand]
        public void PreviousPage()
        {
            if (PageIndex > 0)
            {
                PageIndex--;
                _ = LoadPartiesAsync();
            }
        }

        [RelayCommand]
        public async Task<byte[]> ExportToPdfAsync()
        {
            var allParties = await _partyService.GetAllAsync(ShowTrash);
            return await _importExportService.ExportPartiesToPdfAsync(allParties);
        }

        [RelayCommand]
        public async Task<byte[]> ExportToExcelAsync()
        {
            var allParties = await _partyService.GetAllAsync(ShowTrash);
            return await _importExportService.ExportPartiesToExcelAsync(allParties);
        }

        [RelayCommand]
        public async Task<byte[]> ExportToCsvAsync()
        {
            var allParties = await _partyService.GetAllAsync(ShowTrash);
            return await _importExportService.ExportPartiesToCsvAsync(allParties);
        }
    }
}
