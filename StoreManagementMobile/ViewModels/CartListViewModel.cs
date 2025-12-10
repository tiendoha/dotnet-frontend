using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StoreManagementMobile.Models;
using StoreManagementMobile.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace StoreManagementMobile.ViewModels;

public partial class CartListViewModel : ObservableObject
{
    private readonly ICartService _cartService;
    private readonly IStoreApi _api;
    private bool _promotionsLoaded = false; // Cache flag

    public ObservableCollection<CartItem> Items { get; set; } = new();
    public ObservableCollection<SelectablePromotion> PromoList { get; set; } = new();
    
    [ObservableProperty]
    private bool isLoading;

    // ================= MONEY =================
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

    // Selected promotion (KHÔNG tự áp dụng)
    [ObservableProperty]
    private SelectablePromotion? selectedPromo;

    partial void OnSelectedPromoChanged(SelectablePromotion? value)
    {
        // ❌ Không áp dụng mã ngay
        // ❌ Không tính giảm giá ở đây
        // Chỉ cho phép chọn item có IsEnabled = true
        if (value is not null && !value.IsEnabled)
        {
            SelectedPromo = PromoList.FirstOrDefault(x => x.IsEnabled);
        }
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
        Debug.WriteLine("🔹 LoadPromotions() bắt đầu...");
        
        // Chỉ load 1 lần (cache)
        if (_promotionsLoaded && PromoList.Count > 0)
        {
            Debug.WriteLine("💾 Dùng cache - Không gọi API");
            // Cập nhật IsEnabled dựa trên Subtotal hiện tại
            foreach (var item in PromoList.Skip(1)) // Skip "Không áp dụng mã"
            {
                if (item.Promo != null)
                {
                    item.IsEnabled = Subtotal >= item.Promo.MinOrderAmount;
                }
            }
            return;
        }
        
        try
        {
            Debug.WriteLine("🌐 Gọi API: GET /api/Promotion");
            var response = await _api.GetPromotions();
            Debug.WriteLine($"✅ Nhận được {response?.Data?.Items?.Count ?? 0} khuyến mãi");
            PromoList.Clear();

            // Add "Không áp dụng mã"
            PromoList.Add(new SelectablePromotion
            {
                Promo = new Promotion { PromoCode = "Không áp dụng mã" },
                IsEnabled = true
            });

            foreach (var p in response.Data.Items)
            {
                PromoList.Add(new SelectablePromotion
                {
                    Promo = p,
                    IsEnabled = Subtotal >= p.MinOrderAmount
                });
            }
            
            _promotionsLoaded = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("💥 LoadPromotions error: " + ex);
            var innerMsg = ex.InnerException?.Message ?? ex.Message;
            Debug.WriteLine($"   Chi tiết: {innerMsg}");
            PromoList.Clear();
        }
    }

    // ================= COMMANDS =================

    [RelayCommand]
    public async Task IncreaseQuantity(CartItem item)
    {
        IsLoading = true;
        await _cartService.UpdateQuantityAsync(item.ProductId, item.Quantity + 1);
        await LoadItems();
        // Chỉ cập nhật IsEnabled, không reload API
        UpdatePromotionStates();
        IsLoading = false;
    }

    [RelayCommand]
    public async Task DecreaseQuantity(CartItem item)
    {
        IsLoading = true;
        if (item.Quantity > 1)
            await _cartService.UpdateQuantityAsync(item.ProductId, item.Quantity - 1);
        else
            await _cartService.RemoveItemAsync(item.ProductId);

        await LoadItems();
        UpdatePromotionStates();
        IsLoading = false;
    }

    [RelayCommand]
    public async Task RemoveItem(CartItem item)
    {
        IsLoading = true;
        await _cartService.RemoveItemAsync(item.ProductId);
        await LoadItems();
        UpdatePromotionStates();
        IsLoading = false;
    }

    [RelayCommand]
    public async Task UpdateQuantity(CartItem item)
    {
        if (item.Quantity <= 0) item.Quantity = 1;

        IsLoading = true;
        await _cartService.UpdateQuantityAsync(item.ProductId, item.Quantity);
        await LoadItems();
        UpdatePromotionStates();
        IsLoading = false;
    }
    
    private void UpdatePromotionStates()
    {
        foreach (var item in PromoList.Skip(1))
        {
            if (item.Promo != null)
            {
                item.IsEnabled = Subtotal >= item.Promo.MinOrderAmount;
            }
        }
    }

    // ================= APPLY PROMO =================
    [RelayCommand]
    public async Task ApplyPromo()
    {
        Debug.WriteLine("🔹 ApplyPromo() bắt đầu...");
        
        if (SelectedPromo == null || SelectedPromo.Promo?.PromoCode == "Không áp dụng mã")
        {
            Debug.WriteLine("❌ Không có mã được chọn");
            Discount = 0;
            return;
        }

        string code = SelectedPromo.Promo.PromoCode;
        Debug.WriteLine($"🌐 Gọi API: GET /api/Promotion/by-code/{code}");

        try
        {
            var response = await _api.GetPromotionByCode(code);
            Debug.WriteLine($"✅ API response: Success={response?.Success}, Data={response?.Data?.PromoCode}");

            if (!response.Success || response.Data == null)
            {
                Discount = 0;
                return;
            }

            var promo = response.Data;

            if (Subtotal < promo.MinOrderAmount)
            {
                Discount = 0;
                return;
            }

            if (promo.DiscountType.ToLower() == "percent")
                Discount = (Subtotal * promo.DiscountValue) / 100m;
            else
                Discount = promo.DiscountValue;

            Discount = Math.Min(Discount, Subtotal);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("💥 ApplyPromo error: " + ex);
            var innerMsg = ex.InnerException?.Message ?? ex.Message;
            Debug.WriteLine($"   Chi tiết: {innerMsg}");
            Discount = 0;
        }
    }
    
    //================= Continue Payment =================
    
    [RelayCommand]
    public async Task ContinuePayment()
    {
        try
        {
            Debug.WriteLine("➡ ContinuePaymentCommand gọi!");
    
            // Lấy NavigationWindow hiện tại
            var window = Window.Current;
            if (window == null)
            {
                Debug.WriteLine("❌ Window.Current NULL");
                return;
            }
    
            // Frame hiện tại
            var frame = window.Content as Frame;
            if (frame == null)
            {
                Debug.WriteLine("❌ Frame NULL trong ContinuePayment");
                return;
            }
    
            // Chuẩn bị dữ liệu cần truyền sang Checkout
            var checkoutData = new CheckoutNavigationData
            {
                Subtotal = this.Subtotal,
                Discount = this.Discount,
                Total = this.Total,
                AppliedPromoId = SelectedPromo?.Promo?.PromoId
            };
    
            Debug.WriteLine($"➡ Điều hướng CheckoutPage với Total = {checkoutData.Total}");
    
            frame.Navigate(typeof(CheckoutPage), checkoutData);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"💥 Lỗi trong ContinuePayment(): {ex}");
        }
    }

}
