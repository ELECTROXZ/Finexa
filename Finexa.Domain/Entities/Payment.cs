using System;

namespace Finexa.Domain.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public string ReceiptNumber { get; set; } = string.Empty; // Unique Receipt/Voucher Code
        public DateTime PaymentDate { get; set; } = DateTime.Today;
        
        public int PartyId { get; set; }
        public Party? Party { get; set; }
        
        public int? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        
        public decimal Amount { get; set; }
        public string Type { get; set; } = "Receipt"; // Receipt (Inflow), Payment (Outflow)
        
        public string PaymentMode { get; set; } = "Cash"; // Cash, Bank, UPI, Card, Cheque
        public string ReferenceNumber { get; set; } = string.Empty; // Transaction ID, Cheque No, etc.
        public string Notes { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
