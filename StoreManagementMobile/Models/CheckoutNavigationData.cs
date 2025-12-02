namespace StoreManagementMobile.Models;

public class CheckoutNavigationData
{
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public int? AppliedPromoId { get; set; }
    
    // Flag để biết có phải từ "Mua ngay" không
    public bool IsFromBuyNow { get; set; } = false;
}
