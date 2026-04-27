using BaseCore.Entities;
using BaseCore.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Data.Common;
using System.Globalization;
using System.Text;

namespace BaseCore.APIService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VouchersController : ControllerBase
    {
        private static readonly string[] VoucherOrderTypeColumnCandidates =
        {
            "LoaiDonHangApDung",
            "LoaiDonHang",
            "ApDungChoLoaiDonHang",
            "KieuDonHang",
            "HinhThucDonHang",
            "HinhThucThanhToan",
            "LoaiThanhToanApDung",
            "LoaiThanhToan",
            "PhuongThucThanhToan",
            "ApDungChoThanhToan",
            "ApDungLoaiDonHang"
        };

        private readonly BaseCoreDbContext _context;

        public VouchersController(BaseCoreDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns currently visible vouchers for the storefront.
        /// Optional product/category/brand filters keep scoped vouchers relevant to a product page.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailable(
            [FromQuery] int? productId,
            [FromQuery] int? categoryId,
            [FromQuery] int? brandId)
        {
            var now = DateTime.Now;
            var vouchers = await _context.Vouchers
                .AsNoTracking()
                .Include(v => v.Categories)
                .Include(v => v.Brands)
                .Include(v => v.Products)
                .Where(v =>
                    v.IsActive &&
                    v.StartAt <= now &&
                    v.EndAt >= now &&
                    (!v.UsageLimit.HasValue || v.UsedCount < v.UsageLimit.Value))
                .OrderBy(v => v.MinOrderValue)
                .ThenBy(v => v.Code)
                .ToListAsync();

            var visibleVouchers = vouchers
                .Where(v => IsVoucherVisibleForContext(v, productId, categoryId, brandId))
                .Select(ToVoucherListItem)
                .ToList();

            return Ok(visibleVouchers);
        }

        /// <summary>
        /// Validates a voucher code against the current cart.
        /// Returns the voucher info and calculated discount amount.
        /// </summary>
        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] ValidateVoucherDto dto)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(dto.Code))
            {
                return BadRequest(new { valid = false, message = "Vui lòng nhập mã voucher" });
            }

            var voucher = await _context.Vouchers
                .Include(v => v.Categories)
                .Include(v => v.Brands)
                .Include(v => v.Products)
                .FirstOrDefaultAsync(v => v.Code == dto.Code.Trim());

            if (voucher == null)
            {
                return Ok(new { valid = false, message = "Mã voucher không tồn tại" });
            }

            if (!voucher.IsActive)
            {
                return Ok(new { valid = false, message = "Voucher đã ngưng hoạt động" });
            }

            var now = DateTime.Now;
            if (now < voucher.StartAt || now > voucher.EndAt)
            {
                return Ok(new { valid = false, message = "Voucher đã hết hạn hoặc chưa đến thời gian sử dụng" });
            }

            if (voucher.UsageLimit.HasValue && voucher.UsedCount >= voucher.UsageLimit.Value)
            {
                return Ok(new { valid = false, message = "Voucher đã hết lượt sử dụng" });
            }

            // Check per-user usage limit
            var userUsageCount = await _context.OrderVouchers
                .CountAsync(ov => ov.VoucherId == voucher.Id && ov.Order!.UserId == userId.Value);

            if (userUsageCount >= voucher.MaxUsagePerUser)
            {
                return Ok(new { valid = false, message = $"Bạn đã sử dụng voucher này {userUsageCount} lần (tối đa {voucher.MaxUsagePerUser})" });
            }

            if (dto.Subtotal < voucher.MinOrderValue)
            {
                return Ok(new
                {
                    valid = false,
                    message = $"Đơn hàng tối thiểu {voucher.MinOrderValue:N0}₫ để sử dụng voucher này"
                });
            }

            if (!await IsVoucherAllowedForOrderTypeAsync(_context, voucher.Id, dto.OrderType))
            {
                return Ok(new { valid = false, message = "Voucher không áp dụng cho hình thức thanh toán này" });
            }

            // Scope validation
            if (!string.Equals(voucher.Scope, "All", StringComparison.OrdinalIgnoreCase))
            {
                var scopeValid = await ValidateVoucherScope(voucher, dto.ProductIds, dto.CategoryIds, dto.BrandIds);
                if (!scopeValid)
                {
                    return Ok(new { valid = false, message = "Voucher không áp dụng cho sản phẩm trong đơn hàng này" });
                }
            }

            var discountAmount = CalculateDiscount(voucher, dto.Subtotal);

            return Ok(new
            {
                valid = true,
                message = "Áp dụng voucher thành công",
                discountAmount,
                voucher = new
                {
                    voucher.Id,
                    voucher.Code,
                    voucher.DiscountType,
                    voucher.DiscountValue,
                    voucher.MinOrderValue,
                    voucher.MaxDiscountValue,
                    voucher.Description
                }
            });
        }

        /// <summary>
        /// Returns all applicable vouchers for the current cart.
        /// </summary>
        [HttpPost("applicable")]
        public async Task<IActionResult> GetApplicable([FromBody] ValidateVoucherDto dto)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var now = DateTime.Now;

            var vouchers = await _context.Vouchers
                .AsNoTracking()
                .Include(v => v.Categories)
                .Include(v => v.Brands)
                .Include(v => v.Products)
                .Where(v =>
                    v.IsActive &&
                    v.StartAt <= now &&
                    v.EndAt >= now &&
                    (!v.UsageLimit.HasValue || v.UsedCount < v.UsageLimit.Value) &&
                    dto.Subtotal >= v.MinOrderValue)
                .ToListAsync();

            var applicableVouchers = new List<object>();

            foreach (var voucher in vouchers)
            {
                // Check per-user usage limit
                var userUsageCount = await _context.OrderVouchers
                    .CountAsync(ov => ov.VoucherId == voucher.Id && ov.Order!.UserId == userId.Value);

                if (userUsageCount >= voucher.MaxUsagePerUser)
                {
                    continue;
                }

                if (!await IsVoucherAllowedForOrderTypeAsync(_context, voucher.Id, dto.OrderType))
                {
                    continue;
                }

                // Scope validation
                if (!string.Equals(voucher.Scope, "All", StringComparison.OrdinalIgnoreCase))
                {
                    var scopeValid = await ValidateVoucherScope(voucher, dto.ProductIds, dto.CategoryIds, dto.BrandIds);
                    if (!scopeValid)
                    {
                        continue;
                    }
                }

                var discountAmount = CalculateDiscount(voucher, dto.Subtotal);

                applicableVouchers.Add(new
                {
                    voucher.Id,
                    voucher.Code,
                    voucher.DiscountType,
                    voucher.DiscountValue,
                    voucher.MinOrderValue,
                    voucher.MaxDiscountValue,
                    voucher.Description,
                    voucher.EndAt,
                    voucher.Scope,
                    discountAmount
                });
            }

            // Order by discount amount descending
            var sortedVouchers = applicableVouchers
                .OrderByDescending(v => (decimal)v.GetType().GetProperty("discountAmount")!.GetValue(v, null)!)
                .ToList();

            return Ok(sortedVouchers);
        }

        public static async Task<bool> IsVoucherAllowedForOrderTypeAsync(BaseCoreDbContext context, int voucherId, string? orderType)
        {
            var normalizedOrderType = NormalizeOrderType(orderType);
            if (normalizedOrderType == null)
            {
                return true;
            }

            var connection = context.Database.GetDbConnection();
            var shouldClose = connection.State != System.Data.ConnectionState.Open;
            if (shouldClose)
            {
                await connection.OpenAsync();
            }

            try
            {
                var columnName = await FindVoucherOrderTypeColumnAsync(connection);
                if (columnName == null)
                {
                    return true;
                }

                await using var command = connection.CreateCommand();
                command.CommandText = $@"SELECT [{columnName}] FROM [dbo].[VOUCHER] WHERE [MaVoucher] = @voucherId";

                var parameter = command.CreateParameter();
                parameter.ParameterName = "@voucherId";
                parameter.Value = voucherId;
                command.Parameters.Add(parameter);

                var value = await command.ExecuteScalarAsync();
                if (value == null || value == DBNull.Value)
                {
                    return true;
                }

                return IsOrderTypeMatch(Convert.ToString(value), normalizedOrderType);
            }
            finally
            {
                if (shouldClose)
                {
                    await connection.CloseAsync();
                }
            }
        }

        private static async Task<string?> FindVoucherOrderTypeColumnAsync(DbConnection connection)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = @"
SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo'
  AND TABLE_NAME = 'VOUCHER'
  AND COLUMN_NAME IN (
      'LoaiDonHangApDung',
      'LoaiDonHang',
      'ApDungChoLoaiDonHang',
      'KieuDonHang',
      'HinhThucDonHang',
      'HinhThucThanhToan',
      'LoaiThanhToanApDung',
      'LoaiThanhToan',
      'PhuongThucThanhToan',
      'ApDungChoThanhToan',
      'ApDungLoaiDonHang'
  )";

            var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                existingColumns.Add(reader.GetString(0));
            }

            return VoucherOrderTypeColumnCandidates.FirstOrDefault(existingColumns.Contains);
        }

        private static string? NormalizeOrderType(string? orderType)
        {
            if (string.IsNullOrWhiteSpace(orderType))
            {
                return null;
            }

            var value = orderType.Trim();
            var key = NormalizeTextKey(value);

            return key switch
            {
                "fullpayment" => "FullPayment",
                "full" => "FullPayment",
                "thanhtoantoanbo" => "FullPayment",
                "thanhtoandaydu" => "FullPayment",
                "deposit" => "Deposit",
                "datcoc" => "Deposit",
                "coc" => "Deposit",
                "installment" => "Installment",
                "tragop" => "Installment",
                _ => value
            };
        }

        private static bool IsOrderTypeMatch(string? allowedOrderTypes, string orderType)
        {
            if (string.IsNullOrWhiteSpace(allowedOrderTypes))
            {
                return true;
            }

            var values = allowedOrderTypes
                .Split(new[] { ',', ';', '|', '/' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(NormalizeOrderType)
                .OfType<string>()
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return values.Count == 0 ||
                values.Contains("All") ||
                values.Contains("Any") ||
                values.Contains(orderType);
        }

        private static string NormalizeTextKey(string value)
        {
            var normalized = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);

            foreach (var character in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(character);
                if (category == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(character is 'đ' or 'Đ' ? 'd' : char.ToLowerInvariant(character));
                }
            }

            return builder.ToString();
        }

        private async Task<bool> ValidateVoucherScope(Voucher voucher, List<int>? productIds, List<int>? categoryIds, List<int>? brandIds)
        {
            if (voucher.Scope.Equals("Product", StringComparison.OrdinalIgnoreCase) && voucher.Products.Count > 0)
            {
                var voucherProductIds = voucher.Products.Select(p => p.ProductId).ToHashSet();
                return productIds != null && productIds.Any(pid => voucherProductIds.Contains(pid));
            }

            if (voucher.Scope.Equals("Category", StringComparison.OrdinalIgnoreCase) && voucher.Categories.Count > 0)
            {
                var voucherCategoryIds = voucher.Categories.Select(c => c.CategoryId).ToHashSet();
                return categoryIds != null && categoryIds.Any(cid => voucherCategoryIds.Contains(cid));
            }

            if (voucher.Scope.Equals("Brand", StringComparison.OrdinalIgnoreCase) && voucher.Brands.Count > 0)
            {
                var voucherBrandIds = voucher.Brands.Select(b => b.BrandId).ToHashSet();
                return brandIds != null && brandIds.Any(bid => voucherBrandIds.Contains(bid));
            }

            return true;
        }

        private static bool IsVoucherVisibleForContext(Voucher voucher, int? productId, int? categoryId, int? brandId)
        {
            if (voucher.Scope.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (voucher.Scope.Equals("Product", StringComparison.OrdinalIgnoreCase))
            {
                return !productId.HasValue ||
                    voucher.Products.Count == 0 ||
                    voucher.Products.Any(product => product.ProductId == productId.Value);
            }

            if (voucher.Scope.Equals("Category", StringComparison.OrdinalIgnoreCase))
            {
                return !categoryId.HasValue ||
                    voucher.Categories.Count == 0 ||
                    voucher.Categories.Any(category => category.CategoryId == categoryId.Value);
            }

            if (voucher.Scope.Equals("Brand", StringComparison.OrdinalIgnoreCase))
            {
                return !brandId.HasValue ||
                    voucher.Brands.Count == 0 ||
                    voucher.Brands.Any(brand => brand.BrandId == brandId.Value);
            }

            return false;
        }

        private static object ToVoucherListItem(Voucher voucher)
        {
            return new
            {
                voucher.Id,
                voucher.Code,
                voucher.DiscountType,
                voucher.DiscountValue,
                voucher.MinOrderValue,
                voucher.MaxDiscountValue,
                voucher.Description,
                voucher.EndAt,
                voucher.Scope
            };
        }

        public static decimal CalculateDiscount(Voucher voucher, decimal subtotal)
        {
            decimal discount;
            if (string.Equals(voucher.DiscountType, "Percent", StringComparison.OrdinalIgnoreCase))
            {
                discount = Math.Round(subtotal * voucher.DiscountValue / 100, 0, MidpointRounding.AwayFromZero);
                if (voucher.MaxDiscountValue.HasValue && discount > voucher.MaxDiscountValue.Value)
                {
                    discount = voucher.MaxDiscountValue.Value;
                }
            }
            else
            {
                discount = voucher.DiscountValue;
            }

            if (discount > subtotal)
            {
                discount = subtotal;
            }

            return discount;
        }

        private int? GetUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userId, out var parsedUserId) ? parsedUserId : null;
        }
    }

    public class ValidateVoucherDto
    {
        public string Code { get; set; } = "";
        public decimal Subtotal { get; set; }
        public List<int>? ProductIds { get; set; }
        public List<int>? CategoryIds { get; set; }
        public List<int>? BrandIds { get; set; }
        public string? OrderType { get; set; }
    }
}
