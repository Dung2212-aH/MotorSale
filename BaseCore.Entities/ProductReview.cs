namespace BaseCore.Entities
{
    public class ProductReview
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public int? OrderId { get; set; }
        public byte Rating { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Product? Product { get; set; }
        public User? User { get; set; }
        public Order? Order { get; set; }
    }
}
