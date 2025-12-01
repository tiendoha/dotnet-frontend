public class CategoryApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    
    // 🔥 Sử dụng PagedResult để lấy phần "data"
    public PagedResult<CategoryResponse> Data { get; set; } 
}