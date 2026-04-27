namespace BaseCore.Entities
{
    public class PartCompatibility
    {
        public int Id { get; set; }
        public int PartProductId { get; set; }
        public int? BrandId { get; set; }
        public int? CarModelId { get; set; }
        public short? FromYear { get; set; }
        public short? ToYear { get; set; }
        public bool AppliesToAllMotorcycles { get; set; }
        public string? Note { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Product? PartProduct { get; set; }
        public Brand? Brand { get; set; }
        public CarModel? CarModel { get; set; }
    }
}
