namespace BaseCore.Entities
{
    public class CarModel
    {
        public int Id { get; set; }
        public int BrandId { get; set; }
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Brand? Brand { get; set; }
    }
}
