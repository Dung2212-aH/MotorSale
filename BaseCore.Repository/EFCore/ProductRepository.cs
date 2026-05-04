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
            int? compatibleCarModelId,
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
                    .ThenInclude(v => v.Images)
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
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .ToListAsync();
        }

        public async Task<(List<Product> Products, int TotalCount)> SearchAsync(
            string? keyword,
            int? categoryId,
            string? productType,
            int? brandId,
            int? carModelId,
            int? compatibleCarModelId,
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
                .Include(p => p.Images)
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
                var categoryIds = await _context.Categories
                    .Where(c => c.Id == categoryId.Value || c.ParentCategoryId == categoryId.Value)
                    .Select(c => c.Id)
                    .ToListAsync();

                if (categoryIds.Count == 0)
                {
                    categoryIds.Add(categoryId.Value);
                }

                query = query.Where(p => categoryIds.Contains(p.CategoryId));
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

            if (compatibleCarModelId.HasValue && compatibleCarModelId > 0)
            {
                query = query.Where(p => _context.PartCompatibilities.Any(pc =>
                    pc.PartProductId == p.Id &&
                    pc.IsActive &&
                    (pc.AppliesToAllMotorcycles || pc.CarModelId == compatibleCarModelId.Value)));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => (p.SalePrice ?? p.BasePrice) >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => (p.SalePrice ?? p.BasePrice) <= maxPrice.Value);
            }

            if (!string.IsNullOrWhiteSpace(color))
            {
                var normalizedColor = color.Trim().ToLower();
                query = query.Where(p =>
                    (p.MainColor != null && p.MainColor.ToLower().Contains(normalizedColor)) ||
                    p.Variants.Any(v => v.Color != null && v.Color.ToLower().Contains(normalizedColor)));
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
                .Include(p => p.Images)
                .ToListAsync();
        }
    }
}
