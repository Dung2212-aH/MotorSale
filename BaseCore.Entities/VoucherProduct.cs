namespace BaseCore.Entities
{
    public class VoucherProduct
    {
        public int VoucherId { get; set; }
        public int ProductId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Voucher? Voucher { get; set; }
        public Product? Product { get; set; }
    }
}
