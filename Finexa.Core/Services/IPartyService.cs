using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Finexa.Domain.Entities;

namespace Finexa.Core.Services
{
    public interface IPartyService
    {
        Task<Party?> GetByIdAsync(int id);
        Task<Party?> GetByCodeAsync(string code);
        Task<List<Party>> GetAllAsync(bool includeDeleted = false);
        
        Task<(List<Party> Parties, int TotalCount)> GetPagedAsync(
            int pageIndex, 
            int pageSize, 
            string searchText, 
            string partyType, // "Customer", "Supplier", "Both", "All"
            string statusFilter, // "Active", "Inactive", "Blocked", "All"
            string outstandingFilter, // "Outstanding", "NoOutstanding", "CreditLimitExceeded", "All"
            string gstFilter, // "Registered", "NonRegistered", "All"
            string sortBy, 
            bool sortDescending,
            bool showTrash = false);

        Task<Party> CreateAsync(Party party, string username);
        Task<Party> UpdateAsync(Party party, string username);
        
        Task<bool> SoftDeleteAsync(int id, string username);
        Task<bool> RestoreAsync(int id, string username);
        
        Task<Party> DuplicateAsync(int id, string username);
        Task<string> GenerateNextPartyCodeAsync(string partyType);

        // Addresses
        Task<PartyAddress> AddAddressAsync(int partyId, PartyAddress address);
        Task<bool> RemoveAddressAsync(int addressId);
        Task<List<PartyAddress>> GetAddressesAsync(int partyId);

        // Contacts
        Task<PartyContact> AddContactAsync(int partyId, PartyContact contact);
        Task<bool> RemoveContactAsync(int contactId);
        Task<List<PartyContact>> GetContactsAsync(int partyId);

        // Notes
        Task<PartyNote> AddNoteAsync(int partyId, string text, string username);
        Task<bool> RemoveNoteAsync(int noteId);
        Task<List<PartyNote>> GetNotesAsync(int partyId);

        // Transactions & Ledger
        Task<PartyTransaction> AddTransactionAsync(int partyId, PartyTransaction transaction, string username);
        Task<List<PartyTransaction>> GetTransactionsAsync(int partyId);
        Task RecalculateOutstandingAmountAsync(int partyId);

        // Documents
        Task<PartyDocument> AddDocumentAsync(int partyId, string fileName, string filePath);
        Task<bool> RemoveDocumentAsync(int documentId);
        Task<List<PartyDocument>> GetDocumentsAsync(int partyId);

        // Audit/Activity Log
        Task<List<PartyActivityLog>> GetActivityLogsAsync(int partyId);
        Task LogActivityAsync(int partyId, string action, string username, string details);
    }
}
