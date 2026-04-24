namespace BaseCore.Entities
{
    public class ContactRequest
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string? Email { get; set; }
        public string? Subject { get; set; }
        public string Message { get; set; } = "";
        public string InquiryType { get; set; } = "General";
        public int? ProductId { get; set; }
        public int? ShowroomId { get; set; }
        public string Status { get; set; } = "New";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
        public int? ProcessedByUserId { get; set; }

        public Product? Product { get; set; }
        public Showroom? Showroom { get; set; }
        public User? ProcessedByUser { get; set; }
    }
}
