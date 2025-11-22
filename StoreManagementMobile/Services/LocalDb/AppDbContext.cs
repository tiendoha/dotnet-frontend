using Microsoft.EntityFrameworkCore;
using System.IO;

namespace StoreManagementMobile.Services.LocalDb;

public class AppDbContext : DbContext
{

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = GetDatabasePath();
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    private string GetDatabasePath()
    {
        var fileName = "store_local.db3";
        string path = string.Empty;

        #if __ANDROID__
            path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        #elif __IOS__
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library");
        #else
            path = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
        #endif

        return Path.Combine(path, fileName);
    }
}