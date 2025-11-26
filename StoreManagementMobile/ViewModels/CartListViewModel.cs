using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StoreManagementMobile.Models;
using StoreManagementMobile.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.Json;

namespace StoreManagementMobile.ViewModels;

public partial class CartListViewModel : ObservableObject
{
    private readonly ICartService _cartService;
    private readonly IStoreApi _api;

    public ObservableCollection<CartItem> Items { get; set; } = new();
    public ObservableCollection<SelectablePromotion> PromoList { get; set; } = new();

    // =============== MONEY ===============
    [ObservableProperty]
    private decimal subtotal;

    public string SubtotalText => $"{Subtotal:N0} đ";

    [ObservableProperty]
    private decimal discount;
    
    partial void OnDiscountChanged(decimal value)
    {
        OnPropertyChanged(nameof(DiscountText));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(TotalText));
    }


    public string DiscountText => $"-{Discount:N0} đ";

    public decimal Total => Subtotal - Discount;
    public string TotalText => $"{Total:N0} đ";

    // ⭐ Selected promotion must be SelectablePromotion
    [ObservableProperty]
    private SelectablePromotion? selectedPromo;

    partial void OnSelectedPromoChanged(SelectablePromotion value)
    {

        if (value == null)
        {
            Discount = 0;
            return;
        }
    
        if (!value.IsEnabled)
        {
            // Không cho chọn → chuyển sang mã hợp lệ đầu tiên
            SelectedPromo = PromoList.FirstOrDefault(p => p.IsEnabled);
            return;
        }
    Debug.WriteLine("value OnSelectedPromoChanged: "+value);
    Debug.WriteLine("discount= " + discount);
        _ = ApplyPromo();
        
    }


    public CartListViewModel(ICartService cartService, IStoreApi api)
    {
        _cartService = cartService;
        _api = api;
    }

    // ================= LOAD =================

    public async Task LoadItems()
    {
        var items = await _cartService.GetItemsAsync();
        Items.Clear();
        foreach (var i in items) Items.Add(i);

        Subtotal = items.Sum(x => x.Price * x.Quantity);

        OnPropertyChanged(nameof(SubtotalText));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(TotalText));
    }

    public async Task LoadPromotions()
    {
        try
        {
            var response = await _api.GetPromotions();
            PromoList.Clear();

            // ⭐ Không áp dụng mã
            PromoList.Add(new SelectablePromotion
            {
                Promo = new Promotion { PromoCode = "Không áp dụng mã", DiscountValue = 0 },
                IsEnabled = true
            });

            foreach (var p in response.Data.Items)
            {
                bool valid = Subtotal >= p.MinOrderAmount;

                PromoList.Add(new SelectablePromotion
                {
                    Promo = p,
                    IsEnabled = valid
                });
            }

            // Nếu mã hiện tại không hợp lệ → reset
            if (SelectedPromo != null && !SelectedPromo.IsEnabled)
            {
                SelectedPromo = PromoList.FirstOrDefault(x => x.IsEnabled);
            }
        }
        catch
        {
            PromoList.Clear();
        }
    }

    // =============== COMMANDS ===============

    [RelayCommand]
    public async Task IncreaseQuantity(CartItem item)
    {
        await _cartService.UpdateQuantityAsync(item.ProductId, item.Quantity + 1);
        await LoadItems();
        await LoadPromotions();
        await ApplyPromo();
    }

    [RelayCommand]
    public async Task DecreaseQuantity(CartItem item)
    {
        if (item.Quantity > 1)
            await _cartService.UpdateQuantityAsync(item.ProductId, item.Quantity - 1);
        else
            await _cartService.RemoveItemAsync(item.ProductId);

        await LoadItems();
        await LoadPromotions();
        await ApplyPromo();
    }

    [RelayCommand]
    public async Task RemoveItem(CartItem item)
    {
        await _cartService.RemoveItemAsync(item.ProductId);
        await LoadItems();
        await LoadPromotions();
        await ApplyPromo();
    }

    [RelayCommand]
    public async Task UpdateQuantity(CartItem item)
    {
        if (item.Quantity <= 0) item.Quantity = 1;

        await _cartService.UpdateQuantityAsync(item.ProductId, item.Quantity);
        await LoadItems();
        await LoadPromotions();
        await ApplyPromo();
    }

    // ⭐ TÍNH GIẢM GIÁ
    [RelayCommand]
    public async Task ApplyPromo()
    {
        Debug.WriteLine("▶ ApplyPromo() chạy… SelectedPromo = " + SelectedPromo?.Promo?.PromoCode);
    
        if (SelectedPromo == null || SelectedPromo.Promo?.PromoCode == "Không áp dụng mã")
        {
            Discount = 0;
            return; // Notify sẽ chạy nhờ OnDiscountChanged()
        }
    
        var promo = SelectedPromo.Promo;
    
        if (Subtotal < promo.MinOrderAmount)
            Discount = 0;
        else if (promo.DiscountType.ToLower() == "percent")
            Discount = (Subtotal * promo.DiscountValue) / 100m;
        else
            Discount = promo.DiscountValue;
    
        // Không vượt tổng
        Discount = Math.Min(Discount, Subtotal);
    }

}
