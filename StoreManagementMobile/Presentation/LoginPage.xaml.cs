using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection; // Cần để lấy Service
using StoreManagementMobile.ViewModels;

namespace StoreManagementMobile.Presentation;

public sealed partial class LoginPage : Page
{
    public LoginPage()
    {
        this.InitializeComponent();
        
        // Lấy ViewModel từ Container của App
        var app = (App)Application.Current;
        ViewModel = app.Host.Services.GetRequiredService<LoginViewModel>();
        
        // Gán DataContext để Binding trong XAML hoạt động
        this.DataContext = ViewModel;

        ViewModel.NavigateToMain += () => 
        {
            // Chuyển sang MainPage
            Frame.Navigate(typeof(MainPage));
        };
    }

    public LoginViewModel ViewModel { get; }
}