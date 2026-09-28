using System;

namespace Finexa.Domain.Entities
{
    public class PartyNote
    {
        public int Id { get; set; }
        
        public int PartyId { get; set; }
        public Party? Party { get; set; }
        
        public string Text { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
