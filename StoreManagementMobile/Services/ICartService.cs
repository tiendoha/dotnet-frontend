using StoreManagementMobile.Models;

namespace StoreManagementMobile.Services;

public interface ICartService
{
    Task AddItemAsync(Product product, int quantity);
    Task<List<CartItem>> GetItemsAsync();
    Task UpdateQuantityAsync(int cartItemId, int newQuantity);
    Task RemoveItemAsync(int cartItemId);
    Task ClearAsync();
}
