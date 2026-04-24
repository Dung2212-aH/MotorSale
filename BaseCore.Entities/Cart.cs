namespace BaseCore.Entities
{
    public class Cart
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
        public List<CartItem> Items { get; set; } = new();
    }
}
