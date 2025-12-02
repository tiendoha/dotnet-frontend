using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using StoreManagementMobile.Models;
using StoreManagementMobile.ViewModels;

namespace StoreManagementMobile.Presentation
{
    public sealed partial class ProductDetailPage : Page
    {
        // 🔥 ĐÃ SỬA: Thay đổi thành Public Property với Setter để có thể gán data context.
        public ProductDetailViewModel ViewModel { get; set; }

        public ProductDetailPage()
        {
            this.InitializeComponent();
            // Đặt DataContext cho XAML, ViewModel sẽ được khởi tạo trong OnNavigatedTo
            this.DataContext = this; 
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // 1. Kiểm tra xem tham số truyền vào có phải là ProductResponse không
            if (e.Parameter is ProductResponse product)
            {
                // 2. Khởi tạo ViewModel và truyền dữ liệu sản phẩm vào
                // Giả định bạn đã có ProductDetailViewModel (xem mục 3)
                ViewModel = new ProductDetailViewModel(product);
                
                // 3. Cập nhật DataContext để giao diện hiển thị dữ liệu mới
                this.DataContext = ViewModel;

                // 4. (Tùy chọn) Gán Command cho nút Back (Nếu command không nằm trong ViewModel)
                // BackButton.Command = ViewModel.NavigateBackCommand;
            } 
            else
            {
                // Nếu không có sản phẩm được truyền vào, có thể quay lại trang danh sách
                // hoặc hiển thị lỗi. Ở đây tôi sẽ quay lại trang trước.
                if (this.Frame.CanGoBack)
                {
                    this.Frame.GoBack();
                }
            }
        }
        
        // 🔥 ĐÃ THÊM: Xử lý nút Back (WinUI/MAUI)
        private void BackButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }
        
        // 🔥 MỚI: Xử lý nút Mua ngay
        private async void BuyNowButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                // 1. Thêm sản phẩm vào giỏ hàng (SQLite)
                await ViewModel.AddToCart();
                
                // 2. Tạo dữ liệu để truyền sang CheckoutPage
                var checkoutData = new CheckoutNavigationData
                {
                    Subtotal = ViewModel.ProductPrice * ViewModel.Quantity,
                    Discount = 0,
                    Total = ViewModel.ProductPrice * ViewModel.Quantity,
                    AppliedPromoId = null,
                    IsFromBuyNow = true // Đánh dấu là từ "Mua ngay"
                };
                
                // 3. Điều hướng trực tiếp sang CheckoutPage
                this.Frame.Navigate(typeof(CheckoutPage), checkoutData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Lỗi Mua ngay: {ex}");
            }
        }
    }
}