public class ProductPageResponse
{
    public List<ProductResponse> Items { get; set; } = new List<ProductResponse>();
    public int TotalPages { get; set; }
}
