using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml;

namespace StoreManagementMobile.Presentation;

public sealed partial class OrderSuccessPage : Page
{
    public OrderSuccessPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // Nhận dữ liệu từ CheckoutViewModel
        if (e.Parameter is OrderSuccessData data)
        {
            TotalText.Text = $"{data.Total:N0} đ";
            CustomerNameText.Text = data.CustomerName;
            CustomerPhoneText.Text = data.CustomerPhone;
        }
    }

    private void BackToHome_Click(object sender, RoutedEventArgs e)
    {
        // Quay về trang ProductListPage
        Frame.Navigate(typeof(ProductListPage));
    }
}

// Class để truyền dữ liệu
public class OrderSuccessData
{
    public decimal Total { get; set; }
    public string CustomerName { get; set; } = "";
    public string CustomerPhone { get; set; } = "";
}
