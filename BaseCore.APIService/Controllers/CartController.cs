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
    public class CartController : ControllerBase
    {
        private readonly BaseCoreDbContext _context;

        public CartController(BaseCoreDbContext context)
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

            var cart = await GetOrCreateCart(userId.Value);
            await _context.Entry(cart)
                .Collection(c => c.Items)
                .Query()
                .Include(i => i.Product)
                .Include(i => i.ProductVariant)
                .LoadAsync();

            return Ok(cart);
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] CartItemDto dto)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            if (dto.Quantity <= 0)
            {
                return BadRequest(new { message = "Quantity must be greater than zero" });
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == dto.ProductId && p.IsActive);
            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            if (product.StockQuantity < dto.Quantity)
            {
                return BadRequest(new { message = "Insufficient stock" });
            }

            ProductVariant? variant = null;
            if (dto.ProductVariantId.HasValue)
            {
                variant = await _context.ProductVariants.FirstOrDefaultAsync(v => v.Id == dto.ProductVariantId && v.ProductId == dto.ProductId);
                if (variant == null)
                {
                    return BadRequest(new { message = "Variant not found" });
                }
            }

            var cart = await GetOrCreateCart(userId.Value);
            var item = await _context.CartItems.FirstOrDefaultAsync(i =>
                i.CartId == cart.Id &&
                i.ProductId == dto.ProductId &&
                i.ProductVariantId == dto.ProductVariantId);

            var unitPrice = variant?.PriceOverride ?? product.SalePrice ?? product.BasePrice;
            if (item == null)
            {
                item = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = dto.ProductId,
                    ProductVariantId = dto.ProductVariantId,
                    Quantity = dto.Quantity,
                    UnitPrice = unitPrice,
                    LineTotal = unitPrice * dto.Quantity
                };
                _context.CartItems.Add(item);
            }
            else
            {
                item.Quantity += dto.Quantity;
                item.UnitPrice = unitPrice;
                item.LineTotal = item.UnitPrice * item.Quantity;
                item.UpdatedAt = DateTime.UtcNow;
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(item);
        }

        [HttpPut("items/{itemId:int}")]
        public async Task<IActionResult> UpdateItem(int itemId, [FromBody] UpdateCartItemDto dto)
        {
            if (dto.Quantity <= 0)
            {
                return BadRequest(new { message = "Quantity must be greater than zero" });
            }

            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var item = await _context.CartItems
                .Include(i => i.Cart)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.Cart!.UserId == userId.Value && i.Cart.Status == "Active");

            if (item == null)
            {
                return NotFound(new { message = "Cart item not found" });
            }

            item.Quantity = dto.Quantity;
            item.LineTotal = item.UnitPrice * item.Quantity;
            item.UpdatedAt = DateTime.UtcNow;
            item.Cart!.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(item);
        }

        [HttpDelete("items/{itemId:int}")]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var item = await _context.CartItems
                .Include(i => i.Cart)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.Cart!.UserId == userId.Value && i.Cart.Status == "Active");

            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Cart item removed" });
        }

        private async Task<Cart> GetOrCreateCart(int userId)
        {
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId && c.Status == "Active");
            if (cart != null)
            {
                return cart;
            }

            cart = new Cart { UserId = userId, Status = "Active" };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        private int? GetUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userId, out var parsedUserId) ? parsedUserId : null;
        }
    }

    public class CartItemDto
    {
        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateCartItemDto
    {
        public int Quantity { get; set; }
    }
}
