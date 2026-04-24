namespace BaseCore.Entities
{
    public class OrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }
        public string ProductNameSnapshot { get; set; } = "";
        public string? SkuSnapshot { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }

        public Order? Order { get; set; }
        public Product? Product { get; set; }
        public ProductVariant? ProductVariant { get; set; }
    }
}
