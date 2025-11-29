using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using StoreManagementMobile.ViewModels;
using StoreManagementMobile.Models;

namespace StoreManagementMobile.Presentation;

public sealed partial class CheckoutPage : Page
{
    public CheckoutViewModel ViewModel => (CheckoutViewModel)DataContext;

    public CheckoutPage()
    {
        this.InitializeComponent();
        var app = (App)Application.Current;
        this.DataContext = app.Host.Services.GetRequiredService<CheckoutViewModel>();
    }

    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is CheckoutNavigationData nav)
        {
            ViewModel.Initialize(nav);
        }
    }
}
