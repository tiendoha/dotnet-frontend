using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StoreManagementMobile.Models;
using StoreManagementMobile.Services;

namespace StoreManagementMobile.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IStoreApi _apiService;
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
            // BackendResponse được định nghĩa trong Models/AuthModels.cs
            var request = new LoginRequest { Username = Username, Password = Password };
            
            // Gọi API
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
