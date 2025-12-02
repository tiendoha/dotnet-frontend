using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StoreManagementMobile.Models;
using StoreManagementMobile.Services;

namespace StoreManagementMobile.ViewModels;

/// <summary>
/// ViewModel xử lý login. Khi đăng nhập thành công sẽ raise event `NavigateToMain`.
/// Page (LoginPage.xaml.cs) có thể đăng ký event này để điều hướng (Frame.Navigate hoặc Shell.GoToAsync).
/// Ví dụ trong `LoginPage.xaml.cs`:
/// <code>
/// var vm = (LoginViewModel)DataContext;
/// vm.NavigateToMain += () => { this.Frame?.Navigate(typeof(ProductListPage)); };
/// </code>
/// </summary>
public partial class LoginViewModel : ObservableObject
{
    private readonly IStoreApi _apiService;
    // Event được raise khi login thành công. Page phải đăng ký để thực hiện điều hướng UI.
    public event Action? NavigateToMain;

    public LoginViewModel(IStoreApi apiService)
    {
        _apiService = apiService;
    }

    // QUAY VỀ CÁCH CŨ (Dùng field private có gạch dưới)
    // Mặc kệ warning MVVMTK0045, nó không gây lỗi chạy app
    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [RelayCommand]
    private async Task Login()
    {
        // DEV: bỏ xác nhận backend - luôn coi là đăng nhập thành công
        // LƯU Ý: Đây là chế độ phát triển. Đừng để mã này trong production.
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            // Set a dummy token và UserId
            App.UserToken = "dev-token";
            App.UserId = 1;
            
            // Gọi event để điều hướng
            NavigateToMain?.Invoke();
            
            /* Comment out phần call API thật để test nhanh
            var request = new LoginRequest { Username = Username, Password = Password };
            var response = await _apiService.Login(request);

            if (response.Success && response.Data != null)
            {
                App.UserToken = response.Data.Token;
                App.UserId = response.Data.User.UserId;
                NavigateToMain?.Invoke();
            }
            else
            {
                ErrorMessage = response.Message ?? "Đăng nhập thất bại.";
            }
            */
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
