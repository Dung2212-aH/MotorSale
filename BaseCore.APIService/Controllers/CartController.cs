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

            var cart = await LoadCart(userId.Value);
            return Ok(ToCartDto(cart));
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

            ProductVariant? variant = null;
            if (dto.ProductVariantId.HasValue)
            {
                variant = await _context.ProductVariants.FirstOrDefaultAsync(v => v.Id == dto.ProductVariantId && v.ProductId == dto.ProductId);
                if (variant == null)
                {
                    return BadRequest(new { message = "Variant not found" });
                }
            }

            var availableStock = variant?.StockQuantity ?? product.StockQuantity;
            if (availableStock < dto.Quantity)
            {
                return BadRequest(new { message = "Insufficient stock" });
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

            cart = await LoadCart(userId.Value);
            return Ok(ToCartDto(cart));
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

            var cart = await LoadCart(userId.Value);
            return Ok(ToCartDto(cart));
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

            var cart = await LoadCart(userId.Value);
            return Ok(ToCartDto(cart));
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var cart = await GetOrCreateCart(userId.Value);
            var items = await _context.CartItems.Where(i => i.CartId == cart.Id).ToListAsync();

            if (items.Any())
            {
                _context.CartItems.RemoveRange(items);
                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            var updatedCart = await LoadCart(userId.Value);
            return Ok(ToCartDto(updatedCart));
        }

        private async Task<Cart> LoadCart(int userId)
        {
            var cart = await GetOrCreateCart(userId);

            await _context.Entry(cart)
                .Collection(c => c.Items)
                .Query()
                .Include(i => i.Product)
                    .ThenInclude(p => p!.Brand)
                .Include(i => i.Product)
                    .ThenInclude(p => p!.Category)
                .Include(i => i.Product)
                    .ThenInclude(p => p!.Images)
                .Include(i => i.ProductVariant)
                .LoadAsync();

            return cart;
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

        private static object ToCartDto(Cart cart)
        {
            var items = cart.Items
                .OrderBy(i => i.Id)
                .Select(i =>
                {
                    var product = i.Product;
                    var variant = i.ProductVariant;
                    var unitPrice = i.UnitPrice;
                    var quantity = i.Quantity;

                    return new
                    {
                        i.Id,
                        i.CartId,
                        i.ProductId,
                        i.ProductVariantId,
                        Quantity = quantity,
                        UnitPrice = unitPrice,
                        LineTotal = unitPrice * quantity,
                        Product = product == null ? null : new
                        {
                            product.Id,
                            product.ProductCode,
                            product.Name,
                            product.Slug,
                            product.CategoryId,
                            CategoryName = product.Category?.Name,
                            product.BrandId,
                            BrandName = product.Brand?.Name,
                            product.ProductType,
                            product.ShortDescription,
                            product.BasePrice,
                            product.SalePrice,
                            product.StockQuantity,
                            MainImageUrl = ResolveMainImageUrl(product),
                            product.IsActive,
                            product.Status
                        },
                        ProductVariant = variant == null ? null : new
                        {
                            variant.Id,
                            variant.ProductId,
                            variant.VariantName,
                            variant.Sku,
                            variant.PriceOverride,
                            variant.StockQuantity,
                            variant.Status,
                            variant.Version,
                            variant.Color
                        }
                    };
                })
                .ToList();

            return new
            {
                cart.Id,
                cart.UserId,
                cart.Status,
                cart.CreatedAt,
                cart.UpdatedAt,
                Items = items,
                TotalItems = items.Sum(i => i.Quantity),
                Subtotal = items.Sum(i => i.LineTotal)
            };
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
