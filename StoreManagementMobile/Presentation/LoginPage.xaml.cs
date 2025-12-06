using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
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
            // Chuyển sang ProductListPage (thay MainPage)
            Frame.Navigate(typeof(ProductListPage));
        };
    }

    public LoginViewModel ViewModel { get; }

    // Xử lý nút Back
    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(ProductListPage));
    }
}