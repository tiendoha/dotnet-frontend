using System.Collections.Generic;

namespace StoreManagementMobile.DTOs;

public class OrderDetailDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class CreateOrderRequest
{
    public int? CustomerId { get; set; } = null;
    public string CustomerName { get; set; } = "";
    public string CustomerPhone { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public string CustomerAddress { get; set; } = "";

    public List<OrderDetailDto> OrderDetails { get; set; } = new();

    public string PaymentMethod { get; set; } = "Cash";
    public decimal AmountPaid { get; set; }
    public int? PromoId { get; set; } = null;
}
