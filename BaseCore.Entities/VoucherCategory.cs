namespace BaseCore.Entities
{
    public class VoucherCategory
    {
        public int VoucherId { get; set; }
        public int CategoryId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Voucher? Voucher { get; set; }
        public Category? Category { get; set; }
    }
}
