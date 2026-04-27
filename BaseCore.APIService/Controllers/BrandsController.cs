using BaseCore.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.APIService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandsController : ControllerBase
    {
        private readonly BaseCoreDbContext _context;

        public BrandsController(BaseCoreDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var brands = await _context.Brands
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .ToListAsync();

            return Ok(brands);
        }

        [HttpGet("{id:int}/models")]
        public async Task<IActionResult> GetModels(int id)
        {
            var models = await _context.CarModels
                .Where(m => m.BrandId == id && m.IsActive)
                .OrderBy(m => m.Name)
                .ToListAsync();

            return Ok(models);
        }
    }
}
