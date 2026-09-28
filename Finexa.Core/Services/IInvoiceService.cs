using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Finexa.Domain.Entities;

namespace Finexa.Core.Services
{
    public interface IInvoiceService
    {
        // CRUD Operations
        Task<Invoice?> GetByIdAsync(int id);
        Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber);
        Task<List<Invoice>> GetAllAsync(bool includeDeleted = false);
        Task<(List<Invoice> Invoices, int TotalCount)> GetPagedAsync(
            int pageIndex, 
            int pageSize, 
            string searchText, 
            string paymentStatus, 
            string dateFilter, 
            DateTime? startDate, 
            DateTime? endDate, 
            int? partyId, 
            string sortBy, 
            bool sortDescending,
            bool showTrash = false);

        Task<Invoice> CreateAsync(Invoice invoice, string username);
        Task<Invoice> UpdateAsync(Invoice invoice, string username);
        
        // Soft Delete and Restore
        Task<bool> SoftDeleteAsync(int id, string username);
        Task<bool> RestoreAsync(int id, string username);
        
        // Duplicate
        Task<Invoice> DuplicateAsync(int id, string username);
        
        // Auto-Numbering
        Task<string> GenerateNextInvoiceNumberAsync();
        
        // Payments
        Task<InvoicePayment> AddPaymentAsync(int invoiceId, decimal amount, string method, string reference, string notes, string username);
        Task<List<InvoicePayment>> GetPaymentsAsync(int invoiceId);
        
        // Notes
        Task<InvoiceNote> AddNoteAsync(int invoiceId, string text, bool isInternal, string username);
        Task<List<InvoiceNote>> GetNotesAsync(int invoiceId);
        
        // History/Audit
        Task<List<InvoiceHistory>> GetHistoryAsync(int invoiceId);
        Task LogHistoryAsync(int invoiceId, string action, string username, string details);
    }
}
