using Microsoft.EntityFrameworkCore;
using BaseCore.Entities;

namespace BaseCore.Repository.EFCore
{
    /// <summary>
    /// Product Repository using Entity Framework Core
    /// </summary>
    public interface IProductRepositoryEF : IRepository<Product>
    {
        Task<(List<Product> Products, int TotalCount)> SearchAsync(
            string? keyword,
            int? categoryId,
            string? productType,
            int? brandId,
            int? carModelId,
            decimal? minPrice,
            decimal? maxPrice,
            int? year,
            string? condition,
            string? fuelType,
            string? transmission,
            string? color,
            int? showroomId,
            string? status,
            string? sortBy,
            int page,
            int pageSize);
        Task<List<Product>> GetByCategoryAsync(int categoryId);
    }

    public class ProductRepositoryEF : Repository<Product>, IProductRepositoryEF
    {
        public ProductRepositoryEF(BaseCoreDbContext context) : base(context)
        {
        }

        public override async Task<Product?> GetByIdAsync(object id)
        {
            if (id is not int productId)
            {
                return null;
            }

            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.CarModel)
                .Include(p => p.Showroom)
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == productId);
        }

        public override async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.CarModel)
                .Include(p => p.Showroom)
                .ToListAsync();
        }

        public async Task<(List<Product> Products, int TotalCount)> SearchAsync(
            string? keyword,
            int? categoryId,
            string? productType,
            int? brandId,
            int? carModelId,
            decimal? minPrice,
            decimal? maxPrice,
            int? year,
            string? condition,
            string? fuelType,
            string? transmission,
            string? color,
            int? showroomId,
            string? status,
            string? sortBy,
            int page,
            int pageSize)
        {
            var query = _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.CarModel)
                .Include(p => p.Showroom)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(keyword) ||
                    p.ProductCode.ToLower().Contains(keyword) ||
                    p.Slug.ToLower().Contains(keyword) ||
                    (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(keyword)) ||
                    (p.Description != null && p.Description.ToLower().Contains(keyword)));
            }

            if (categoryId.HasValue && categoryId > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            if (!string.IsNullOrWhiteSpace(productType))
            {
                query = query.Where(p => p.ProductType == productType);
            }

            if (brandId.HasValue && brandId > 0)
            {
                query = query.Where(p => p.BrandId == brandId);
            }

            if (carModelId.HasValue && carModelId > 0)
            {
                query = query.Where(p => p.CarModelId == carModelId);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => (p.SalePrice ?? p.BasePrice) >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => (p.SalePrice ?? p.BasePrice) <= maxPrice.Value);
            }

            if (year.HasValue)
            {
                query = query.Where(p => p.Year == year);
            }

            if (!string.IsNullOrWhiteSpace(condition))
            {
                query = query.Where(p => p.Condition == condition);
            }

            if (!string.IsNullOrWhiteSpace(fuelType))
            {
                query = query.Where(p => p.FuelType == fuelType);
            }

            if (!string.IsNullOrWhiteSpace(transmission))
            {
                query = query.Where(p => p.Transmission == transmission);
            }

            if (!string.IsNullOrWhiteSpace(color))
            {
                var normalizedColor = color.Trim().ToLower();
                query = query.Where(p =>
                    (p.ExteriorColor != null && p.ExteriorColor.ToLower().Contains(normalizedColor)) ||
                    (p.InteriorColor != null && p.InteriorColor.ToLower().Contains(normalizedColor)));
            }

            if (showroomId.HasValue && showroomId > 0)
            {
                query = query.Where(p => p.ShowroomId == showroomId);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(p => p.Status == status);
            }

            var totalCount = await query.CountAsync();

            query = sortBy switch
            {
                "price_asc" => query.OrderBy(p => p.SalePrice ?? p.BasePrice),
                "price_desc" => query.OrderByDescending(p => p.SalePrice ?? p.BasePrice),
                "year_desc" => query.OrderByDescending(p => p.Year),
                "year_asc" => query.OrderBy(p => p.Year),
                "name_asc" => query.OrderBy(p => p.Name),
                "name_desc" => query.OrderByDescending(p => p.Name),
                _ => query.OrderByDescending(p => p.Id)
            };

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalCount);
        }

        public async Task<List<Product>> GetByCategoryAsync(int categoryId)
        {
            return await _dbSet
                .Where(p => p.CategoryId == categoryId)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.CarModel)
                .Include(p => p.Showroom)
                .ToListAsync();
        }
    }
}
