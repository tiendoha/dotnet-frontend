using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StoreManagementMobile.Models;

namespace StoreManagementMobile.Presentation
{
    public partial class ProductDetailViewModel : ObservableObject
    {
        [ObservableProperty]
        private ProductResponse _product;

        [ObservableProperty]
        private int _quantity = 1;

        public ProductDetailViewModel(ProductResponse product)
        {
            Product = product;
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
                Quantity--;
        }

        [RelayCommand]
        private void AddToCart()
        {
            // TODO: Thêm sản phẩm vào giỏ hàng
            // Ví dụ:
            // CartService.Add(Product, Quantity);
        }
    }
}
