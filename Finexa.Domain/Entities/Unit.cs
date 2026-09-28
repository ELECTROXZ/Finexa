namespace Finexa.Domain.Entities
{
    public class Unit
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // e.g. Pieces, Kilograms
        public string Abbreviation { get; set; } = string.Empty; // e.g. Pcs, Kg
        public bool IsActive { get; set; } = true;
    }
}
