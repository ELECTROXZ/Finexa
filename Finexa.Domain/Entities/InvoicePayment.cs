using System;

namespace Finexa.Domain.Entities
{
    public class InvoicePayment
    {
        public int Id { get; set; }
        
        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        
        public DateTime PaymentDate { get; set; } = DateTime.Today;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "Cash"; // Cash, Bank, UPI, Card, Cheque
        public string TransactionReference { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        
        public string ReceivedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
