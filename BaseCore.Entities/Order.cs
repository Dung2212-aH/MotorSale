using System;
using System.Collections.Generic;

namespace BaseCore.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderCode { get; set; } = "";
        public int UserId { get; set; }
        public int? ShowroomId { get; set; }
        public string ShippingFullName { get; set; } = "";
        public string ShippingPhoneNumber { get; set; } = "";
        public string? ShippingEmail { get; set; }
        public string ShippingAddressLine { get; set; } = "";
        public string? ShippingWard { get; set; }
        public string? ShippingDistrict { get; set; }
        public string ShippingProvince { get; set; } = "";
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } = "Pending";
        public string PaymentStatus { get; set; } = "Unpaid";
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
        public Showroom? Showroom { get; set; }
        public List<OrderDetail> OrderDetails { get; set; } = new();
    }
}
