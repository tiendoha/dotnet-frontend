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
        // Giỏ hàng chung cho tất cả user - không lọc theo UserId
        return await _db.CartItems.ToListAsync();
    }

    public async Task AddItemAsync(Product product, int quantity)
    {
        // Tìm sản phẩm theo ProductId - không cần UserId
        var existing = await _db.CartItems
            .FirstOrDefaultAsync(x => x.ProductId == product.ProductId);

        if (existing == null)
        {
            await _db.CartItems.AddAsync(new CartItem
            {
                UserId = 0, // Giỏ hàng chung - không phân biệt user
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
        // Tìm theo ProductId - không cần UserId
        var item = await _db.CartItems
            .FirstOrDefaultAsync(x => x.ProductId == productId);
    
        if (item == null) return;
    
        item.Quantity = newQuantity;
        await _db.SaveChangesAsync();
    }


    public async Task RemoveItemAsync(int productId)
    {
        // Tìm theo ProductId - không cần UserId
        var item = await _db.CartItems
            .FirstOrDefaultAsync(x => x.ProductId == productId);
    
        if (item == null) return;
    
        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();
    }


    public async Task ClearAsync()
    {
        // Xóa toàn bộ giỏ hàng - không phân biệt user
        var items = _db.CartItems;
        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync();
    }
}
