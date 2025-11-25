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

    // -----------------------------
    // ⭐ UserId: dùng cho Cart local
    // -----------------------------
    public static int UserId { get; set; } = 1;  
    // ⚠ LƯU Ý:
    // Khi LoginPage làm xong, bạn sẽ thay thế dòng trên bằng:
    // App.UserId = loginResponse.data.userId;

    private Window? _mainWindow;

    public App()
    {
        this.InitializeComponent();

        Debug.WriteLine("🔥 App(): Constructor chạy");
        Console.WriteLine("🔥 App(): Constructor chạy");
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Debug.WriteLine("🚀 OnLaunched bắt đầu");
        Console.WriteLine("🚀 OnLaunched bắt đầu");

        try
        {
            //
            // ============================
            // 1. Build Host
            // ============================
            //
            Debug.WriteLine("🏗 Bắt đầu tạo Host...");
            Console.WriteLine("🏗 Bắt đầu tạo Host...");

            var builder = this.CreateBuilder(args)
                .Configure(host => host
#if DEBUG
                    .UseEnvironment(Environments.Development)
#endif
                    .ConfigureServices((context, services) =>
                    {
                        Debug.WriteLine("🔧 Đang đăng ký DI Services...");
                        Console.WriteLine("🔧 Đang đăng ký DI Services...");

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
            Console.WriteLine("✅ Host build thành công");

            //
            // ============================
            // 2. SQLite Create DB
            // ============================
            //
            using (var scope = Host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                Debug.WriteLine("📦 EnsureCreated() database...");
                Console.WriteLine("📦 EnsureCreated() database...");

                db.Database.EnsureCreated();

                // ⭐ SEED TEST CART DỮ LIỆU GIẢ CHO userId = 1
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

            //
            // ============================
            // 3. Lấy Window hiện tại
            // ============================
            //
            Debug.WriteLine("🪟 Đang lấy Window.Current…");
            Console.WriteLine("🪟 Đang lấy Window.Current…");

            var window = Window.Current;

            if (window == null)
            {
                Debug.WriteLine("⚠ Window.Current == null → tạo mới");
                Console.WriteLine("⚠ Window.Current == null → tạo mới");
                window = new Window();
            }
            else
            {
                Debug.WriteLine("✅ Window.Current lấy thành công");
                Console.WriteLine("✅ Window.Current lấy thành công");
            }

            _mainWindow = window;

            //
            // ============================
            // 4. Tạo Frame root nếu cần
            // ============================
            //
            var rootFrame = window.Content as Frame;

            if (rootFrame == null)
            {
                Debug.WriteLine("📄 rootFrame == null → tạo Frame mới");
                Console.WriteLine("📄 rootFrame == null → tạo Frame mới");

                rootFrame = new Frame();
                window.Content = rootFrame;
            }

            //
            // ============================
            // 5. Điều hướng CartPage để test
            // ============================
            //
            Debug.WriteLine("➡ Bắt đầu điều hướng vào CartPage...");
            Console.WriteLine("➡ Bắt đầu điều hướng vào CartPage...");

            var result = rootFrame.Navigate(typeof(CartPage));

            Debug.WriteLine(result
                ? "✅ Navigate CartPage thành công"
                : "❌ Navigate CartPage thất bại");

            //
            // ============================
            // 6. Kích hoạt Window
            // ============================
            //
            window.Activate();

            Debug.WriteLine("🚀 OnLaunched kết thúc OK");
            Console.WriteLine("🚀 OnLaunched kết thúc OK");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"💥 Lỗi trong OnLaunched: {ex}");
            Console.WriteLine($"💥 Lỗi trong OnLaunched: {ex}");
        }
    }
}
