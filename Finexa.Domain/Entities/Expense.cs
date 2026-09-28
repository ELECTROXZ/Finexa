using System;

namespace Finexa.Domain.Entities
{
    public class Expense
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty; // Expense entry voucher code
        public DateTime Date { get; set; } = DateTime.Today;
        
        public int CategoryId { get; set; }
        public ExpenseCategory? Category { get; set; }
        
        public decimal Amount { get; set; }
        public string PaymentMode { get; set; } = "Cash"; // Cash, Bank, UPI, Card, Cheque
        public string Description { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty; // Transaction ID
        public string AttachmentPath { get; set; } = string.Empty; // Path to receipt image/PDF
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
