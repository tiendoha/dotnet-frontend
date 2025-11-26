namespace StoreManagementMobile.Models;

public class SelectablePromotion
{
    public Promotion Promo { get; set; } = null!;
    public bool IsEnabled { get; set; }
    public string DisplayText => $"{Promo.PromoCode} - {Promo.Description}";
}
