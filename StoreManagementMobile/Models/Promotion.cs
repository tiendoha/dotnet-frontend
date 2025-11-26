namespace StoreManagementMobile.Models;

public class Promotion
{
    public int PromoId { get; set; }
    public string PromoCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal MinOrderAmount { get; set; }

    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
    public bool IsUsageLimitReached { get; set; }
}
