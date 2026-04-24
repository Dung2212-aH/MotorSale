using BaseCore.Entities;
using BaseCore.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BaseCore.APIService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly BaseCoreDbContext _context;

        public FavoritesController(BaseCoreDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMine()
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId.Value)
                .Include(f => f.Product)
                .ThenInclude(p => p!.Brand)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return Ok(favorites);
        }

        [HttpPost("{productId:int}")]
        public async Task<IActionResult> Add(int productId)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var productExists = await _context.Products.AnyAsync(p => p.Id == productId && p.IsActive);
            if (!productExists)
            {
                return NotFound(new { message = "Product not found" });
            }

            var exists = await _context.Favorites.AnyAsync(f => f.UserId == userId.Value && f.ProductId == productId);
            if (!exists)
            {
                _context.Favorites.Add(new Favorite { UserId = userId.Value, ProductId = productId });
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Product added to favorites" });
        }

        [HttpDelete("{productId:int}")]
        public async Task<IActionResult> Remove(int productId)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var favorite = await _context.Favorites.FirstOrDefaultAsync(f => f.UserId == userId.Value && f.ProductId == productId);
            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Product removed from favorites" });
        }

        private int? GetUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userId, out var parsedUserId) ? parsedUserId : null;
        }
    }
}
