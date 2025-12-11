using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StoreManagementMobile.Models;
using StoreManagementMobile.Services;
using StoreManagementMobile.DTOs;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace StoreManagementMobile.ViewModels;

public partial class CheckoutViewModel : ObservableObject
{
    private readonly ICartService _cartService;
    private readonly IStoreApi _api;
    private readonly IOrderHistoryService _orderHistoryService;

    // Navigation data
    public CheckoutNavigationData NavigationData { get; private set; } = new();

    // ---------------------- FORM FIELDS ----------------------
    [ObservableProperty] private string customerName = "";
    [ObservableProperty] private string customerPhone = "";
    [ObservableProperty] private string customerEmail = "";
    [ObservableProperty] private string customerAddress = "";

    // Payment method enum to backend: Cash | Card | EWallet
    [ObservableProperty] private string paymentMethod = "Cash";

    public bool IsCash { get => PaymentMethod == "cash"; set { if (value) PaymentMethod = "cash"; } }
    public bool IsCard { get => PaymentMethod == "card"; set { if (value) PaymentMethod = "card"; } }
    public bool IsEWallet { get => PaymentMethod == "ewallet"; set { if (value) PaymentMethod = "ewallet"; } }

    // ---------------------- SUMMARY ----------------------
    [ObservableProperty] private decimal subtotal;
    [ObservableProperty] private decimal discount;
    [ObservableProperty] private decimal total;

    public string TotalText => $"{Total:N0} đ";

    partial void OnTotalChanged(decimal value)
    {
        OnPropertyChanged(nameof(TotalText));
    }

    // ---------------------- UI STATE ----------------------
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string statusMessage = "";

    public CheckoutViewModel(ICartService cartService, IStoreApi api, IOrderHistoryService orderHistoryService)
    {
        _cartService = cartService;
        _api = api;
        _orderHistoryService = orderHistoryService;
    }

    // ============================================================
    // Initialize from navigation
    // ============================================================
    public void Initialize(CheckoutNavigationData navData)
    {
        NavigationData = navData ?? new CheckoutNavigationData();

        Subtotal = navData.Subtotal;
        Discount = navData.Discount;
        Total = navData.Total;

        StatusMessage = "";
    }

    // ============================================================
    // Validation Helpers
    // ============================================================
    private bool IsValidPhone(string phone)
    {
        return Regex.IsMatch(phone, @"^[0-9]{10}$");
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return true; // optional
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }
    
    // ============================================================
    // Customer Information
    // ============================================================  
    
