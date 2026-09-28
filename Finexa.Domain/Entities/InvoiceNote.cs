using System;

namespace Finexa.Domain.Entities
{
    public class InvoiceNote
    {
        public int Id { get; set; }
        
        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
        public bool IsInternal { get; set; } = true;
    }
}
