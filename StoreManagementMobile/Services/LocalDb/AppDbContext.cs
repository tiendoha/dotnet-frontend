using Microsoft.EntityFrameworkCore;
using StoreManagementMobile.Models;
using System.IO;

namespace StoreManagementMobile.Services.LocalDb;

public class AppDbContext : DbContext
{
    public DbSet<CartItem> CartItems => Set<CartItem>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = GetDatabasePath();
        EnsureDatabaseFolderExists(dbPath);

        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    // 🔥 Tạo thư mục nếu chưa có (Fix lỗi Android không tạo DB)
    private void EnsureDatabaseFolderExists(string dbPath)
    {
        var folder = Path.GetDirectoryName(dbPath);
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder!);
    }

    // 🔥 Lấy đúng đường dẫn SQLite cho từng platform
    private string GetDatabasePath()
    {
        var fileName = "store_local.db3";

#if __ANDROID__
        string folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        );
#elif __IOS__
        string folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "..", "Library"
        );
#else
        string folder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#endif

        return Path.Combine(folder, fileName);
    }
}