    public async Task LoadInfoCustomer()
    {
        try
        {
            StatusMessage = "Đang tải thông tin khách hàng...";
    
            int userId = App.UserId;   // ⭐ Lấy userId từ App (bạn đã set khi login)
    
            if (userId <= 0)
            {
                StatusMessage = "Không tìm thấy userId.";
                return;
            }
    
            var response = await _api.GetCustomerByUserId(userId);
    
            if (response == null || !response.Success || response.Data == null)
            {
                StatusMessage = "Không thể tải thông tin khách hàng.";
                return;
            }
    
            var customer = response.Data;
    
            // ⭐ Đổ dữ liệu vào UI
            CustomerName = customer.Name;
            CustomerPhone = customer.Phone;
            CustomerEmail = customer.Email;
            CustomerAddress = customer.Address;
    
            StatusMessage = "";
            Debug.WriteLine("✔ LoadInfoCustomer() thành công.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("💥 Exception LoadInfoCustomer: " + ex);
            StatusMessage = "Không thể tải thông tin khách hàng.";
        }
    }
  

    // ============================================================
    // MAIN: Place Order
    // ============================================================
    [RelayCommand]
    public async Task PlaceOrder()
    {
        if (IsBusy) return;

        // -------------- VALIDATION --------------
        if (string.IsNullOrWhiteSpace(CustomerName))
        {
            StatusMessage = "Vui lòng nhập họ tên.";
            return;
        }

        if (!IsValidPhone(CustomerPhone))
        {
            StatusMessage = "Số điện thoại phải là 10 chữ số.";
            return;
        }

        if (!IsValidEmail(CustomerEmail))
        {
            StatusMessage = "Email không hợp lệ.";
            return;
        }

        if (string.IsNullOrWhiteSpace(CustomerAddress))
        {
            StatusMessage = "Vui lòng nhập địa chỉ giao hàng.";
            return;
        }

        // UI Loading
        IsBusy = true;
        StatusMessage = "Đang tạo đơn hàng...";

        try
        {
            List<Models.OrderItemDto> details;
            
            // Nếu là "Mua ngay" - dùng sản phẩm từ NavigationData
            if (NavigationData.IsFromBuyNow && NavigationData.BuyNowProduct != null)
            {
                var product = NavigationData.BuyNowProduct;
                details = new List<Models.OrderItemDto>
                {
                    new Models.OrderItemDto
                    {
                        ProductId = product.ProductId,
                        ProductName = product.ProductName, 
                        Quantity = product.Quantity,
                        Price = product.Price * product.Quantity
                    }
                };
                Debug.WriteLine($"🛒 Đặt hàng 'Mua ngay': {product.ProductName} x{product.Quantity}");
            }
            else
            {
                // Nếu là thanh toán bình thường - lấy từ giỏ hàng SQLite
                var cartItems = await _cartService.GetItemsAsync();
                if (!cartItems.Any())
                {
                    StatusMessage = "Giỏ hàng trống.";
                    return;
                }

                details = cartItems.Select(c => new Models.OrderItemDto
                {
                    ProductId = c.ProductId,
                    ProductName = c.ProductName, // ⭐ Thêm ProductName
                    Quantity = c.Quantity,
                    Price = c.Price * c.Quantity
                }).ToList();
                Debug.WriteLine($"🛍️ Đặt hàng từ giỏ: {details.Count} sản phẩm");
            }


            var request = new Models.CreateOrderRequest
            {
                CustomerId = null,
                CustomerName = CustomerName,
                CustomerPhone = CustomerPhone,
                CustomerEmail = string.IsNullOrWhiteSpace(CustomerEmail) ? null : CustomerEmail,
                CustomerAddress = CustomerAddress,
                OrderDetails = details,
                PaymentMethod = PaymentMethod,
                AmountPaid = Total,
                PromoId = NavigationData?.AppliedPromoId
            };

            Debug.WriteLine("📦 Sending Order:");
            Debug.WriteLine($"   📋 OrderDetails count: {request.OrderDetails.Count}");
            foreach (var item in request.OrderDetails)
            {
                Debug.WriteLine($"      - ProductId={item.ProductId}, Qty={item.Quantity}, Price={item.Price}");
            }
            Debug.WriteLine(System.Text.Json.JsonSerializer.Serialize(request, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

            Debug.WriteLine("🌐 Gọi API: POST /api/orders");
            var apiResult = await _api.CreateOrder(request);
            Debug.WriteLine($"✅ API response: Success={apiResult?.Success}, Message={apiResult?.Message}");
            
            if (apiResult?.Data != null)
            {
                Debug.WriteLine($"   📄 OrderId returned: {apiResult.Data.OrderId}");
            }
            if (apiResult?.Errors != null && apiResult.Errors.Any())
            {
                Debug.WriteLine($"   ❌ Errors: {string.Join(", ", apiResult.Errors)}");
            }

            if (apiResult.Success)
            {
                // 1️⃣ Lưu vào SQLite OrderHistory
                var orderHistory = new OrderHistory
                {
                    OrderId = apiResult.Data?.OrderId ?? 0,
                    OrderDate = DateTime.Now,
                    CustomerName = CustomerName,
                    CustomerPhone = CustomerPhone,
                    CustomerAddress = CustomerAddress,
                    TotalAmount = Subtotal,
                    DiscountAmount = Discount,
                    FinalAmount = Total,
                    PaymentMethod = PaymentMethod,
                    Status = "Completed",
                    OrderDetailsJson = System.Text.Json.JsonSerializer.Serialize(details)
                };
                
                await _orderHistoryService.AddOrderAsync(orderHistory);
                Debug.WriteLine($"💾 Đã lưu OrderHistory vào SQLite: OrderId={orderHistory.OrderId}");
                
                // 2️⃣ Chỉ clear giỏ hàng nếu KHÔNG phải "Mua ngay"
                if (!NavigationData.IsFromBuyNow)
                {
                    await _cartService.ClearAsync();
                    Debug.WriteLine("✅ Đã xoá giỏ hàng");
                }
                else
                {
                    Debug.WriteLine("✅ 'Mua ngay' - Giữ nguyên giỏ hàng");
                }

                // 3️⃣ Chuyển sang trang OrderSuccessPage
                var successData = new StoreManagementMobile.Presentation.OrderSuccessData
                {
                    Total = Total,
                    CustomerName = CustomerName,
                    CustomerPhone = CustomerPhone
                };
                
                var window = Window.Current;
                (window.Content as Frame)?.Navigate(typeof(StoreManagementMobile.Presentation.OrderSuccessPage), successData);
                return;
            }

            // API lỗi
            StatusMessage = apiResult.Errors?.FirstOrDefault() ?? apiResult.Message ?? "Tạo đơn thất bại.";
        }
        catch (System.Exception ex)
        {
            var innerMsg = ex.InnerException?.Message ?? ex.Message;
            StatusMessage = $"❌ Lỗi kết nối: {innerMsg}";
            Debug.WriteLine("💥 PlaceOrder Exception: " + ex);
            Debug.WriteLine($"   Chi tiết: {innerMsg}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ============================================================
    // SUCCESS DIALOG
    // ============================================================
    private async Task ShowSuccessDialog()
    {
        var successMessage = $"✅ Đơn hàng đã được tạo thành công!\n\n" +
                           $"💰 Tổng tiền: {Total:N0} đ\n" +
                           $"📍 Giao tới: {CustomerName}\n" +
                           $"📞 Liên hệ: {CustomerPhone}\n\n" +
                           $"Cảm ơn bạn đã mua hàng! 🙏";
        
        var dialog = new ContentDialog
        {
            Title = "🎉 Đặt hàng thành công!",
            Content = successMessage,
            CloseButtonText = "OK",
            XamlRoot = (Window.Current.Content as FrameworkElement)?.XamlRoot
        };

        await dialog.ShowAsync();
    }
    
    // ============================================================
    // Điều hướng quay lại CartPag
    // ============================================================    
    
    [RelayCommand]
    public void CartPageNavigation()
    {
        try
        {
            var window = Window.Current;
            var frame = window?.Content as Frame;
    
            if (frame != null)
            {
                frame.Navigate(typeof(StoreManagementMobile.Presentation.CartPage));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("💥 CartPageNavigation() Error: " + ex);
            StatusMessage = "Không thể quay về giỏ hàng.";
        }
    }

}
