using System;

namespace Finexa.Domain.Entities
{
    public class PartyContact
    {
        public int Id { get; set; }
        
        public int PartyId { get; set; }
        public Party? Party { get; set; }
        
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
    }
}
