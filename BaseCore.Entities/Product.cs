namespace BaseCore.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string ProductCode { get; set; } = "";
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public int CategoryId { get; set; }
        public int? BrandId { get; set; }
        public int? CarModelId { get; set; }
        public int? ShowroomId { get; set; }
        public string ProductType { get; set; } = "Motorcycle";
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? SalePrice { get; set; }
        public int StockQuantity { get; set; }
        public string? MainImageUrl { get; set; }
        public bool IsActive { get; set; } = true;

        public string? MainColor { get; set; }
        public string? MotorcycleType { get; set; }
        public int? EngineCapacity { get; set; }
        public string? Power { get; set; }
        public string? Torque { get; set; }
        public decimal? FuelTankCapacity { get; set; }
        public string? FrontBrake { get; set; }
        public string? RearBrake { get; set; }
        public bool? HasAbs { get; set; }
        public decimal? Weight { get; set; }
        public int? SeatHeight { get; set; }
        public string? Origin { get; set; }
        public int? WarrantyMonths { get; set; }
        public string? Condition { get; set; }
        public int? Year { get; set; }
        public int? Mileage { get; set; }
        public string? ExteriorColor { get; set; }
        public string? InteriorColor { get; set; }
        public int? Seats { get; set; }
        public string? Transmission { get; set; }
        public string? FuelType { get; set; }
        public string? Engine { get; set; }
        public string? DriveType { get; set; }
        public string? Vin { get; set; }
        public string? LicensePlate { get; set; }
        public string Status { get; set; } = "Available";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Category? Category { get; set; }
        public Brand? Brand { get; set; }
        public CarModel? CarModel { get; set; }
        public Showroom? Showroom { get; set; }
        public List<ProductVariant> Variants { get; set; } = new();
        public List<ProductImage> Images { get; set; } = new();
    }
}
