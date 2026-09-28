using System;

namespace Finexa.Domain.Entities
{
    public class BackupHistory
    {
        public int Id { get; set; }
        public DateTime BackupDate { get; set; } = DateTime.UtcNow;
        public string FilePath { get; set; } = string.Empty;
        public string Status { get; set; } = "Success"; // Success, Failed
        public string BackupType { get; set; } = "Manual"; // Manual, Auto
        public string Notes { get; set; } = string.Empty;
    }
}
