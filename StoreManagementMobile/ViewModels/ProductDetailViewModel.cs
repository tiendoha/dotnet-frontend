using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StoreManagementMobile.Models;
using StoreManagementMobile.Services;
using System.Threading.Tasks;

namespace StoreManagementMobile.ViewModels;

public partial class ProductDetailViewModel : ObservableObject
{
    private readonly ICartService _cartService;

    public Product? Product { get; set; }  // nullable để tránh warning

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public ProductDetailViewModel(ICartService cartService)
    {
        _cartService = cartService;
    }

    [RelayCommand]
    public async Task AddToCart(int qty = 1)
    {
        if (Product == null) return;

        await _cartService.AddItemAsync(Product, qty);
        StatusMessage = "Đã thêm vào giỏ hàng";
        // Nếu cần hiển thị alert, page có thể lắng nghe StatusMessage hoặc raise event
    }
}
