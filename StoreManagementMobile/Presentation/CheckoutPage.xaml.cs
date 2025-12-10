using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using StoreManagementMobile.ViewModels;
using StoreManagementMobile.Models;

namespace StoreManagementMobile.Presentation;

public sealed partial class CheckoutPage : Page
{
    public CheckoutViewModel ViewModel => (CheckoutViewModel)DataContext;
    private bool _isFromBuyNow = false;

    public CheckoutPage()
    {
        this.InitializeComponent();
        var app = (App)Application.Current;
        this.DataContext = app.Host.Services.GetRequiredService<CheckoutViewModel>();
    }

    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is CheckoutNavigationData nav)
        {
            ViewModel.Initialize(nav);
            _isFromBuyNow = nav.IsFromBuyNow; // Lưu flag
            
            // 👉 Đã bỏ auto-load thông tin khách hàng
            // User sẽ tự nhập hoặc có thể thêm nút "Tải thông tin" nếu cần
            // _ = ViewModel.LoadInfoCustomer();
        }
    }
    
    // Override nút Back hệ thống
    protected override void OnNavigatingFrom(Microsoft.UI.Xaml.Navigation.NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);
        
        // Nếu đang quay lại và là từ "Mua ngay"
        if (e.NavigationMode == Microsoft.UI.Xaml.Navigation.NavigationMode.Back && _isFromBuyNow)
        {
            // Hủy navigation mặc định
            e.Cancel = true;
            
            // Quay lại 2 bước (bỏ qua ProductDetailPage, về ProductListPage)
            // Hoặc quay về trang trước ProductDetailPage
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack(); // Về ProductDetailPage
            }
        }
    }
}
