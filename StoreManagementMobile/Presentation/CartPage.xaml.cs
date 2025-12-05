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
}
