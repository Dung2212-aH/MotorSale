using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BaseCore.Entities;
using BaseCore.Repository;
using BaseCore.Repository.EFCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BaseCore.APIService.Controllers
{
    /// <summary>
    /// Order API Controller for cars/accessories checkout.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private static readonly HashSet<string> AllowedOrderTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "FullPayment",
            "Deposit",
            "Installment"
        };

        private static readonly HashSet<string> AllowedReceivingMethods = new(StringComparer.OrdinalIgnoreCase)
        {
            "Delivery",
            "Pickup"
        };

        private static readonly HashSet<string> AllowedPaymentMethods = new(StringComparer.OrdinalIgnoreCase)
        {
            "COD",
            "BankTransfer",
            "Card",
            "Momo",
            "VNPay"
        };

        private readonly IOrderRepositoryEF _orderRepository;
        private readonly IOrderDetailRepositoryEF _orderDetailRepository;
        private readonly IProductRepositoryEF _productRepository;
        private readonly BaseCoreDbContext _context;

        public OrdersController(
            IOrderRepositoryEF orderRepository,
            IOrderDetailRepositoryEF orderDetailRepository,
            IProductRepositoryEF productRepository,
            BaseCoreDbContext context)
        {
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _productRepository = productRepository;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var orders = await _orderRepository.GetByUserAsync(userId.Value);
            return Ok(orders);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderRepository.GetAllAsync();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _orderRepository.GetWithDetailsAsync(id);
            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            var details = await _orderDetailRepository.GetByOrderAsync(id);
            var vouchers = await _context.OrderVouchers
                .Where(ov => ov.OrderId == id)
                .Select(ov => new
                {
                    ov.VoucherId,
                    ov.VoucherCodeSnapshot,
                    ov.DiscountAmount,
                    ov.DiscountTypeSnapshot,
                    ov.DiscountValueSnapshot,
                    ov.CreatedAt
                })
                .ToListAsync();
            return Ok(new { order, details, vouchers });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var basicValidation = ValidateCreateOrderDto(dto);
            if (basicValidation != null)
            {
                return basicValidation;
            }

            if (dto.Items.Count == 0)
            {
                return BadRequest(new { message = "Order must contain at least one item" });
            }

            decimal subtotal = 0;
            var orderDetails = new List<OrderDetail>();
            var requestedQuantities = new Dictionary<(int ProductId, int? VariantId), int>();
            var now = DateTime.Now;
            var productIds = new List<int>();
            var categoryIds = new List<int>();
            var brandIds = new List<int>();

            foreach (var item in dto.Items)
            {
                if (item.Quantity <= 0)
                {
                    return BadRequest(new { message = "Item quantity must be greater than zero" });
                }

                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null || !product.IsActive || product.Status is "Hidden" or "Sold")
                {
                    return BadRequest(new { message = $"Product {item.ProductId} is not available" });
                }

                if (!productIds.Contains(product.Id)) productIds.Add(product.Id);
                if (!categoryIds.Contains(product.CategoryId))
                    categoryIds.Add(product.CategoryId);
                if (product.BrandId.HasValue && !brandIds.Contains(product.BrandId.Value))
                    brandIds.Add(product.BrandId.Value);

                ProductVariant? variant = null;
                if (item.ProductVariantId.HasValue)
                {
                    variant = product.Variants.FirstOrDefault(v => v.Id == item.ProductVariantId.Value);
                    if (variant == null)
                    {
                        return BadRequest(new { message = $"Variant {item.ProductVariantId} does not belong to product {item.ProductId}" });
                    }
                }

                var key = (item.ProductId, item.ProductVariantId);
                requestedQuantities.TryGetValue(key, out var alreadyRequested);
                requestedQuantities[key] = alreadyRequested + item.Quantity;

                var physicalStock = variant != null
                    ? variant.StockQuantity ?? 0
                    : product.StockQuantity;
                var activeHeldQuantity = await _context.InventoryHolds
                    .Where(hold =>
                        hold.ProductId == item.ProductId &&
                        hold.ProductVariantId == item.ProductVariantId &&
                        hold.Status == "Active" &&
                        hold.ExpiresAt > now)
                    .SumAsync(hold => (int?)hold.Quantity) ?? 0;
                var availableStock = physicalStock - activeHeldQuantity;
                if (availableStock < requestedQuantities[key])
                {
                    return BadRequest(new { message = $"Insufficient stock for {product.Name}" });
                }

                var unitPrice = variant?.PriceOverride ?? product.SalePrice ?? product.BasePrice;
                var lineTotal = unitPrice * item.Quantity;
                subtotal += lineTotal;

                orderDetails.Add(new OrderDetail
                {
                    ProductId = item.ProductId,
                    ProductVariantId = item.ProductVariantId,
                    ProductNameSnapshot = product.Name,
                    SkuSnapshot = variant?.Sku ?? product.ProductCode,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    LineTotal = lineTotal
                });
            }

            // Voucher processing
            Voucher? appliedVoucher = null;
            decimal voucherDiscount = dto.DiscountAmount;
            if (!string.IsNullOrWhiteSpace(dto.VoucherCode))
            {
                appliedVoucher = await _context.Vouchers
                    .Include(v => v.Categories)
                    .Include(v => v.Brands)
                    .Include(v => v.Products)
                    .FirstOrDefaultAsync(v => v.Code == dto.VoucherCode.Trim());

                if (appliedVoucher == null)
                {
                    return BadRequest(new { message = "Mã voucher không tồn tại" });
                }

                if (!appliedVoucher.IsActive)
                {
                    return BadRequest(new { message = "Voucher đã ngưng hoạt động" });
                }

                var currentTime = DateTime.Now;
                if (currentTime < appliedVoucher.StartAt || currentTime > appliedVoucher.EndAt)
                {
                    return BadRequest(new { message = "Voucher đã hết hạn hoặc chưa đến thời gian sử dụng" });
                }

                if (appliedVoucher.UsageLimit.HasValue && appliedVoucher.UsedCount >= appliedVoucher.UsageLimit.Value)
                {
                    return BadRequest(new { message = "Voucher đã hết lượt sử dụng" });
                }

                var userUsageCount = await _context.OrderVouchers
                    .CountAsync(ov => ov.VoucherId == appliedVoucher.Id && ov.Order!.UserId == userId.Value);
                if (userUsageCount >= appliedVoucher.MaxUsagePerUser)
                {
                    return BadRequest(new { message = "Bạn đã sử dụng hết lượt cho voucher này" });
                }

                if (subtotal < appliedVoucher.MinOrderValue)
                {
                    return BadRequest(new { message = $"Đơn hàng tối thiểu {appliedVoucher.MinOrderValue:N0}₫ để sử dụng voucher" });
                }

                if (!await VouchersController.IsVoucherAllowedForOrderTypeAsync(_context, appliedVoucher.Id, dto.OrderType))
                {
                    return BadRequest(new { message = "Voucher không áp dụng cho hình thức thanh toán này" });
                }

                voucherDiscount = VouchersController.CalculateDiscount(appliedVoucher, subtotal);
            }

            var totalAmount = subtotal - voucherDiscount + dto.ShippingFee;
            if (totalAmount < 0)
            {
                totalAmount = 0;
            }

            var orderAmounts = ResolveOrderAmounts(dto.OrderType, dto.DepositAmount, totalAmount);
            if (orderAmounts.ErrorMessage != null)
            {
                return BadRequest(new { message = orderAmounts.ErrorMessage });
            }

            var order = new Order
            {
                OrderCode = string.IsNullOrWhiteSpace(dto.OrderCode)
                    ? $"ORD-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}"
                    : dto.OrderCode,
                UserId = userId.Value,
                ShowroomId = dto.ShowroomId,
                ShippingFullName = dto.ShippingFullName,
                ShippingPhoneNumber = dto.ShippingPhoneNumber,
                ShippingEmail = dto.ShippingEmail,
                ShippingAddressLine = dto.ShippingAddressLine,
                ShippingWard = dto.ShippingWard,
                ShippingDistrict = dto.ShippingDistrict,
                ShippingProvince = dto.ShippingProvince,
                Subtotal = subtotal,
                DiscountAmount = voucherDiscount,
                ShippingFee = dto.ShippingFee,
                TotalAmount = totalAmount,
                OrderStatus = "Pending",
                PaymentStatus = "Unpaid",
                Note = dto.Note,
                CartId = dto.CartId,
                ReceivingMethod = NormalizeReceivingMethod(dto.ReceivingMethod),
                ShippingStatus = "NotShipped",
                OrderType = NormalizeOrderType(dto.OrderType),
                DepositAmount = orderAmounts.DepositAmount,
                RemainingAmount = orderAmounts.RemainingAmount,
                PickupAppointmentAt = dto.PickupAppointmentAt,
                FulfillmentNote = dto.FulfillmentNote,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (order.OrderType == "Installment")
            {
                var installmentValidation = ValidateInstallmentDto(dto, order);
                if (installmentValidation != null)
                {
                    return installmentValidation;
                }
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var detail in orderDetails)
                {
                    detail.OrderId = order.Id;
                    _context.OrderDetails.Add(detail);
                }

                await _context.SaveChangesAsync();

                var holdExpiresAt = DateTime.Now.AddMinutes(dto.HoldMinutes <= 0 ? 15 : dto.HoldMinutes);
                foreach (var detail in orderDetails)
                {
                    _context.InventoryHolds.Add(new InventoryHold
                    {
                        OrderId = order.Id,
                        OrderDetailId = detail.Id,
                        ProductId = detail.ProductId,
                        ProductVariantId = detail.ProductVariantId,
                        Quantity = detail.Quantity,
                        Status = "Active",
                        ExpiresAt = holdExpiresAt,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        Note = "Hold inventory when order is created"
                    });
                }

                var paymentAmount = ResolveInitialPaymentAmount(order);
                if (paymentAmount > 0)
                {
                    _context.Payments.Add(new Payment
                    {
                        PaymentCode = $"PAY-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}",
                        OrderId = order.Id,
                        Amount = paymentAmount,
                        PaymentMethod = NormalizePaymentMethod(dto.PaymentMethod),
                        PaymentStatus = "Pending",
                        PaymentType = ResolveInitialPaymentType(order.OrderType),
                        CreatedAt = DateTime.UtcNow
                    });
                }

                if (order.OrderType == "Installment")
                {
                    _context.InstallmentPlans.Add(new InstallmentPlan
                    {
                        OrderId = order.Id,
                        DownPaymentAmount = order.DepositAmount,
                        FinancedAmount = order.RemainingAmount,
                        Months = dto.InstallmentMonths!.Value,
                        MonthlyInterestRate = dto.MonthlyInterestRate ?? 0,
                        MonthlyPaymentAmount = dto.MonthlyPaymentAmount!.Value,
                        PaidPeriods = 0,
                        BuyerFullName = dto.InstallmentBuyerFullName,
                        PhoneNumber = dto.InstallmentPhoneNumber,
                        CitizenId = dto.InstallmentCitizenId,
                        Address = dto.InstallmentAddress,
                        FinanceCompany = dto.FinanceCompany,
                        StartDate = dto.InstallmentStartDate,
                        EndDate = dto.InstallmentEndDate,
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Note = dto.InstallmentNote
                    });
                }

                // Record voucher usage
                if (appliedVoucher != null)
                {
                    _context.OrderVouchers.Add(new OrderVoucher
                    {
                        OrderId = order.Id,
                        VoucherId = appliedVoucher.Id,
                        VoucherCodeSnapshot = appliedVoucher.Code,
                        DiscountAmount = voucherDiscount,
                        DiscountTypeSnapshot = appliedVoucher.DiscountType,
                        DiscountValueSnapshot = appliedVoucher.DiscountValue,
                        CreatedAt = DateTime.UtcNow
                    });

                    appliedVoucher.UsedCount += 1;
                    appliedVoucher.UpdatedAt = DateTime.UtcNow;
                }

                if (dto.CartId.HasValue)
                {
                    var cart = await _context.Carts.FirstOrDefaultAsync(c => c.Id == dto.CartId.Value && c.UserId == userId.Value);
                    if (cart != null)
                    {
                        cart.Status = "CheckedOut";
                        cart.UpdatedAt = DateTime.UtcNow;
                    }
                }

                order.OrderStatus = "AwaitingPayment";
                order.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message });
            }

            return CreatedAtAction(nameof(GetById), new { id = order.Id }, new { order, details = orderDetails });
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            order.OrderStatus = dto.OrderStatus ?? order.OrderStatus;
            order.PaymentStatus = dto.PaymentStatus ?? order.PaymentStatus;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);

            return Ok(order);
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id, [FromBody] CancelOrderDto? dto)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            if (order.OrderStatus == "Completed")
            {
                return BadRequest(new { message = "Cannot cancel completed order" });
            }

            var cancelReason = dto?.Reason;
            await _context.Database.ExecuteSqlInterpolatedAsync($@"EXEC dbo.sp_DonHang_HuyVaNhaGiuCho
                @MaDonHang={id},
                @LyDoHuyDon={cancelReason}");

            order = await _orderRepository.GetByIdAsync(id);

            return Ok(new { message = "Order cancelled successfully", order });
        }

        private int? GetUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userId, out var parsedUserId) ? parsedUserId : null;
        }

        private IActionResult? ValidateCreateOrderDto(CreateOrderDto dto)
        {
            if (!AllowedOrderTypes.Contains(dto.OrderType))
            {
                return BadRequest(new { message = "OrderType must be FullPayment, Deposit, or Installment" });
            }

            if (!AllowedReceivingMethods.Contains(dto.ReceivingMethod))
            {
                return BadRequest(new { message = "ReceivingMethod must be Delivery or Pickup" });
            }

            if (!AllowedPaymentMethods.Contains(dto.PaymentMethod))
            {
                return BadRequest(new { message = "PaymentMethod must be COD, BankTransfer, Card, Momo, or VNPay" });
            }

            if (dto.DiscountAmount < 0 || dto.ShippingFee < 0)
            {
                return BadRequest(new { message = "DiscountAmount and ShippingFee must be non-negative" });
            }

            return null;
        }

        private IActionResult? ValidateInstallmentDto(CreateOrderDto dto, Order order)
        {
            if (dto.InstallmentMonths is not (3 or 6 or 9 or 12 or 18 or 24 or 36))
            {
                return BadRequest(new { message = "InstallmentMonths must be one of 3, 6, 9, 12, 18, 24, or 36" });
            }

            if (dto.MonthlyPaymentAmount == null || dto.MonthlyPaymentAmount <= 0)
            {
                return BadRequest(new { message = "MonthlyPaymentAmount is required for installment orders" });
            }

            if (order.RemainingAmount <= 0)
            {
                return BadRequest(new { message = "Installment order must have a remaining financed amount greater than zero" });
            }

            return null;
        }

        private static (decimal DepositAmount, decimal RemainingAmount, string? ErrorMessage) ResolveOrderAmounts(
            string orderType,
            decimal requestedDeposit,
            decimal totalAmount)
        {
            var normalizedOrderType = NormalizeOrderType(orderType);
            if (normalizedOrderType == "FullPayment")
            {
                return (0, 0, null);
            }

            if (normalizedOrderType == "Deposit")
            {
                if (requestedDeposit <= 0 || requestedDeposit >= totalAmount)
                {
                    return (0, 0, "Deposit amount must be greater than 0 and less than total amount");
                }

                return (requestedDeposit, totalAmount - requestedDeposit, null);
            }

            if (requestedDeposit < 0 || requestedDeposit >= totalAmount)
            {
                return (0, 0, "Installment down payment must be non-negative and less than total amount");
            }

            return (requestedDeposit, totalAmount - requestedDeposit, null);
        }

        private static decimal ResolveInitialPaymentAmount(Order order)
        {
            return order.OrderType switch
            {
                "Deposit" => order.DepositAmount,
                "Installment" => order.DepositAmount,
                _ => order.TotalAmount
            };
        }

        private static string ResolveInitialPaymentType(string orderType)
        {
            return orderType switch
            {
                "Deposit" => "Deposit",
                "Installment" => "Installment",
                _ => "Full"
            };
        }

        private static string NormalizeOrderType(string orderType)
        {
            return AllowedOrderTypes.First(type => string.Equals(type, orderType, StringComparison.OrdinalIgnoreCase));
        }

        private static string NormalizeReceivingMethod(string receivingMethod)
        {
            return AllowedReceivingMethods.First(method => string.Equals(method, receivingMethod, StringComparison.OrdinalIgnoreCase));
        }

        private static string NormalizePaymentMethod(string paymentMethod)
        {
            return AllowedPaymentMethods.First(method => string.Equals(method, paymentMethod, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class CreateOrderDto
    {
        public string? OrderCode { get; set; }
        public int? ShowroomId { get; set; }
        public string ShippingFullName { get; set; } = "";
        public string ShippingPhoneNumber { get; set; } = "";
        public string? ShippingEmail { get; set; }
        public string ShippingAddressLine { get; set; } = "";
        public string? ShippingWard { get; set; }
        public string? ShippingDistrict { get; set; }
        public string ShippingProvince { get; set; } = "";
        public decimal DiscountAmount { get; set; }
        public string? VoucherCode { get; set; }
        public decimal ShippingFee { get; set; }
        public string? Note { get; set; }
        public int? CartId { get; set; }
        public string ReceivingMethod { get; set; } = "Delivery";
        public string OrderType { get; set; } = "FullPayment";
        public decimal DepositAmount { get; set; }
        public DateTime? PickupAppointmentAt { get; set; }
        public string? FulfillmentNote { get; set; }
        public int HoldMinutes { get; set; } = 15;
        public string PaymentMethod { get; set; } = "COD";
        public int? InstallmentMonths { get; set; }
        public decimal? MonthlyInterestRate { get; set; }
        public decimal? MonthlyPaymentAmount { get; set; }
        public string? InstallmentBuyerFullName { get; set; }
        public string? InstallmentPhoneNumber { get; set; }
        public string? InstallmentCitizenId { get; set; }
        public string? InstallmentAddress { get; set; }
        public string? FinanceCompany { get; set; }
        public DateTime? InstallmentStartDate { get; set; }
        public DateTime? InstallmentEndDate { get; set; }
        public string? InstallmentNote { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateStatusDto
    {
        public string? OrderStatus { get; set; }
        public string? PaymentStatus { get; set; }
    }

    public class CancelOrderDto
    {
        public string? Reason { get; set; }
    }
}
