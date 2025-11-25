using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StoreManagementMobile.Models;
using StoreManagementMobile.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace StoreManagementMobile.ViewModels;

public partial class CartListViewModel : ObservableObject
{
    private readonly ICartService _cartService;

    public ObservableCollection<CartItem> Items { get; set; } = new();

    // ---- MONEY ----
    [ObservableProperty]
    private decimal subtotal;

    public string SubtotalText => $"{Subtotal:N0} đ";

    [ObservableProperty]
    private decimal discount;

    public string DiscountText => $"-{Discount:N0} đ";

    public decimal Total => Subtotal - Discount;
    public string TotalText => $"{Total:N0} đ";

    // promo
    [ObservableProperty]
    private string promoCode = "";

    public CartListViewModel(ICartService cartService)
    {
        _cartService = cartService;
    }

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

    // ---- COMMANDS ----

    [RelayCommand]
    public async Task IncreaseQuantity(CartItem item)
    {
        await _cartService.UpdateQuantityAsync(item.Id, item.Quantity + 1);
        await LoadItems();
    }

    [RelayCommand]
    public async Task DecreaseQuantity(CartItem item)
    {
        if (item.Quantity <= 1)
            await _cartService.RemoveItemAsync(item.Id);
        else
            await _cartService.UpdateQuantityAsync(item.Id, item.Quantity - 1);

        await LoadItems();
    }

    [RelayCommand]
    public async Task RemoveItem(CartItem item)
    {
        await _cartService.RemoveItemAsync(item.Id);
        await LoadItems();
    }

    [RelayCommand]
    public async Task ApplyPromo()
    {
        if (PromoCode == "SALE10")
            Discount = (decimal)(Subtotal * 0.1m);
        else
            Discount = 0;

        OnPropertyChanged(nameof(DiscountText));
        OnPropertyChanged(nameof(TotalText));
        OnPropertyChanged(nameof(Total));
    }
}
