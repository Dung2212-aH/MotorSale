namespace BaseCore.Entities
{
    public class VoucherBrand
    {
        public int VoucherId { get; set; }
        public int BrandId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Voucher? Voucher { get; set; }
        public Brand? Brand { get; set; }
    }
}
