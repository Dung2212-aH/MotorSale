namespace BaseCore.Entities
{
    public class InventoryHold
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int? OrderDetailId { get; set; }
        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string? Note { get; set; }

        public Order? Order { get; set; }
        public OrderDetail? OrderDetail { get; set; }
        public Product? Product { get; set; }
        public ProductVariant? ProductVariant { get; set; }
    }
}
