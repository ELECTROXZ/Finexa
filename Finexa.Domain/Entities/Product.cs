using System;

namespace Finexa.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty; // SKU / Product Code (Unique)
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal Stock { get; set; }
        public decimal MinStock { get; set; } // Low Stock Alert threshold
        public decimal MaxStock { get; set; }
        
        public DateTime? ExpiryDate { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string QRCode { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
        
        public int? BrandId { get; set; }
        public Brand? Brand { get; set; }
        
        public int? UnitId { get; set; }
        public Unit? Unit { get; set; }

        public string HSNCode { get; set; } = string.Empty;
        public decimal TaxRate { get; set; } // e.g. 18.00 representing 18% GST
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
