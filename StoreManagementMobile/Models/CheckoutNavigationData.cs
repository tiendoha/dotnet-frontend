namespace StoreManagementMobile.Models;

public class CheckoutNavigationData
{
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public int? AppliedPromoId { get; set; }
}
