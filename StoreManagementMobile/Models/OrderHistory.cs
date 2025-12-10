using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StoreManagementMobile.Models;

/// <summary>
/// Lưu lịch sử đơn hàng đã đặt thành công vào SQLite
/// </summary>
public class OrderHistory
{
    [System.ComponentModel.DataAnnotations.Key]
    public int Id { get; set; }
    
    // ID từ backend API (sau khi tạo order thành công)
    public int OrderId { get; set; }
    
    public DateTime OrderDate { get; set; }
    
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    
    public string PaymentMethod { get; set; } = "cash"; // cash, card, ewallet
    
    public string Status { get; set; } = "Pending"; // Pending, Completed, Cancelled
    
    // JSON string chứa danh sách sản phẩm
    public string OrderDetailsJson { get; set; } = "[]";
    
    [NotMapped]
    public string TotalText => $"{FinalAmount:N0} đ";
    
    [NotMapped]
    public string OrderDateText => OrderDate.ToString("dd/MM/yyyy HH:mm");
}
