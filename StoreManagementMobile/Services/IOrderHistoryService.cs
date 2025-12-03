using StoreManagementMobile.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StoreManagementMobile.Services;

public interface IOrderHistoryService
{
    Task<List<OrderHistory>> GetAllOrdersAsync();
    Task<OrderHistory?> GetOrderByIdAsync(int orderId);
    Task AddOrderAsync(OrderHistory order);
    Task ClearAllAsync();
}
