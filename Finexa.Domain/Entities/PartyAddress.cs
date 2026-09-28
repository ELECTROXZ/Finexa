using System;

namespace Finexa.Domain.Entities
{
    public class PartyAddress
    {
        public int Id { get; set; }
        
        public int PartyId { get; set; }
        public Party? Party { get; set; }
        
        public string AddressLine1 { get; set; } = string.Empty;
        public string AddressLine2 { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        
        public bool IsPrimary { get; set; }
    }
}
