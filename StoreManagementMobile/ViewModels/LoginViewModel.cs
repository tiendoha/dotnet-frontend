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
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            // Gọi API login thật
            var request = new LoginRequest { Username = Username, Password = Password };
            var response = await _apiService.Login(request);

            if (response.Success && response.Data != null)
            {
                // Lưu token và userId
                App.UserToken = response.Data.Token;
                App.UserId = response.Data.User.UserId;
                
                System.Diagnostics.Debug.WriteLine($"✅ Login thành công! UserId={App.UserId}");
                
                // Điều hướng sang ProductListPage
                NavigateToMain?.Invoke();
            }
            else
            {
                ErrorMessage = response.Message ?? "Đăng nhập thất bại.";
                System.Diagnostics.Debug.WriteLine($"❌ Login failed: {ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi kết nối: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"💥 Login exception: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
