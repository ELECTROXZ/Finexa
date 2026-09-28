using System;

namespace Finexa.Domain.Entities
{
    public class LedgerTransaction
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;
        
        public string TransactionType { get; set; } = "Debit"; // Debit (Dr), Credit (Cr)
        public decimal Amount { get; set; }
        public decimal RunningBalance { get; set; } // Running balance calculated chronologically
        public string Description { get; set; } = string.Empty;
        
        // Ledger Account Type
        public string AccountType { get; set; } = "Party"; // Party, Cash, Bank
        
        // Optional links to source transactions
        public int? PartyId { get; set; }
        public Party? Party { get; set; }
        
        public int? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        
        public int? PaymentId { get; set; }
        public Payment? Payment { get; set; }
        
        public int? ExpenseId { get; set; }
        public Expense? Expense { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
