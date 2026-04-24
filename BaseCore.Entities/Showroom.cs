namespace BaseCore.Entities
{
    public class Showroom
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public string AddressLine { get; set; } = "";
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string Province { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? OpeningHours { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
