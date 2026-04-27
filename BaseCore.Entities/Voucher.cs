namespace BaseCore.Entities
{
    public class Voucher
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string DiscountType { get; set; } = "Amount";
        public decimal DiscountValue { get; set; }
        public decimal MinOrderValue { get; set; }
        public decimal? MaxDiscountValue { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public int? UsageLimit { get; set; }
        public int UsedCount { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Description { get; set; }
        public int MaxUsagePerUser { get; set; } = 1;
        public string Scope { get; set; } = "All";
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public List<VoucherCategory> Categories { get; set; } = new();
        public List<VoucherBrand> Brands { get; set; } = new();
        public List<VoucherProduct> Products { get; set; } = new();
    }
}
