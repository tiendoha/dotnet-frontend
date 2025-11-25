using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using StoreManagementMobile.ViewModels;

namespace StoreManagementMobile.Presentation;

public sealed partial class CartPage : Page
{
    public CartListViewModel ViewModel { get; }

    public CartPage()
    {
        this.InitializeComponent();

        // 🔥 Lấy App để truy cập Host
        var app = (App)Application.Current;

        // 🔥 Resolve ViewModel từ DI container
        ViewModel = app.Host.Services.GetRequiredService<CartListViewModel>();

        // 🔥 Gán DataContext cho XAML
        this.DataContext = ViewModel;

        // 🔥 Load giỏ hàng khi vào page
        _ = ViewModel.LoadItems();
    }
}
