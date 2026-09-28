using System;
using System.Collections.Generic;

namespace Finexa.Domain.Entities
{
    public class Invoice
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty; // Unique Invoice Number
        public DateTime InvoiceDate { get; set; } = DateTime.Today;
        
        public int PartyId { get; set; }
        public Party? Party { get; set; }
        
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; } // Aggregate GST
        public decimal ShippingCharges { get; set; }
        public decimal RoundOff { get; set; }
        public decimal TotalAmount { get; set; }
        
        public string Notes { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = "Unpaid"; // Paid, Partially Paid, Unpaid, Void
        public string PaymentMethod { get; set; } = "Cash"; // Cash, Bank, UPI, Card, Cheque
        public bool IsDraft { get; set; } = false;
        
        // Soft Delete Flags
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        
        // Audit Fields
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        public List<InvoiceItem> InvoiceItems { get; set; } = new();
        public List<InvoicePayment> InvoicePayments { get; set; } = new();
        public List<InvoiceHistory> InvoiceHistory { get; set; } = new();
        public List<InvoiceNote> InvoiceNotes { get; set; } = new();
    }
}
