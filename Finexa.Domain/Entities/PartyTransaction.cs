using System;

namespace Finexa.Domain.Entities
{
    public class PartyTransaction
    {
        public int Id { get; set; }
        
        public int PartyId { get; set; }
        public Party? Party { get; set; }
        
        public DateTime Date { get; set; } = DateTime.Today;
        public string ReferenceType { get; set; } = string.Empty; // Invoice, Purchase, Payment, Return, Adjustment
        public string ReferenceNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } = "Debit"; // Debit, Credit
        
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
