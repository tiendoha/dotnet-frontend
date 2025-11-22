using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using StoreManagementMobile.ViewModels;

namespace StoreManagementMobile.Presentation;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
        
        var app = (App)Application.Current;
        ViewModel = app.Host.Services.GetRequiredService<HomeViewModel>();
        this.DataContext = ViewModel;
    }

    public HomeViewModel ViewModel { get; }
}