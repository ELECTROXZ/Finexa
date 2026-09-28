using System;

namespace Finexa.Domain.Entities
{
    public class PartyDocument
    {
        public int Id { get; set; }
        
        public int PartyId { get; set; }
        public Party? Party { get; set; }
        
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
