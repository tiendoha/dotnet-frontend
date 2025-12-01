using Refit;
using StoreManagementMobile.Models;

namespace StoreManagementMobile.Services;

public interface IStoreApi
{
    [Post("/api/auth/login")]
    Task<BackendResponse<LoginResponse>> Login([Body] LoginRequest request);
    
    // ⭐ GET promotions
    [Get("/api/Promotion")]
    Task<BackendResponse<PromotionListData>> GetPromotions();
    
    // By-Code
    [Get("/api/Promotion/by-code/{code}")]
    Task<BackendResponse<Promotion>> GetPromotionByCode(string code);
    
    [Post("/api/orders")]
    Task<BackendResponse<OrderResponse>> CreateOrder([Body] CreateOrderRequest request);
    
    [Get("/api/Customer/{id}")]
    Task<BackendResponse<Customer>> GetCustomerById(int id);
    
}
