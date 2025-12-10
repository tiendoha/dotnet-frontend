using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StoreManagementMobile.Models
{
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class CreateOrderRequest
    {
        public int? CustomerId { get; set; } = null;

        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string? CustomerEmail { get; set; } = null;
        public string CustomerAddress { get; set; } = string.Empty;

        public List<OrderItemDto> OrderDetails { get; set; } = new();

        // must match backend enum: Cash / Card / EWallet
        public string PaymentMethod { get; set; } = "cash";

        public decimal AmountPaid { get; set; }

        // nếu không có mã → null
        public int? PromoId { get; set; } = null;
    }
}
