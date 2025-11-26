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
    public static int UserId { get; set; } = 1;

    private Window? _mainWindow;

    public App()
    {
        this.InitializeComponent();
        Debug.WriteLine("🔥 App(): Constructor chạy");
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

                        // Services
                        services.AddSingleton<ICartService, CartService>();
                    })
            );

            Host = builder.Build();
            Debug.WriteLine("✅ Host build thành công");

            // ======================================================
            // ⭐⭐ 2. Fake UserId & Token (Test Mode)
            // ======================================================
            // 👉 LƯU Ý:
            // Khi login hoạt động, chỉ cần COMMENT 2 dòng này.
            App.UserId = 1;
            App.UserToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIxIiwidW5pcXVlX25hbWUiOiJhZG1pbiIsInJvbGUiOiJBZG1pbiIsImp0aSI6IjE1OGRlMTNjLTI0ZTktNGQxMS04MmI1LWIyMTBhMDU1ZmQxMyIsImlhdCI6MTc2NDE0NzA2NiwibmJmIjoxNzY0MTQ3MDY2LCJleHAiOjE3NjQxNTA2NjYsImlzcyI6IlN0b3JlTWFuYWdlbWVudEFQSSIsImF1ZCI6IlN0b3JlTWFuYWdlbWVudENsaWVudCJ9.Sfn386SCnhLv0-zXL2sn9QdVs02_YGGXzs3BpSgx0zs";
            // ======================================================

            // ============================
            // 3. SQLite Create DB
            // ============================
            using (var scope = Host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                Debug.WriteLine("📦 EnsureCreated() database...");
                db.Database.EnsureCreated();

                // ⭐ SEED TEST CART
                if (!db.CartItems.Any(c => c.UserId == App.UserId))
                {
                    db.CartItems.Add(new CartItem
                    {
                        UserId = App.UserId,
                        ProductId = 1,
                        ProductName = "Sản phẩm A",
                        Price = 12000,
                        Quantity = 1,
                        ImagePath = "https://via.placeholder.com/150"
                    });

                    db.CartItems.Add(new CartItem
                    {
                        UserId = App.UserId,
                        ProductId = 2,
                        ProductName = "Sản phẩm B",
                        Price = 54000,
                        Quantity = 2,
                        ImagePath = "https://via.placeholder.com/150"
                    });

                    db.SaveChanges();
                }
            }

            // ============================
            // 4. Lấy Window
            // ============================
            var window = Window.Current ?? new Window();
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

            // ============================
            // 6. Navigate vào CartPage
            // ============================
            Debug.WriteLine("➡ Điều hướng CartPage...");
            Debug.WriteLine("➡ UserId:" +App.UserId);
            Debug.WriteLine("➡ UserToken:" +App.UserToken);
            rootFrame.Navigate(typeof(CartPage));

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
