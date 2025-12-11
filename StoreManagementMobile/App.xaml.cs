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
using System.Diagnostics;
using StoreManagementMobile.Services.Auth;

namespace StoreManagementMobile;

public partial class App : Application
{
    public IHost Host { get; private set; } = null!;
    public static string UserToken { get; set; } = string.Empty;

    // ⭐ Cart theo user local
    public static int UserId { get; set; } = 0;

    private Window? _mainWindow;
    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Debug.WriteLine("🚀 OnLaunched bắt đầu");

        try
        {
            // ============================
            // 1. Build Host
            // ============================
            var builder = this.CreateBuilder(args)
                .Configure(host => host
#if DEBUG
                    .UseEnvironment(Environments.Development)
#endif
                    .ConfigureServices((context, services) =>
                    {
                        Debug.WriteLine("🔧 Đang đăng ký DI Services...");

                        services.AddTransient<TokenHandler>();

                        services.AddRefitClient<IStoreApi>()
                            .ConfigureHttpClient(c =>
                            {
#if __ANDROID__
                                c.BaseAddress = new Uri("http://10.0.2.2:5000");
#else
                                c.BaseAddress = new Uri("http://localhost:5000");
#endif
                            })
                            .AddHttpMessageHandler<TokenHandler>();

                        services.AddDbContext<AppDbContext>();

                        // ViewModels
                        services.AddTransient<LoginViewModel>();
                        services.AddTransient<HomeViewModel>();
                        services.AddTransient<CartListViewModel>();
                        services.AddTransient<ProductDetailViewModel>();
                        services.AddTransient<CheckoutViewModel>();
//                        services.AddTransient<ProductListViewModel>();

                        // Services
                        services.AddSingleton<ICartService, CartService>();
                        services.AddSingleton<IOrderHistoryService, OrderHistoryService>();
                    })
           
            );

            Host = builder.Build();
            Debug.WriteLine("✅ Host build thành công");

            // ======================================================
            // ⭐⭐ 2. Fake UserId & Token (Test Mode)
            // ======================================================
            // 👉 Đã comment - app mới vào chưa login
            // App.UserId = 1;
            // App.UserToken = "...";
            // ======================================================

     
        var window = Microsoft.UI.Xaml.Window.Current;
            // ============================
            // 3. SQLite Create DB
            // ============================
            using (var scope = Host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                Debug.WriteLine("📦 EnsureCreated() database...");
                
                // 👉 ĐÃ COMMENT để KHÔNG xóa database mỗi lần mở app
                // try
                // {
                //     db.Database.EnsureDeleted();
                //     Debug.WriteLine("🗑️ Đã xóa database cũ");
                // }
                // catch { }
                
                db.Database.EnsureCreated();
                Debug.WriteLine("✅ Database đã được tạo với bảng OrderHistories");
                
            }

            // ============================
            // 4. Lấy Window
            // ============================
            _mainWindow = window;

            // ============================
            // 5. Tạo Frame nếu chưa có
            // ============================
            var rootFrame = window.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                window.Content = rootFrame;
            }

        // 4. Điều hướng vào trang ProductListPage
        if (rootFrame.Content == null)
        {
            Debug.WriteLine("👉 Điều hướng vào ProductListPage với userId :" + App.UserId + " - và token là :" + App.UserToken);
            rootFrame.Navigate(typeof(ProductListPage));
        }

            // ============================
            // 7. Kích hoạt Window
            // ============================
            window.Activate();
            Debug.WriteLine("🚀 OnLaunched kết thúc OK");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"💥 Lỗi trong OnLaunched: {ex}");
        }
    }
}
