using Microsoft.EntityFrameworkCore;
using BaseCore.Entities;

namespace BaseCore.Repository.EFCore
{
    /// <summary>
    /// Category Repository using Entity Framework Core
    /// </summary>
    public interface ICategoryRepositoryEF : IRepository<Category>
    {
        Task<Category?> GetByNameAsync(string name);
    }

    public class CategoryRepositoryEF : Repository<Category>, ICategoryRepositoryEF
    {
        public CategoryRepositoryEF(BaseCoreDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _dbSet
                .Include(c => c.ParentCategory)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
        }
    }
}
