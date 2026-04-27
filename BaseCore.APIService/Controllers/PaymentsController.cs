using BaseCore.Entities;
using BaseCore.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.APIService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private static readonly HashSet<string> AllowedPaymentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Full",
            "Deposit",
            "Remaining",
            "Installment"
        };

        private static readonly HashSet<string> AllowedPaymentMethods = new(StringComparer.OrdinalIgnoreCase)
        {
            "COD",
            "BankTransfer",
            "Card",
            "Momo",
            "VNPay"
        };

        private readonly BaseCoreDbContext _context;

        public PaymentsController(BaseCoreDbContext context)
        {
            _context = context;
        }

        [HttpGet("order/{orderId:int}")]
        public async Task<IActionResult> GetByOrder(int orderId)
        {
            var payments = await _context.Payments
                .Where(p => p.OrderId == orderId)
                .Include(p => p.Refunds)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Ok(payments);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePaymentRequest request)
        {
            if (!AllowedPaymentTypes.Contains(request.PaymentType))
            {
                return BadRequest(new { message = "PaymentType must be Full, Deposit, Remaining, or Installment" });
            }

            if (!AllowedPaymentMethods.Contains(request.PaymentMethod))
            {
                return BadRequest(new { message = "PaymentMethod must be COD, BankTransfer, Card, Momo, or VNPay" });
            }

            if (request.Amount <= 0)
            {
                return BadRequest(new { message = "Amount must be greater than zero" });
            }

            if (string.Equals(request.PaymentType, "Installment", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var payment = await CreateInstallmentPaymentAsync(request);
                    return Ok(payment);
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }

            var payments = await _context.Payments
                .FromSqlInterpolated($@"EXEC dbo.sp_ThanhToan_TaoGiaoDich
                    @MaDonHang={request.OrderId},
                    @LoaiThanhToan={NormalizePaymentType(request.PaymentType)},
                    @SoTien={request.Amount},
                    @PhuongThuc={NormalizePaymentMethod(request.PaymentMethod)},
                    @MaGiaoDich={request.TransactionRef},
                    @NoiDungChuyenKhoan={request.TransferContent},
                    @MaNganHang={request.BankCode},
                    @ResponseRaw={request.RawResponse}")
                .ToListAsync();

            return Ok(payments.FirstOrDefault());
        }

        [HttpPost("{paymentId:int}/confirm-success")]
        public async Task<IActionResult> ConfirmSuccess(int paymentId, [FromBody] ConfirmPaymentRequest request)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == paymentId);
            if (payment == null)
            {
                return NotFound(new { message = "Payment not found", paymentId });
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == payment.OrderId);

            if (order == null)
            {
                return NotFound(new { message = "Order not found", orderId = payment.OrderId });
            }

            if (RequiresStockConfirmation(payment, order))
            {
                var holdError = await ValidateActiveInventoryHoldsAsync(order.Id);
                if (holdError != null)
                {
                    return BadRequest(new { message = holdError });
                }
            }

            if (payment.PaymentStatus == "Pending")
            {
                await _context.Database.ExecuteSqlInterpolatedAsync($@"EXEC dbo.sp_ThanhToan_XacNhanThanhCong
                    @MaThanhToan={paymentId},
                    @MaGiaoDich={request.TransactionRef},
                    @ResponseRaw={request.RawResponse}");

                payment = await _context.Payments.FirstAsync(p => p.Id == paymentId);
            }
            else if (payment.PaymentStatus is not ("Paid" or "PartiallyRefunded" or "Refunded"))
            {
                return BadRequest(new
                {
                    message = $"Payment cannot be confirmed from status {payment.PaymentStatus}",
                    paymentId
                });
            }

            if (RequiresStockConfirmation(payment, order))
            {
                var confirmError = await ConfirmOrderAndDeductStockAsync(order.Id);
                if (confirmError != null)
                {
                    return BadRequest(new { message = confirmError });
                }
            }

            await transaction.CommitAsync();
            return Ok(new { message = "Payment confirmed", paymentId });
        }

        [HttpPost("{paymentId:int}/cancel")]
        public async Task<IActionResult> Cancel(int paymentId, [FromBody] CancelPaymentRequest request)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"EXEC dbo.sp_ThanhToan_HuyGiaoDich
                @MaThanhToan={paymentId},
                @LyDoHuy={request.Reason}");

            return Ok(new { message = "Payment cancelled", paymentId });
        }

        [HttpPost("{paymentId:int}/refund")]
        public async Task<IActionResult> Refund(int paymentId, [FromBody] RefundPaymentRequest request)
        {
            if (request.Amount <= 0)
            {
                return BadRequest(new { message = "Refund amount must be greater than zero" });
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == paymentId);
            if (payment == null)
            {
                return NotFound(new { message = "Payment not found", paymentId });
            }

            if (payment.PaymentStatus is not ("Paid" or "PartiallyRefunded"))
            {
                return BadRequest(new { message = "Only paid payments can be refunded" });
            }

            var totalRefunded = payment.RefundedAmount + request.Amount;
            if (totalRefunded > payment.Amount)
            {
                return BadRequest(new { message = "Refund amount exceeds paid amount" });
            }

            payment.RefundedAmount = totalRefunded;
            payment.PaymentStatus = totalRefunded == payment.Amount ? "Refunded" : "PartiallyRefunded";
            payment.RawResponse = request.RawResponse ?? payment.RawResponse;

            _context.PaymentRefunds.Add(new BaseCore.Entities.PaymentRefund
            {
                PaymentId = payment.Id,
                OrderId = payment.OrderId,
                Amount = request.Amount,
                RefundTransactionRef = request.RefundTransactionRef,
                Reason = request.Reason,
                Status = "Succeeded",
                RawResponse = request.RawResponse,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            await _context.Database.ExecuteSqlInterpolatedAsync($"EXEC dbo.sp_DonHang_DongBoTrangThaiThanhToan @MaDonHang={payment.OrderId}");
            await transaction.CommitAsync();

            return Ok(new { message = "Payment refunded", paymentId });
        }

        private async Task<Payment> CreateInstallmentPaymentAsync(CreatePaymentRequest request)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId);
            if (order == null)
            {
                throw new InvalidOperationException("Order not found");
            }

            if (order.OrderType != "Installment")
            {
                throw new InvalidOperationException("Installment payment can only be created for installment orders");
            }

            var totalReceived = await GetTotalReceivedAsync(order.Id);
            var remaining = order.TotalAmount - totalReceived;
            if (remaining <= 0)
            {
                throw new InvalidOperationException("Order is already fully paid");
            }

            if (request.Amount > remaining)
            {
                throw new InvalidOperationException("Payment amount exceeds remaining amount");
            }

            var payment = new Payment
            {
                PaymentCode = $"PAY-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}",
                OrderId = request.OrderId,
                Amount = request.Amount,
                PaymentMethod = NormalizePaymentMethod(request.PaymentMethod),
                PaymentStatus = "Pending",
                TransactionRef = request.TransactionRef,
                CreatedAt = DateTime.UtcNow,
                PaymentType = "Installment",
                TransferContent = request.TransferContent,
                BankCode = request.BankCode,
                RawResponse = request.RawResponse
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        private async Task<string?> ValidateActiveInventoryHoldsAsync(int orderId)
        {
            var now = DateTime.Now;
            var hasActiveHold = await _context.InventoryHolds.AnyAsync(hold =>
                hold.OrderId == orderId &&
                hold.Status == "Active" &&
                hold.ExpiresAt > now);

            return hasActiveHold
                ? null
                : "Order does not have an active inventory hold. Please checkout again.";
        }

        private async Task<string?> ConfirmOrderAndDeductStockAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return "Order not found";
            }

            if (order.OrderStatus is "Confirmed" or "Completed")
            {
                return null;
            }

            var totalReceived = await GetTotalReceivedAsync(orderId);
            if (order.OrderType == "FullPayment" && totalReceived < order.TotalAmount)
            {
                return "Full payment order has not collected enough money";
            }

            if (order.OrderType is "Deposit" or "Installment" && totalReceived < order.DepositAmount)
            {
                return "Order has not collected enough deposit/down payment";
            }

            var now = DateTime.Now;
            var holds = await _context.InventoryHolds
                .Where(hold => hold.OrderId == orderId && hold.Status == "Active" && hold.ExpiresAt > now)
                .ToListAsync();

            if (holds.Count == 0)
            {
                return "Order does not have an active inventory hold";
            }

            foreach (var variantGroup in holds.Where(h => h.ProductVariantId.HasValue).GroupBy(h => h.ProductVariantId!.Value))
            {
                var variant = await _context.ProductVariants.FirstOrDefaultAsync(v => v.Id == variantGroup.Key);
                if (variant == null)
                {
                    return $"Variant {variantGroup.Key} not found";
                }

                var quantity = variantGroup.Sum(h => h.Quantity);
                var currentStock = variant.StockQuantity ?? 0;
                if (currentStock < quantity)
                {
                    return $"Insufficient stock for variant {variant.Sku}";
                }

                variant.StockQuantity = currentStock - quantity;
                variant.UpdatedAt = DateTime.UtcNow;
            }

            foreach (var productGroup in holds.Where(h => !h.ProductVariantId.HasValue).GroupBy(h => h.ProductId))
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productGroup.Key);
                if (product == null)
                {
                    return $"Product {productGroup.Key} not found";
                }

                var quantity = productGroup.Sum(h => h.Quantity);
                if (product.StockQuantity < quantity)
                {
                    return $"Insufficient stock for product {product.Name}";
                }

                product.StockQuantity -= quantity;
                product.UpdatedAt = DateTime.UtcNow;
            }

            foreach (var hold in holds)
            {
                hold.Status = "Confirmed";
                hold.UpdatedAt = DateTime.Now;
                hold.Note = string.IsNullOrWhiteSpace(hold.Note)
                    ? "Payment confirmed and stock deducted"
                    : hold.Note + " | Payment confirmed and stock deducted";
            }

            order.OrderStatus = "Confirmed";
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _context.Database.ExecuteSqlInterpolatedAsync($"EXEC dbo.sp_DonHang_DongBoTrangThaiThanhToan @MaDonHang={orderId}");
            return null;
        }

        private async Task<decimal> GetTotalReceivedAsync(int orderId)
        {
            return await _context.Payments
                .Where(payment =>
                    payment.OrderId == orderId &&
                    (payment.PaymentStatus == "Paid" ||
                     payment.PaymentStatus == "PartiallyRefunded" ||
                     payment.PaymentStatus == "Refunded"))
                .SumAsync(payment => (decimal?)(payment.Amount - payment.RefundedAmount)) ?? 0;
        }

        private static bool RequiresStockConfirmation(Payment payment, BaseCore.Entities.Order order)
        {
            return payment.PaymentType != "Remaining" && order.OrderStatus is not ("Confirmed" or "Completed");
        }

        private static string NormalizePaymentType(string paymentType)
        {
            return AllowedPaymentTypes.First(type => string.Equals(type, paymentType, StringComparison.OrdinalIgnoreCase));
        }

        private static string NormalizePaymentMethod(string paymentMethod)
        {
            return AllowedPaymentMethods.First(method => string.Equals(method, paymentMethod, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class CreatePaymentRequest
    {
        public int OrderId { get; set; }
        public string PaymentType { get; set; } = "Full";
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "COD";
        public string? TransactionRef { get; set; }
        public string? TransferContent { get; set; }
        public string? BankCode { get; set; }
        public string? RawResponse { get; set; }
    }

    public class ConfirmPaymentRequest
    {
        public string? TransactionRef { get; set; }
        public string? RawResponse { get; set; }
    }

    public class CancelPaymentRequest
    {
        public string? Reason { get; set; }
    }

    public class RefundPaymentRequest
    {
        public decimal Amount { get; set; }
        public string? Reason { get; set; }
        public string? RefundTransactionRef { get; set; }
        public string? RawResponse { get; set; }
    }
}
