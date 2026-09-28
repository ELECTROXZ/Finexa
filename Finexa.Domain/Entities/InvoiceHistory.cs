using System;

namespace Finexa.Domain.Entities
{
    public class InvoiceHistory
    {
        public int Id { get; set; }
        
        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        
        public string Action { get; set; } = string.Empty; // Created, Edited, Soft-Deleted, Restored, Duplicate, Printed, Exported
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string ChangedBy { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
}
