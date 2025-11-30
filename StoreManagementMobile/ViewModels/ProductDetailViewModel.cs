// File: ProductDetailViewModel.cs (Ví dụ mẫu)

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StoreManagementMobile.Models; // Giả sử ProductResponse ở đây
using System.Globalization;

namespace StoreManagementMobile.Presentation
{
    public partial class ProductDetailViewModel : ObservableObject
    {
        private readonly ProductResponse _product;

        [ObservableProperty]
        private int _quantity = 1;

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

        [RelayCommand]
        private void Increase()
        {
            Quantity++;
        }

        [RelayCommand]
        private void Decrease()
        {
            if (Quantity > 1)
            {
                Quantity--;
            }
        }

        [RelayCommand]
        private void AddToCart()
        {
            // TODO: Thêm logic thêm sản phẩm (_product) với số lượng (Quantity) vào giỏ hàng
            // Ví dụ: MessagingService.Send(new AddToCartMessage(_product, Quantity));
        }

        // Bạn sẽ cần thêm Command cho nút Back nếu không xử lý trong code-behind
        // [RelayCommand]
        // private void NavigateBack()
        // {
        //     // Logic Frame.GoBack()
        // }
    }
}