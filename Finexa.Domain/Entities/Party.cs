using System;
using System.Collections.Generic;

namespace Finexa.Domain.Entities
{
    public class Party
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty; // Unique Code (e.g. CUS-000001, SUP-000001)
        public string Name { get; set; } = string.Empty; // Business Name
        public string DisplayName { get; set; } = string.Empty; // Display Name / Trading Name
        public string Type { get; set; } = "Customer"; // Customer, Supplier, Both
        
        public string ContactPerson { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AlternativePhone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        
        public string GSTIN { get; set; } = string.Empty; // GST Number
        public string PAN { get; set; } = string.Empty; // PAN Number
        public string Aadhaar { get; set; } = string.Empty; // Aadhaar (optional)
        public string BusinessRegistrationNumber { get; set; } = string.Empty; // BRN
        
        // Primary Address fields stored directly for compatibility/quick access
        public string Address { get; set; } = string.Empty; // Standard address string
        public string AddressLine1 { get; set; } = string.Empty;
        public string AddressLine2 { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        
        public decimal OpeningBalance { get; set; }
        public decimal OutstandingAmount { get; set; } // Current ledger balance
        public decimal CreditLimit { get; set; }
        public string PaymentTerms { get; set; } = "COD"; // COD, Net 15, Net 30, Net 60, etc.
        public string PreferredPaymentMethod { get; set; } = "Cash"; // Cash, Bank, Cheque, UPI, etc.
        
        public string Notes { get; set; } = string.Empty;
        public string Status { get; set; } = "Active"; // Active, Inactive, Blocked
        public bool IsActive { get; set; } = true; // Backward compatibility
        
        public byte[]? ProfileImage { get; set; } // Optional Profile Image / Company Logo
        
        // Soft delete and Auditing
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation properties
        public ICollection<PartyAddress> Addresses { get; set; } = new List<PartyAddress>();
        public ICollection<PartyContact> Contacts { get; set; } = new List<PartyContact>();
        public ICollection<PartyTransaction> Transactions { get; set; } = new List<PartyTransaction>();
        public ICollection<PartyNote> NotesList { get; set; } = new List<PartyNote>();
        public ICollection<PartyDocument> Documents { get; set; } = new List<PartyDocument>();
        public ICollection<PartyActivityLog> ActivityLogs { get; set; } = new List<PartyActivityLog>();
    }
}
