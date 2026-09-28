using System;

namespace Finexa.Domain.Entities
{
    public class PartyActivityLog
    {
        public int Id { get; set; }
        
        public int PartyId { get; set; }
        public Party? Party { get; set; }
        
        public string Action { get; set; } = string.Empty; // Created, Updated, Deleted, Restored, Viewed, Imported, Exported
        public string ChangedBy { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
