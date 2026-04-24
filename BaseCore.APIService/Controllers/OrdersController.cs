using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BaseCore.Entities;
using BaseCore.Repository.EFCore;
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
        private readonly IOrderRepositoryEF _orderRepository;
        private readonly IOrderDetailRepositoryEF _orderDetailRepository;
        private readonly IProductRepositoryEF _productRepository;

        public OrdersController(
            IOrderRepositoryEF orderRepository,
            IOrderDetailRepositoryEF orderDetailRepository,
            IProductRepositoryEF productRepository)
        {
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _productRepository = productRepository;
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
            return Ok(new { order, details });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            if (dto.Items.Count == 0)
            {
                return BadRequest(new { message = "Order must contain at least one item" });
            }

            decimal subtotal = 0;
            var orderDetails = new List<OrderDetail>();

            foreach (var item in dto.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null || !product.IsActive || product.Status is "Hidden" or "Sold")
                {
                    return BadRequest(new { message = $"Product {item.ProductId} is not available" });
                }

                if (product.StockQuantity < item.Quantity)
                {
                    return BadRequest(new { message = $"Insufficient stock for {product.Name}" });
                }

                var unitPrice = product.SalePrice ?? product.BasePrice;
                var lineTotal = unitPrice * item.Quantity;
                subtotal += lineTotal;

                orderDetails.Add(new OrderDetail
                {
                    ProductId = item.ProductId,
                    ProductVariantId = item.ProductVariantId,
                    ProductNameSnapshot = product.Name,
                    SkuSnapshot = product.ProductCode,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    LineTotal = lineTotal
                });

                product.StockQuantity -= item.Quantity;
                if (product.ProductType == "Car" && product.StockQuantity == 0)
                {
                    product.Status = "Sold";
                }

                product.UpdatedAt = DateTime.UtcNow;
                await _productRepository.UpdateAsync(product);
            }

            var totalAmount = subtotal - dto.DiscountAmount + dto.ShippingFee;
            if (totalAmount < 0)
            {
                totalAmount = 0;
            }

            var order = new Order
            {
                OrderCode = string.IsNullOrWhiteSpace(dto.OrderCode)
                    ? $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}"
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
                DiscountAmount = dto.DiscountAmount,
                ShippingFee = dto.ShippingFee,
                TotalAmount = totalAmount,
                OrderStatus = "Pending",
                PaymentStatus = "Unpaid",
                Note = dto.Note,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _orderRepository.AddAsync(order);

            foreach (var detail in orderDetails)
            {
                detail.OrderId = order.Id;
                await _orderDetailRepository.AddAsync(detail);
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
        public async Task<IActionResult> CancelOrder(int id)
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

            var details = await _orderDetailRepository.GetByOrderAsync(id);
            foreach (var detail in details)
            {
                var product = await _productRepository.GetByIdAsync(detail.ProductId);
                if (product == null)
                {
                    continue;
                }

                product.StockQuantity += detail.Quantity;
                if (product.Status == "Sold")
                {
                    product.Status = "Available";
                }

                product.UpdatedAt = DateTime.UtcNow;
                await _productRepository.UpdateAsync(product);
            }

            order.OrderStatus = "Cancelled";
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);

            return Ok(new { message = "Order cancelled successfully", order });
        }

        private int? GetUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userId, out var parsedUserId) ? parsedUserId : null;
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
        public decimal ShippingFee { get; set; }
        public string? Note { get; set; }
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
}
