// File: ProductDetailViewModel.cs (Ví dụ mẫu)

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StoreManagementMobile.Models;
using StoreManagementMobile.Services;
using System.Threading.Tasks;

namespace StoreManagementMobile.ViewModels;
using System.Globalization;

public partial class ProductDetailViewModel : ObservableObject
{
    private readonly ICartService _cartService;
    private readonly ProductResponse _product;

    public Product? Product { get; set; }  // nullable để tránh warning

    [ObservableProperty]
    private string statusMessage = string.Empty;

        public string ProductName => _product.ProductName;
        public string ProductImageUrl => _product.ImageUrl;
        public string ProductBarcode => _product.Barcode;
        public string ProductUnit => _product.Unit;
        public string ProductCategoryName => _product.CategoryName; 
        
        // Định dạng giá tiền
        public string ProductPriceFormatted => 
            _product.Price.ToString("N0", new CultureInfo("vi-VN")) + " VNĐ";

        public ProductDetailViewModel(ProductResponse product)
        {
            _product = product;
        }
    
        public ProductDetailViewModel(ICartService cartService)
        {
            _cartService = cartService;
        }
        
        [RelayCommand]
        private void Increase()
        {
            Quantity++;
        }
        
        
        
        [RelayCommand]
        public async Task AddToCart(int qty = 1)
        {
            if (Product == null) return;
    
            await _cartService.AddItemAsync(Product, qty);
            StatusMessage = "Đã thêm vào giỏ hàng";
            // Nếu cần hiển thị alert, page có thể lắng nghe StatusMessage hoặc raise event
        }
        
        [RelayCommand]
        private void Decrease()
        {
            if (Quantity > 1)
            {
                Quantity--;
            }
        }
    }
