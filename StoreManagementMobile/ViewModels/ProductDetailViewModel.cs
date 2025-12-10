using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StoreManagementMobile.Models;
using StoreManagementMobile.Services;
using System.Threading.Tasks;

namespace StoreManagementMobile.ViewModels;

public partial class ProductDetailViewModel : ObservableObject
{
    private readonly ICartService _cartService;

    // Properties cho binding với XAML
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public string ProductBarcode { get; set; } = "";
    public decimal ProductPrice { get; set; }
    public string ProductPriceFormatted => $"{ProductPrice:N0} ₫";
    public string ProductUnit { get; set; } = "";
    public string ProductImageUrl { get; set; } = "";
    public string ProductCategoryName { get; set; } = "";
    public int CategoryId { get; set; }

    [ObservableProperty]
    private int quantity = 1;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public ProductDetailViewModel(ICartService cartService)
    {
        _cartService = cartService;
    }
    
    // Constructor nhận ProductResponse để dễ khởi tạo từ navigation
    public ProductDetailViewModel(ProductResponse product)
    {
        // Lấy CartService từ DI container
        var app = App.Current as App;
        _cartService = app!.Host.Services.GetRequiredService<ICartService>();
        
        // Map dữ liệu từ ProductResponse
        ProductId = product.ProductId;
        ProductName = product.ProductName ?? "";
        ProductBarcode = product.Barcode ?? "";
        ProductPrice = product.Price;
        ProductUnit = product.Unit ?? "";
        ProductImageUrl = product.ImageUrl ?? "";
        ProductCategoryName = product.CategoryName;
        CategoryId = product.CategoryId;
        Quantity = 1;
    }

    [RelayCommand]
    public void Increase()
    {
        Quantity++;
    }

    [RelayCommand]
    public void Decrease()
    {
        if (Quantity > 1)
            Quantity--;
    }

    [RelayCommand]
    public async Task AddToCart()
    {
        // Tạo Product object từ dữ liệu hiện tại
        var product = new Product
        {
            ProductId = this.ProductId,
            ProductName = this.ProductName,
            ImagePath = this.ProductImageUrl,
            Price = this.ProductPrice
        };

        await _cartService.AddItemAsync(product, Quantity);
        StatusMessage = "Đã thêm vào giỏ hàng";
    }
}
