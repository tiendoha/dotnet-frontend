using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using StoreManagementMobile.ViewModels;
using Microsoft.Extensions.DependencyInjection;

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

        _ = vm.LoadItems();
        _ = vm.LoadPromotions();
    }
    
    private void QuantityTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb && tb.DataContext is CartItem item)
        {
            // Gọi command trong ViewModel
            ViewModel.UpdateQuantityCommand.Execute(item);
        }
    }

}
