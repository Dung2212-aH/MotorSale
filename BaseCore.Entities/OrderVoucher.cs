namespace BaseCore.Entities
{
    public class OrderVoucher
    {
        public int OrderId { get; set; }
        public int VoucherId { get; set; }
        public string VoucherCodeSnapshot { get; set; } = "";
        public decimal DiscountAmount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Order? Order { get; set; }
        public Voucher? Voucher { get; set; }
    }
}
