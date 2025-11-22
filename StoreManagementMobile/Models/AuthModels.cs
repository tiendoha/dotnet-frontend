namespace StoreManagementMobile.Models;

public class BackendResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; } // Thêm ?
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty; // Gán mặc định
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserInfo? User { get; set; } // Có thể null
}

public class UserInfo
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}