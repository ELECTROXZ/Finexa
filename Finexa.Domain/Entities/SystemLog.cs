using System;

namespace Finexa.Domain.Entities
{
    public class SystemLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Exception { get; set; } = string.Empty;
        public string Properties { get; set; } = string.Empty;
    }
}
