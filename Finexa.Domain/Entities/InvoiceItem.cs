namespace Finexa.Domain.Entities
{
    public class InvoiceItem
    {
        public int Id { get; set; }
        
        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        
        public decimal TaxPercent { get; set; } // GST Rate (e.g. 18.00)
        public decimal TaxAmount { get; set; } // Calculated GST Amount
        
        public decimal SubTotal { get; set; } // Amount after discount but before tax
        public decimal TotalAmount { get; set; } // Amount after discount and after tax
    }
}
