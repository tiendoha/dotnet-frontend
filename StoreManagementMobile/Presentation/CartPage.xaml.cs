using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using StoreManagementMobile.ViewModels;
using StoreManagementMobile.Models;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.UI.Xaml.Navigation;

namespace StoreManagementMobile.Presentation;

public sealed partial class CartPage : Page
{
    public CartListViewModel ViewModel => (CartListViewModel)this.DataContext;

    public CartPage()
    {
        this.InitializeComponent();

        // 👇 GIẢI QUYẾT LỖI COMMAND KHÔNG CHẠY
        var app = (App)Application.Current;
        var vm = app.Host.Services.GetRequiredService<CartListViewModel>();

        this.DataContext = vm;
    }
    
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        // Load dữ liệu khi điều hướng vào trang
        await ViewModel.LoadItems();
        // Chỉ load promotions nếu giỏ hàng có sản phẩm
        if (ViewModel.Items.Count > 0)
        {
            await ViewModel.LoadPromotions();
        }
    }
    
    private void QuantityTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb && tb.DataContext is CartItem item)
        {
            // Gọi command trong ViewModel
            ViewModel.UpdateQuantityCommand.Execute(item);
        }
    }
    
    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        // Luôn về ProductListPage (trang chính)
        this.Frame.Navigate(typeof(ProductListPage));
    }
    
    private async void TestApiButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var app = (App)Application.Current;
            var api = app.Host.Services.GetRequiredService<Services.IStoreApi>();
            
            System.Diagnostics.Debug.WriteLine("🔧 Testing API connection to /api/Promotion...");
            var result = await api.GetPromotions();
            
            if (result?.Success == true)
            {
                await new ContentDialog
                {
                    Title = "✅ Kết nối thành công",
                    Content = $"Backend đang chạy!\n\nURL: http://10.0.2.2:5000\nEndpoint: /api/Promotion\nKết quả: {result.Data?.Items?.Count ?? 0} khuyến mãi",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                }.ShowAsync();
            }
            else
            {
                await new ContentDialog
                {
                    Title = "⚠️ Kết nối nhưng có lỗi",
                    Content = $"Backend trả về lỗi:\n{result?.Message}\n\nURL: http://10.0.2.2:5000",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                }.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            System.Diagnostics.Debug.WriteLine($"💥 API test failed: {msg}");
            System.Diagnostics.Debug.WriteLine($"   Full exception: {ex}");
            
            var errorDetail = "";
            if (msg.Contains("404"))
                errorDetail = "\n\n❗ Endpoint không tồn tại - Kiểm tra backend có route /api/Promotion không";
            else if (msg.Contains("Connection refused") || msg.Contains("No connection"))
                errorDetail = "\n\n❗ Backend chưa chạy - Hãy start backend trước";
            else if (msg.Contains("401") || msg.Contains("Unauthorized"))
                errorDetail = "\n\n❗ Chưa đăng nhập - Thử login lại";
            
            await new ContentDialog
            {
                Title = "❌ Lỗi kết nối",
                Content = $"Không thể kết nối backend:\n{msg}\n\nURL: http://10.0.2.2:5000{errorDetail}",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            }.ShowAsync();
        }
    }

}
