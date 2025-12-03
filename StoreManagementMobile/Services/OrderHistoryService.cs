using Microsoft.EntityFrameworkCore;
using StoreManagementMobile.Models;
using StoreManagementMobile.Services.LocalDb;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StoreManagementMobile.Services;

public class OrderHistoryService : IOrderHistoryService
{
    private readonly AppDbContext _db;

    public OrderHistoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<OrderHistory>> GetAllOrdersAsync()
    {
        return await _db.OrderHistories
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<OrderHistory?> GetOrderByIdAsync(int orderId)
    {
        return await _db.OrderHistories
            .FirstOrDefaultAsync(o => o.OrderId == orderId);
    }

    public async Task AddOrderAsync(OrderHistory order)
    {
        _db.OrderHistories.Add(order);
        await _db.SaveChangesAsync();
    }

    public async Task ClearAllAsync()
    {
        _db.OrderHistories.RemoveRange(_db.OrderHistories);
        await _db.SaveChangesAsync();
    }
}
