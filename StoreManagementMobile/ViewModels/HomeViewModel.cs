using CommunityToolkit.Mvvm.ComponentModel;

namespace StoreManagementMobile.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    [ObservableProperty]
    private string _welcomeMessage = "Xin chào! Đăng nhập thành công.";
}