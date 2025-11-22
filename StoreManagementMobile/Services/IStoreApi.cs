using Refit;
using StoreManagementMobile.Models;

namespace StoreManagementMobile.Services;

public interface IStoreApi
{
    [Post("/api/auth/login")]
    Task<BackendResponse<LoginResponse>> Login([Body] LoginRequest request);
}