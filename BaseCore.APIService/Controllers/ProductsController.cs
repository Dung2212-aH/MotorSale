using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BaseCore.Entities;
using BaseCore.Repository;
using BaseCore.Repository.EFCore;

namespace BaseCore.APIService.Controllers
{
    /// <summary>
    /// Product API Controller for motorcycles and parts.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepositoryEF _productRepository;
        private readonly ICategoryRepositoryEF _categoryRepository;
        private readonly BaseCoreDbContext _context;

        public ProductsController(
            IProductRepositoryEF productRepository,
            ICategoryRepositoryEF categoryRepository,
            BaseCoreDbContext context)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? keyword,
            [FromQuery] int? categoryId,
            [FromQuery] string? productType,
            [FromQuery] int? brandId,
            [FromQuery] int? carModelId,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? color,
            [FromQuery] int? showroomId,
            [FromQuery] string? status,
            [FromQuery] string? sortBy,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

            var (products, totalCount) = await _productRepository.SearchAsync(
                keyword,
                categoryId,
                productType,
                brandId,
                carModelId,
                minPrice,
                maxPrice,
                null,
                null,
                null,
                null,
                color,
                showroomId,
                status,
                sortBy,
                page,
                pageSize);

            return Ok(new
            {
                items = products.Select(ToListItemDto),
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }

        [HttpGet("filters")]
        public async Task<IActionResult> GetFilters()
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            var brands = await _context.Brands
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .ToListAsync();

            var carModels = await _context.CarModels
                .Where(m => m.IsActive)
                .OrderBy(m => m.BrandId)
                .ThenBy(m => m.Name)
                .ToListAsync();

            var showrooms = await _context.Showrooms
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return Ok(new
            {
                categories = categories.Select(c => new
                {
                    c.Id,
                    c.ParentCategoryId,
                    c.Name,
                    c.Slug,
                    c.Description,
                    c.SortOrder,
                    c.IsActive
                }),
                brands = brands.Select(b => new
                {
                    b.Id,
                    b.Name,
                    b.Slug,
                    b.LogoUrl,
                    b.IsActive
                }),
                carModels = carModels.Select(m => new
                {
                    m.Id,
                    m.BrandId,
                    m.Name,
                    m.Slug,
                    m.IsActive
                }),
                showrooms = showrooms.Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Slug,
                    s.AddressLine,
                    s.Ward,
                    s.District,
                    s.Province,
                    s.PhoneNumber,
                    s.Email,
                    s.OpeningHours,
                    s.IsActive
                })
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            return Ok(ToDetailDto(product));
        }

        [HttpGet("{id:int}/variants")]
        public async Task<IActionResult> GetVariants(int id)
        {
            var variants = await _context.ProductVariants
                .Where(v => v.ProductId == id)
                .Include(v => v.Images)
                .OrderBy(v => v.Version)
                .ThenBy(v => v.Color)
                .ToListAsync();

            return Ok(variants.Select(v => new ProductVariantDto
            {
                Id = v.Id,
                VariantName = v.VariantName,
                Sku = v.Sku,
                PriceOverride = v.PriceOverride,
                StockQuantity = v.StockQuantity,
                Status = v.Status,
                Version = v.Version,
                Color = v.Color,
                Images = v.Images.OrderBy(i => i.SortOrder).Select(i => new ProductImageDto
                {
                    Id = i.Id,
                    ProductVariantId = i.ProductVariantId,
                    ImageUrl = i.ImageUrl,
                    AltText = i.AltText,
                    IsPrimary = i.IsPrimary,
                    SortOrder = i.SortOrder
                }).ToList()
            }));
        }

        [HttpGet("{id:int}/colors")]
        public async Task<IActionResult> GetColors(int id)
        {
            var colors = await _context.ProductVariants
                .Where(v => v.ProductId == id && v.Color != null)
                .Select(v => new { v.Id, v.Version, v.Color, v.StockQuantity, v.PriceOverride })
                .OrderBy(v => v.Version)
                .ThenBy(v => v.Color)
                .ToListAsync();

            return Ok(colors);
        }

