using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StoreManagementMobile.Models;
using StoreManagementMobile.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace StoreManagementMobile.ViewModels;

public partial class CheckoutViewModel : ObservableObject
{
    private readonly ICartService _cartService;
    private readonly IStoreApi _api;

    // Navigation data passed from Cart page
    public CheckoutNavigationData NavigationData { get; private set; } = new();

    // Form fields
    [ObservableProperty] private string customerName = string.Empty;
    [ObservableProperty] private string customerPhone = string.Empty;
    [ObservableProperty] private string customerEmail = string.Empty;
    [ObservableProperty] private string customerAddress = string.Empty;

    // Payment method: "Cash", "Card", "EWallet"
    [ObservableProperty] private string paymentMethod = "Cash";

    // Order summary (bound to UI)
    [ObservableProperty] private decimal subtotal;
    [ObservableProperty] private decimal discount;
    [ObservableProperty] private decimal total;

    // UI state
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string statusMessage = string.Empty;

    public IEnumerable<string> PaymentOptions { get; } = new[] { "Cash", "Card", "EWallet" };

    public CheckoutViewModel(ICartService cartService, IStoreApi api)
    {
        _cartService = cartService;
        _api = api;
    }

    /// <summary>
    /// Call this when page is navigated to, pass CheckoutNavigationData from Cart.
    /// It will populate summary fields and leave form empty for user to fill.
    /// </summary>
    public void Initialize(CheckoutNavigationData navData)
    {
        NavigationData = navData ?? new CheckoutNavigationData();
        Subtotal = NavigationData.Subtotal;
        Discount = NavigationData.Discount;
        Total = NavigationData.Total;
        Debug.WriteLine("Subtotal "  +  Subtotal +"+ Discount "+ Discount+ "+ Total : " + Total);

        // Clear any previous messages
        StatusMessage = string.Empty;
    }

    // ---- Place order ----
    [RelayCommand]
    public async Task PlaceOrder()
    {
        if (IsBusy) return;

        // basic validation
        if (string.IsNullOrWhiteSpace(CustomerName))
        {
            StatusMessage = "Vui lòng nhập họ tên.";
            return;
        }
        if (string.IsNullOrWhiteSpace(CustomerPhone))
        {
            StatusMessage = "Vui lòng nhập số điện thoại.";
            return;
        }
        if (string.IsNullOrWhiteSpace(CustomerAddress))
        {
            StatusMessage = "Vui lòng nhập địa chỉ giao hàng.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Đang gửi đơn hàng...";

        try
        {
            // Lấy cart items từ local sqlite
            var cartItems = await _cartService.GetItemsAsync();
            if (cartItems == null || !cartItems.Any())
            {
                StatusMessage = "Giỏ hàng trống.";
                return;
            }

            // Map sang order details
            var details = cartItems.Select(c => new OrderItemDto
            {
                ProductId = c.ProductId,
                Quantity = c.Quantity
            }).ToList();

            // Build request
            var request = new Models.CreateOrderRequest
            {
                CustomerId = null, // nếu có userId và backend muốn gán, có thể truyền App.UserId (tùy API)
                CustomerName = CustomerName,
                CustomerPhone = CustomerPhone,
                CustomerEmail = string.IsNullOrWhiteSpace(CustomerEmail) ? null : CustomerEmail,
                CustomerAddress = CustomerAddress,
                OrderDetails = details,
                PaymentMethod = PaymentMethod, // ensure matches backend enum strings
                AmountPaid = Total,
                PromoId = NavigationData?.AppliedPromoId
            };

            Debug.WriteLine("▶ PlaceOrder(): Gửi payload -> " + System.Text.Json.JsonSerializer.Serialize(request));

            // gọi API
            var apiResponse = await _api.CreateOrder(request);

            if (apiResponse != null && apiResponse.Success)
            {
                // Success flow
                StatusMessage = "Đặt hàng thành công!";
                Debug.WriteLine("▶ PlaceOrder(): success -> " + apiResponse.Message);

                // Clear local cart
                await _cartService.ClearAsync();

                // Điều hướng về MainPage (hoặc trang thông báo)
                var window = Window.Current;
                var frame = window?.Content as Frame;
                if (frame != null)
                {
                    // bạn có thể đổi về MainPage hoặc OrderSuccessPage nếu có
                    frame.Navigate(typeof(StoreManagementMobile.Presentation.MainPage));
                }
            }
            else
            {
                // API returned failure
                var msg = apiResponse?.Message ?? "Tạo đơn thất bại.";
                StatusMessage = $"Lỗi: {msg}";
                Debug.WriteLine("▶ PlaceOrder(): failed -> " + msg);
                // Không xoá cart, giữ nguyên cho user sửa
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("💥 Exception PlaceOrder: " + ex);
            StatusMessage = "Lỗi khi gửi đơn hàng: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
