using Microsoft.Extensions.Logging;
using StoreManagementMobile.Presentation;
using StoreManagementMobile.Services;
using StoreManagementMobile.Services.LocalDb;
using StoreManagementMobile.ViewModels;
using StoreManagementMobile.Models;
using Refit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Uno.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace StoreManagementMobile;

public partial class App : Application
{
    public IHost Host { get; private set; } = null!;
    public static string UserToken { get; set; } = string.Empty;
    private Window? _mainWindow;
    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var builder = this.CreateBuilder(args)
            .Configure(host => host
#if DEBUG
                .UseEnvironment(Environments.Development)
#endif
                .ConfigureServices((context, services) =>
                {
                    // Cấu hình API (Android dùng 10.0.2.2)
                    services.AddRefitClient<IStoreApi>()
                            .ConfigureHttpClient(c =>
                            {
#if __ANDROID__
                                c.BaseAddress = new Uri("http://10.0.2.2:5000");
#else
                                c.BaseAddress = new Uri("http://localhost:5000");
#endif
                            });

            

                    services.AddDbContext<AppDbContext>();
                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<HomeViewModel>();
                    services.AddTransient<ProductListViewModel>();
                })
            );

        Host = builder.Build();

     
        var window = Microsoft.UI.Xaml.Window.Current;

        // 2. Nếu không có (thường là trên Windows), thì mới tạo mới
        if (window == null)
        {
            window = new Microsoft.UI.Xaml.Window();
        }

        // Gán vào biến _mainWindow để giữ tham chiếu (tránh bị Windows thu hồi bộ nhớ)
        _mainWindow = window;

        // 3. Tạo khung hiển thị (Frame)
        var rootFrame = window.Content as Frame;
        if (rootFrame == null)
        {
            rootFrame = new Frame();
            window.Content = rootFrame;
        }

        // 4. Điều hướng vào trang Login
        if (rootFrame.Content == null)
        {
            rootFrame.Navigate(typeof(LoginPage));
        }

        // 5. Kích hoạt cửa sổ
        window.Activate();
    }
}