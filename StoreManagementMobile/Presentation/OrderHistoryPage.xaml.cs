using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml;
using StoreManagementMobile.Models;
using StoreManagementMobile.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace StoreManagementMobile.Presentation;

public sealed partial class OrderHistoryPage : Page
{
    private readonly IOrderHistoryService _orderHistoryService;

    public OrderHistoryPage()
    {
        this.InitializeComponent();
        
        var app = (App)Application.Current;
        _orderHistoryService = app.Host.Services.GetRequiredService<IOrderHistoryService>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await LoadOrders();
    }

    private async System.Threading.Tasks.Task LoadOrders()
    {
        try
        {
            var orders = await _orderHistoryService.GetAllOrdersAsync();
            
            if (orders == null || !orders.Any())
            {
                EmptyMessage.Visibility = Visibility.Visible;
                OrderList.ItemsSource = null;
            }
            else
            {
                EmptyMessage.Visibility = Visibility.Collapsed;
                OrderList.ItemsSource = orders;
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"💥 Lỗi load orders: {ex}");
            
            var dialog = new ContentDialog
            {
                Title = "Lỗi",
                Content = $"Không thể tải lịch sử đơn hàng: {ex.Message}",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(ProductListPage));
    }

    private async void ViewDetails_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int orderId)
        {
            var order = await _orderHistoryService.GetOrderByIdAsync(orderId);
            
            if (order != null)
            {
                var detailsText = $"Đơn hàng #{order.OrderId}\n\n" +
                                $"Ngày: {order.OrderDateText}\n" +
                                $"Khách hàng: {order.CustomerName}\n" +
                                $"SĐT: {order.CustomerPhone}\n" +
                                $"Địa chỉ: {order.CustomerAddress}\n\n";

                // Parse OrderDetailsJson để hiển thị danh sách sản phẩm
                if (!string.IsNullOrEmpty(order.OrderDetailsJson))
                {
                    try
                    {
                        var products = JsonSerializer.Deserialize<List<OrderProductDetail>>(order.OrderDetailsJson);
                        if (products != null && products.Any())
                        {
                            detailsText += "Sản phẩm:\n";
                            foreach (var product in products)
                            {
                                detailsText += $"• {product.ProductName}\n";
                                detailsText += $"  Số lượng: {product.Quantity} x {product.Price:N0} đ = {product.Quantity * product.Price:N0} đ\n";
                            }
                            detailsText += "\n";
                        }
                    }
                    catch (System.Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Lỗi parse OrderDetailsJson: {ex}");
                    }
                }

                detailsText += $"Tạm tính: {order.TotalAmount:N0} đ\n" +
                              $"Giảm giá: -{order.DiscountAmount:N0} đ\n" +
                              $"Tổng cộng: {order.FinalAmount:N0} đ\n\n" +
                              $"Thanh toán: {order.PaymentMethod}\n" +
                              $"Trạng thái: {order.Status}";
                
                var dialog = new ContentDialog
                {
                    Title = "Chi tiết đơn hàng",
                    Content = new ScrollViewer
                    {
                        Content = new TextBlock
                        {
                            Text = detailsText,
                            TextWrapping = TextWrapping.Wrap
                        },
                        MaxHeight = 400
                    },
                    CloseButtonText = "Đóng",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }
    }

    // DTO for deserializing OrderDetailsJson
    private class OrderProductDetail
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
