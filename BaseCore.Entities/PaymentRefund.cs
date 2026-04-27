namespace BaseCore.Entities
{
    public class PaymentRefund
    {
        public int Id { get; set; }
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string? RefundTransactionRef { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = "Succeeded";
        public string? RawResponse { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Payment? Payment { get; set; }
        public Order? Order { get; set; }
    }
}
