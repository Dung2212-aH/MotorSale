namespace BaseCore.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public string PaymentCode { get; set; } = "";
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "COD";
        public string PaymentStatus { get; set; } = "Pending";
        public string? TransactionRef { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string PaymentType { get; set; } = "Full";
        public decimal RefundedAmount { get; set; }
        public string? TransferContent { get; set; }
        public string? BankCode { get; set; }
        // Refund transaction details live in THANHTOAN_HOANTIEN in the current
        // schema. These properties are ignored by EF for backward API compatibility.
        public string? RefundTransactionRef { get; set; }
        public DateTime? RefundedAt { get; set; }
        public string? CancelReason { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? RawResponse { get; set; }

        public Order? Order { get; set; }
        public List<PaymentRefund> Refunds { get; set; } = new();
    }
}
