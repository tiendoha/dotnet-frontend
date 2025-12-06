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
                // Tạo giao diện đẹp hơn với StackPanel
                var contentPanel = new StackPanel { Spacing = 16, Padding = new Thickness(8) };

                // Header - Mã đơn hàng
                var headerPanel = new StackPanel 
                { 
                    Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.LightBlue),
                    Padding = new Thickness(12),
                    CornerRadius = new CornerRadius(8)
                };
                headerPanel.Children.Add(new TextBlock 
                { 
                    Text = $"Đơn hàng #{order.OrderId}",
                    FontSize = 20,
                    FontWeight = Microsoft.UI.Text.FontWeights.Bold
                });
                headerPanel.Children.Add(new TextBlock 
                { 
                    Text = order.OrderDateText,
                    FontSize = 14,
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.DarkSlateGray)
                });
                contentPanel.Children.Add(headerPanel);

                // Thông tin khách hàng
                var customerPanel = new StackPanel { Spacing = 8 };
                customerPanel.Children.Add(new TextBlock 
                { 
                    Text = "Thông tin khách hàng",
                    FontSize = 16,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
                });
                customerPanel.Children.Add(new TextBlock { Text = $"👤 {order.CustomerName}" });
                customerPanel.Children.Add(new TextBlock { Text = $"📞 {order.CustomerPhone}" });
                customerPanel.Children.Add(new TextBlock 
                { 
                    Text = $"📍 {order.CustomerAddress}",
                    TextWrapping = TextWrapping.Wrap
                });
                contentPanel.Children.Add(customerPanel);

                // Danh sách sản phẩm
                if (!string.IsNullOrEmpty(order.OrderDetailsJson))
                {
                    try
                    {
                        var products = JsonSerializer.Deserialize<List<OrderProductDetail>>(order.OrderDetailsJson);
                        if (products != null && products.Any())
                        {
                            var productsPanel = new StackPanel { Spacing = 8 };
                            productsPanel.Children.Add(new TextBlock 
                            { 
                                Text = "Sản phẩm đã mua",
                                FontSize = 16,
                                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                                Margin = new Thickness(0, 8, 0, 0)
                            });

                            foreach (var product in products)
                            {
                                var productBorder = new Border
                                {
                                    Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.WhiteSmoke),
                                    Padding = new Thickness(12),
                                    CornerRadius = new CornerRadius(6),
                                    Margin = new Thickness(0, 4, 0, 4)
                                };

                                var productStack = new StackPanel { Spacing = 4 };
                                productStack.Children.Add(new TextBlock 
                                { 
                                    Text = product.ProductName,
                                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                                    FontSize = 14
                                });
                                productStack.Children.Add(new TextBlock 
                                { 
                                    Text = $"Số lượng: {product.Quantity} x {product.Price:N0} đ = {product.Quantity * product.Price:N0} đ",
                                    FontSize = 13,
                                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.DarkGreen)
                                });

                                productBorder.Child = productStack;
                                productsPanel.Children.Add(productBorder);
                            }

                            contentPanel.Children.Add(productsPanel);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Lỗi parse OrderDetailsJson: {ex}");
                    }
                }

                // Tổng tiền
                var summaryPanel = new StackPanel 
                { 
                    Spacing = 4,
                    Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.LightYellow),
                    Padding = new Thickness(12),
                    CornerRadius = new CornerRadius(8),
                    Margin = new Thickness(0, 8, 0, 0)
                };
                summaryPanel.Children.Add(new TextBlock { Text = $"Tạm tính: {order.TotalAmount:N0} đ" });
                summaryPanel.Children.Add(new TextBlock 
                { 
                    Text = $"Giảm giá: -{order.DiscountAmount:N0} đ",
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red)
                });
                summaryPanel.Children.Add(new TextBlock 
                { 
                    Text = $"Tổng cộng: {order.FinalAmount:N0} đ",
                    FontSize = 18,
                    FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green)
                });
                summaryPanel.Children.Add(new TextBlock 
                { 
                    Text = $"Thanh toán: {order.PaymentMethod}",
                    Margin = new Thickness(0, 8, 0, 0)
                });
                summaryPanel.Children.Add(new TextBlock { Text = $"Trạng thái: {order.Status}" });
                contentPanel.Children.Add(summaryPanel);

                var dialog = new ContentDialog
                {
                    Title = "Chi tiết đơn hàng",
                    Content = new ScrollViewer
                    {
                        Content = contentPanel,
                        MaxHeight = 500
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
