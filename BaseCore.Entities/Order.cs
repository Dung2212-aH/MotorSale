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
        // Kept for API compatibility. The current ShowroomDB schema stores shipping
        // address in DONHANG.DiaChiNhanHang only.
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
        public DateTime? CheckoutExpiresAt { get; set; }
        public DateTime? PaidSuccessfullyAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancelReason { get; set; }
        public int? CartId { get; set; }
        public string ReceivingMethod { get; set; } = "Delivery";
        public string ShippingStatus { get; set; } = "NotShipped";
        public string OrderType { get; set; } = "FullPayment";
        public decimal DepositAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public DateTime? PickupAppointmentAt { get; set; }
        public string? FulfillmentNote { get; set; }

        public User? User { get; set; }
        public Showroom? Showroom { get; set; }
        public Cart? Cart { get; set; }
        public List<OrderDetail> OrderDetails { get; set; } = new();
        public List<Payment> Payments { get; set; } = new();
        public List<InventoryHold> InventoryHolds { get; set; } = new();
    }
}
