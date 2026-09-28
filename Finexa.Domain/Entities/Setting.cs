namespace Finexa.Domain.Entities
{
    public class Setting
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty; // Unique configuration key
        public string Value { get; set; } = string.Empty;
        public string Group { get; set; } = "General"; // General, Business, Invoice, Printer, Theme
    }
}