        [HttpGet("{id:int}/images")]
        public async Task<IActionResult> GetImages(int id, [FromQuery] int? variantId)
        {
            var query = _context.ProductImages.Where(i => i.ProductId == id);
            if (variantId.HasValue)
            {
                query = query.Where(i => i.ProductVariantId == variantId.Value || i.ProductVariantId == null);
            }

            var images = await query.OrderBy(i => i.ProductVariantId == variantId ? 0 : 1)
                .ThenByDescending(i => i.IsPrimary)
                .ThenBy(i => i.SortOrder)
                .ToListAsync();

            return Ok(images.Select(i => new ProductImageDto
            {
                Id = i.Id,
                ProductVariantId = i.ProductVariantId,
                ImageUrl = i.ImageUrl,
                AltText = i.AltText,
                IsPrimary = i.IsPrimary,
                SortOrder = i.SortOrder
            }));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] ProductCreateDto dto)
        {
            var validation = await ValidateReferences(dto.CategoryId, dto.BrandId, dto.CarModelId, dto.ShowroomId);
            if (validation != null)
            {
                return validation;
            }

            var product = new Product();
            ApplyDto(product, dto);
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.AddAsync(product);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateDto dto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            var categoryId = dto.CategoryId ?? product.CategoryId;
            var brandId = dto.BrandId ?? product.BrandId;
            var carModelId = dto.CarModelId ?? product.CarModelId;
            var showroomId = dto.ShowroomId ?? product.ShowroomId;
            var validation = await ValidateReferences(categoryId, brandId, carModelId, showroomId);
            if (validation != null)
            {
                return validation;
            }

            ApplyDto(product, dto);
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);
            return Ok(product);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            product.IsActive = false;
            product.Status = "Hidden";
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(product);

            return Ok(new { message = "Product hidden successfully" });
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var products = await _productRepository.GetByCategoryAsync(categoryId);
            return Ok(products.Select(ToListItemDto));
        }

        private static ProductListItemDto ToListItemDto(Product product)
        {
            return new ProductListItemDto
            {
                Id = product.Id,
                ProductCode = product.ProductCode,
                Name = product.Name,
                Slug = product.Slug,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                BrandId = product.BrandId,
                BrandName = product.Brand?.Name,
                CarModelId = product.CarModelId,
                CarModelName = product.CarModel?.Name,
                ShowroomId = product.ShowroomId,
                ShowroomName = product.Showroom?.Name,
                ProductType = product.ProductType,
                ShortDescription = product.ShortDescription,
                BasePrice = product.BasePrice,
                SalePrice = product.SalePrice,
                StockQuantity = product.StockQuantity,
                MainImageUrl = ResolveMainImageUrl(product),
                IsActive = product.IsActive,
                MainColor = product.MainColor,
                MotorcycleType = product.MotorcycleType,
                EngineCapacity = product.EngineCapacity,
                Status = product.Status,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }

        private static ProductDetailDto ToDetailDto(Product product)
        {
            var dto = new ProductDetailDto
            {
                Id = product.Id,
                ProductCode = product.ProductCode,
                Name = product.Name,
                Slug = product.Slug,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                BrandId = product.BrandId,
                BrandName = product.Brand?.Name,
                CarModelId = product.CarModelId,
                CarModelName = product.CarModel?.Name,
                ShowroomId = product.ShowroomId,
                ShowroomName = product.Showroom?.Name,
                ProductType = product.ProductType,
                ShortDescription = product.ShortDescription,
                Description = product.Description,
                BasePrice = product.BasePrice,
                SalePrice = product.SalePrice,
                StockQuantity = product.StockQuantity,
                MainImageUrl = ResolveMainImageUrl(product),
                IsActive = product.IsActive,
                MainColor = product.MainColor,
                MotorcycleType = product.MotorcycleType,
                EngineCapacity = product.EngineCapacity,
                Power = product.Power,
                Torque = product.Torque,
                FuelTankCapacity = product.FuelTankCapacity,
                FrontBrake = product.FrontBrake,
                RearBrake = product.RearBrake,
                HasAbs = product.HasAbs,
                Weight = product.Weight,
                SeatHeight = product.SeatHeight,
                Origin = product.Origin,
                WarrantyMonths = product.WarrantyMonths,
                Status = product.Status,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Images = product.Images
                    .OrderBy(i => i.SortOrder)
                    .Select(i => new ProductImageDto
                    {
                        Id = i.Id,
                        ProductVariantId = i.ProductVariantId,
                        ImageUrl = i.ImageUrl,
                        AltText = i.AltText,
                        IsPrimary = i.IsPrimary,
                        SortOrder = i.SortOrder
                    })
                    .ToList(),
                Variants = product.Variants
                    .Select(v => new ProductVariantDto
                    {
                        Id = v.Id,
                        VariantName = v.VariantName,
                        Sku = v.Sku,
                        PriceOverride = v.PriceOverride,
                        StockQuantity = v.StockQuantity,
                        Status = v.Status,
                        Version = v.Version,
                        Color = v.Color,
                        Images = v.Images
                            .OrderBy(i => i.SortOrder)
                            .Select(i => new ProductImageDto
                            {
                                Id = i.Id,
                                ProductVariantId = i.ProductVariantId,
                                ImageUrl = i.ImageUrl,
                                AltText = i.AltText,
                                IsPrimary = i.IsPrimary,
                                SortOrder = i.SortOrder
                            })
                            .ToList()
                    })
                    .ToList()
            };

            return dto;
        }

        private static string? ResolveMainImageUrl(Product product)
        {
            return product.Images
                .OrderByDescending(i => i.IsPrimary)
                .ThenBy(i => i.SortOrder)
                .ThenBy(i => i.Id)
                .Select(i => i.ImageUrl)
                .FirstOrDefault(url => !string.IsNullOrWhiteSpace(url))
                ?? product.MainImageUrl;
        }

        private async Task<IActionResult?> ValidateReferences(int categoryId, int? brandId, int? carModelId, int? showroomId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
            {
                return BadRequest(new { message = "Category not found" });
            }

            if (brandId.HasValue && !await _context.Brands.AnyAsync(b => b.Id == brandId.Value))
            {
                return BadRequest(new { message = "Brand not found" });
            }

            if (carModelId.HasValue && !await _context.CarModels.AnyAsync(m => m.Id == carModelId.Value))
            {
                return BadRequest(new { message = "Car model not found" });
            }

            if (showroomId.HasValue && !await _context.Showrooms.AnyAsync(s => s.Id == showroomId.Value))
            {
                return BadRequest(new { message = "Showroom not found" });
            }

            return null;
        }

        private static void ApplyDto(Product product, ProductCreateDto dto)
        {
            product.ProductCode = dto.ProductCode;
            product.Name = dto.Name;
            product.Slug = dto.Slug;
            product.CategoryId = dto.CategoryId;
            product.BrandId = dto.BrandId;
            product.CarModelId = dto.CarModelId;
            product.ShowroomId = dto.ShowroomId;
            product.ProductType = dto.ProductType;
            product.ShortDescription = dto.ShortDescription;
            product.Description = dto.Description;
            product.BasePrice = dto.BasePrice;
            product.SalePrice = dto.SalePrice;
            product.StockQuantity = dto.StockQuantity;
            product.MainImageUrl = dto.MainImageUrl;
            product.IsActive = dto.IsActive;
            product.MainColor = dto.MainColor;
            product.MotorcycleType = dto.MotorcycleType;
            product.EngineCapacity = dto.EngineCapacity;
            product.Power = dto.Power;
            product.Torque = dto.Torque;
            product.FuelTankCapacity = dto.FuelTankCapacity;
            product.FrontBrake = dto.FrontBrake;
            product.RearBrake = dto.RearBrake;
            product.HasAbs = dto.HasAbs;
            product.Weight = dto.Weight;
            product.SeatHeight = dto.SeatHeight;
            product.Origin = dto.Origin;
            product.WarrantyMonths = dto.WarrantyMonths;
            product.Status = dto.Status;
        }

        private static void ApplyDto(Product product, ProductUpdateDto dto)
        {
            product.ProductCode = dto.ProductCode ?? product.ProductCode;
            product.Name = dto.Name ?? product.Name;
            product.Slug = dto.Slug ?? product.Slug;
            product.CategoryId = dto.CategoryId ?? product.CategoryId;
            product.BrandId = dto.BrandId ?? product.BrandId;
            product.CarModelId = dto.CarModelId ?? product.CarModelId;
            product.ShowroomId = dto.ShowroomId ?? product.ShowroomId;
            product.ProductType = dto.ProductType ?? product.ProductType;
            product.ShortDescription = dto.ShortDescription ?? product.ShortDescription;
            product.Description = dto.Description ?? product.Description;
            product.BasePrice = dto.BasePrice ?? product.BasePrice;
            product.SalePrice = dto.SalePrice ?? product.SalePrice;
            product.StockQuantity = dto.StockQuantity ?? product.StockQuantity;
            product.MainImageUrl = dto.MainImageUrl ?? product.MainImageUrl;
            product.IsActive = dto.IsActive ?? product.IsActive;
            product.MainColor = dto.MainColor ?? product.MainColor;
            product.MotorcycleType = dto.MotorcycleType ?? product.MotorcycleType;
            product.EngineCapacity = dto.EngineCapacity ?? product.EngineCapacity;
            product.Power = dto.Power ?? product.Power;
            product.Torque = dto.Torque ?? product.Torque;
            product.FuelTankCapacity = dto.FuelTankCapacity ?? product.FuelTankCapacity;
            product.FrontBrake = dto.FrontBrake ?? product.FrontBrake;
            product.RearBrake = dto.RearBrake ?? product.RearBrake;
            product.HasAbs = dto.HasAbs ?? product.HasAbs;
            product.Weight = dto.Weight ?? product.Weight;
            product.SeatHeight = dto.SeatHeight ?? product.SeatHeight;
            product.Origin = dto.Origin ?? product.Origin;
            product.WarrantyMonths = dto.WarrantyMonths ?? product.WarrantyMonths;
            product.Status = dto.Status ?? product.Status;
        }
    }

    public class ProductCreateDto
    {
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
        public string Status { get; set; } = "Available";
    }

    public class ProductListItemDto
    {
        public int Id { get; set; }
        public string ProductCode { get; set; } = "";
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? BrandId { get; set; }
        public string? BrandName { get; set; }
        public int? CarModelId { get; set; }
        public string? CarModelName { get; set; }
        public int? ShowroomId { get; set; }
        public string? ShowroomName { get; set; }
        public string ProductType { get; set; } = "";
        public string? ShortDescription { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? SalePrice { get; set; }
        public int StockQuantity { get; set; }
        public string? MainImageUrl { get; set; }
        public bool IsActive { get; set; }
        public string? MainColor { get; set; }
        public string? MotorcycleType { get; set; }
        public int? EngineCapacity { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ProductDetailDto : ProductListItemDto
    {
        public string? Description { get; set; }
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
        public List<ProductImageDto> Images { get; set; } = new();
        public List<ProductVariantDto> Variants { get; set; } = new();
    }

    public class ProductImageDto
    {
        public int Id { get; set; }
        public int? ProductVariantId { get; set; }
        public string ImageUrl { get; set; } = "";
        public string? AltText { get; set; }
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
    }

    public class ProductVariantDto
    {
        public int Id { get; set; }
        public string VariantName { get; set; } = "";
        public string Sku { get; set; } = "";
        public decimal? PriceOverride { get; set; }
        public int? StockQuantity { get; set; }
        public string Status { get; set; } = "";
        public string? Version { get; set; }
        public string? Color { get; set; }
        public List<ProductImageDto> Images { get; set; } = new();
    }

    public class ProductUpdateDto
    {
        public string? ProductCode { get; set; }
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public int? CarModelId { get; set; }
        public int? ShowroomId { get; set; }
        public string? ProductType { get; set; }
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public decimal? BasePrice { get; set; }
        public decimal? SalePrice { get; set; }
        public int? StockQuantity { get; set; }
        public string? MainImageUrl { get; set; }
        public bool? IsActive { get; set; }
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
        public string? Status { get; set; }
    }
}
