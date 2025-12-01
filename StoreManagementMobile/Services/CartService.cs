using Microsoft.EntityFrameworkCore;
using StoreManagementMobile.Models;
using StoreManagementMobile.Services.LocalDb;

namespace StoreManagementMobile.Services;

public class CartService : ICartService
{
    private readonly AppDbContext _db;

    public CartService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CartItem>> GetItemsAsync()
    {
        return await _db.CartItems
            .Where(x => x.UserId == App.UserId)
            .ToListAsync();
    }

    public async Task AddItemAsync(Product product, int quantity)
    {
        var existing = await _db.CartItems
            .FirstOrDefaultAsync(x => x.ProductId == product.ProductId && x.UserId == App.UserId);

        if (existing == null)
        {
            await _db.CartItems.AddAsync(new CartItem
            {
                UserId = App.UserId,
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                ImagePath = product.ImagePath,
                Price = product.Price,
                Quantity = quantity
            });
        }
        else
        {
            existing.Quantity += quantity;
        }

        await _db.SaveChangesAsync();
    }

    public async Task UpdateQuantityAsync(int productId, int newQuantity)
    {
        var item = await _db.CartItems
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.UserId == App.UserId);
    
        if (item == null) return;
    
        item.Quantity = newQuantity;
        await _db.SaveChangesAsync();
    }


    public async Task RemoveItemAsync(int productId)
    {
        var item = await _db.CartItems
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.UserId == App.UserId);
    
        if (item == null) return;
    
        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();
    }


    public async Task ClearAsync()
    {
        var items = _db.CartItems.Where(x => x.UserId == App.UserId);
        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync();
    }
}
