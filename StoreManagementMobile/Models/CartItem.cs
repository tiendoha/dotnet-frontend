using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StoreManagementMobile.Models;

public class CartItem
{
    [System.ComponentModel.DataAnnotations.Key]   // <<< FIX: Ghi rõ namespace
    public int Id { get; set; }
    
    public int UserId { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string ImagePath { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    [NotMapped]
    public string PriceText => $"{Price:N0} đ";

    [NotMapped]
    public string TotalPriceText => $"{(Price * Quantity):N0} đ";
}
