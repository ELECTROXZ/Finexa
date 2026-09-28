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
    public partial class PartyDetailsViewModel : ObservableObject
    {
        private readonly IPartyService _partyService;
        private readonly FinexaDbContext _dbContext;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PartyInitials))]
        [NotifyPropertyChangedFor(nameof(IsNotesEmpty))]
        [NotifyPropertyChangedFor(nameof(StatusColorBrush))]
        [NotifyPropertyChangedFor(nameof(StatusBgBrush))]
        [NotifyPropertyChangedFor(nameof(PartyCreditLimitDisplay))]
        [NotifyPropertyChangedFor(nameof(OutstandingBalanceColorBrush))]
        private Party? _party;

        [ObservableProperty] private bool _isLoading = false;
        [ObservableProperty] private string _newNoteText = string.Empty;

        public string PartyInitials => string.IsNullOrWhiteSpace(Party?.Name) ? "P" : Party.Name.Substring(0, 1).ToUpper();

        public bool IsNotesEmpty => string.IsNullOrEmpty(Party?.Notes);

        public string PartyNotes => Party?.Notes ?? string.Empty;

        public Microsoft.UI.Xaml.Media.Brush StatusColorBrush => new Microsoft.UI.Xaml.Media.SolidColorBrush(
            Party?.Status == "Active" ? Windows.UI.Color.FromArgb(255, 76, 175, 80) : 
            Party?.Status == "Blocked" ? Windows.UI.Color.FromArgb(255, 244, 67, 54) : 
            Windows.UI.Color.FromArgb(255, 255, 152, 0)
        );

        public Microsoft.UI.Xaml.Media.Brush StatusBgBrush => new Microsoft.UI.Xaml.Media.SolidColorBrush(
            Party?.Status == "Active" ? Windows.UI.Color.FromArgb(40, 76, 175, 80) : 
            Party?.Status == "Blocked" ? Windows.UI.Color.FromArgb(40, 244, 67, 54) : 
            Windows.UI.Color.FromArgb(40, 255, 152, 0)
        );

        public string PartyCreditLimitDisplay => Party == null ? string.Empty : Party.CreditLimit <= 0 ? "Unlimited" : $"₹{Party.CreditLimit:N2}";

        public Microsoft.UI.Xaml.Media.Brush OutstandingBalanceColorBrush => new Microsoft.UI.Xaml.Media.SolidColorBrush(
            Party == null ? Windows.UI.Color.FromArgb(255, 158, 158, 158) :
            Party.OutstandingAmount > 0 ? Windows.UI.Color.FromArgb(255, 244, 67, 54) :
            Party.OutstandingAmount < 0 ? Windows.UI.Color.FromArgb(255, 76, 175, 80) :
            Windows.UI.Color.FromArgb(255, 158, 158, 158)
        );
        
        // Add dialog properties
        [ObservableProperty] private string _errorMessages = string.Empty;

        public ObservableCollection<PartyAddress> Addresses { get; } = new();
        public ObservableCollection<PartyContact> Contacts { get; } = new();
        public ObservableCollection<PartyTransaction> Transactions { get; } = new();
        public ObservableCollection<PartyNote> Notes { get; } = new();
        public ObservableCollection<PartyDocument> Documents { get; } = new();
        public ObservableCollection<PartyActivityLog> ActivityLogs { get; } = new();

        public PartyDetailsViewModel(IPartyService partyService, FinexaDbContext dbContext)
        {
            _partyService = partyService;
            _dbContext = dbContext;
        }

        public async Task LoadPartyDetailsAsync(int partyId)
        {
            IsLoading = true;
            try
            {
                Party = await _partyService.GetByIdAsync(partyId);
                if (Party != null)
                {
                    await RefreshCollectionsAsync();
                    await _partyService.LogActivityAsync(partyId, "Viewed", "admin", "Party profile page viewed.");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, $"Failed to load party details for ID: {partyId}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task RefreshCollectionsAsync()
        {
            if (Party == null) return;

            Addresses.Clear();
            var addrList = await _partyService.GetAddressesAsync(Party.Id);
            foreach (var a in addrList) Addresses.Add(a);

            Contacts.Clear();
            var contactList = await _partyService.GetContactsAsync(Party.Id);
            foreach (var c in contactList) Contacts.Add(c);

            Transactions.Clear();
            var transList = await _partyService.GetTransactionsAsync(Party.Id);
            foreach (var t in transList) Transactions.Add(t);

            Notes.Clear();
            var noteList = await _partyService.GetNotesAsync(Party.Id);
            foreach (var n in noteList) Notes.Add(n);

            Documents.Clear();
            var docList = await _partyService.GetDocumentsAsync(Party.Id);
            foreach (var d in docList) Documents.Add(d);

            ActivityLogs.Clear();
            var logList = await _partyService.GetActivityLogsAsync(Party.Id);
            foreach (var l in logList) ActivityLogs.Add(l);
        }

        [RelayCommand]
        public async Task AddNoteAsync()
        {
            if (Party == null || string.IsNullOrWhiteSpace(NewNoteText)) return;
            try
            {
                await _partyService.AddNoteAsync(Party.Id, NewNoteText, "admin");
                NewNoteText = string.Empty;
                await RefreshCollectionsAsync();
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to add note.");
            }
        }

        [RelayCommand]
        public async Task RemoveNoteAsync(PartyNote note)
        {
            if (note == null) return;
            try
            {
                await _partyService.RemoveNoteAsync(note.Id);
                await RefreshCollectionsAsync();
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to remove note.");
            }
        }

        [RelayCommand]
        public async Task AddAddressAsync(PartyAddress address)
        {
            if (Party == null || address == null) return;
            try
            {
                await _partyService.AddAddressAsync(Party.Id, address);
                await RefreshCollectionsAsync();
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to add address.");
            }
        }

        [RelayCommand]
        public async Task RemoveAddressAsync(PartyAddress address)
        {
            if (address == null) return;
            try
            {
                await _partyService.RemoveAddressAsync(address.Id);
                await RefreshCollectionsAsync();
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to remove address.");
            }
        }

        [RelayCommand]
        public async Task AddContactAsync(PartyContact contact)
        {
            if (Party == null || contact == null) return;
            try
            {
                await _partyService.AddContactAsync(Party.Id, contact);
                await RefreshCollectionsAsync();
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to add contact.");
            }
        }

        [RelayCommand]
        public async Task RemoveContactAsync(PartyContact contact)
        {
            if (contact == null) return;
            try
            {
                await _partyService.RemoveContactAsync(contact.Id);
                await RefreshCollectionsAsync();
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to remove contact.");
            }
        }

        [RelayCommand]
        public async Task AddTransactionAsync(PartyTransaction transaction)
        {
            if (Party == null || transaction == null) return;
            try
            {
                await _partyService.AddTransactionAsync(Party.Id, transaction, "admin");
                await RefreshCollectionsAsync();
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to add transaction.");
            }
        }

        public async Task AddDocumentAsync(string fileName, string filePath)
        {
            if (Party == null) return;
            try
            {
                await _partyService.AddDocumentAsync(Party.Id, fileName, filePath);
                await RefreshCollectionsAsync();
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to add document attachment.");
            }
        }

        [RelayCommand]
        public async Task RemoveDocumentAsync(PartyDocument doc)
        {
            if (doc == null) return;
            try
            {
                await _partyService.RemoveDocumentAsync(doc.Id);
                await RefreshCollectionsAsync();
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to remove document attachment.");
            }
        }

        public async Task<List<string>> ValidatePartyAsync(Party party)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(party.Name))
                errors.Add("Business Name is required.");

            if (string.IsNullOrWhiteSpace(party.Type))
                errors.Add("Party Type is required.");

            // Email validation
            if (!string.IsNullOrWhiteSpace(party.Email) && !party.Email.Contains("@"))
                errors.Add("Email format is invalid.");

            // GSTIN validation (15 characters alphanumeric)
            if (!string.IsNullOrWhiteSpace(party.GSTIN))
            {
                if (party.GSTIN.Length != 15)
                    errors.Add("GST Number must be exactly 15 characters.");
                
                bool dupGst = await _dbContext.Parties
                    .AnyAsync(p => p.Id != party.Id && p.GSTIN.ToLower() == party.GSTIN.ToLower() && !p.IsDeleted);
                if (dupGst)
                    errors.Add($"GST Number '{party.GSTIN}' is already registered to another party.");
            }

            // PAN validation (10 characters alphanumeric)
            if (!string.IsNullOrWhiteSpace(party.PAN))
            {
                if (party.PAN.Length != 10)
                    errors.Add("PAN Number must be exactly 10 characters.");
                
                bool dupPan = await _dbContext.Parties
                    .AnyAsync(p => p.Id != party.Id && p.PAN.ToLower() == party.PAN.ToLower() && !p.IsDeleted);
                if (dupPan)
                    errors.Add($"PAN Number '{party.PAN}' is already registered to another party.");
            }

            // Phone duplicate check
            if (!string.IsNullOrWhiteSpace(party.Phone))
            {
                bool dupPhone = await _dbContext.Parties
                    .AnyAsync(p => p.Id != party.Id && p.Phone.ToLower() == party.Phone.ToLower() && !p.IsDeleted);
                if (dupPhone)
                    errors.Add($"Phone number '{party.Phone}' is already registered to another party.");
            }

            return errors;
        }

        [RelayCommand]
        public async Task SavePartyDetailsAsync()
        {
            if (Party == null) return;
            try
            {
                var errors = await ValidatePartyAsync(Party);
                if (errors.Any())
                {
                    ErrorMessages = string.Join("\n", errors);
                    return;
                }

                ErrorMessages = string.Empty;
                await _partyService.UpdateAsync(Party, "admin");
                await RefreshCollectionsAsync();
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to update party details.");
                ErrorMessages = $"Failed to save details: {ex.Message}";
            }
        }
    }
}
