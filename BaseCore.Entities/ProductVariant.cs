namespace BaseCore.Entities
{
    public class ProductVariant
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string VariantName { get; set; } = "";
        public string Sku { get; set; } = "";
        public decimal? PriceOverride { get; set; }
        public int? StockQuantity { get; set; }
        public string Status { get; set; } = "Available";
        public string? Version { get; set; }
        public string? ExteriorColor { get; set; }
        public string? InteriorColor { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
