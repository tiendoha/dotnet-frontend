namespace StoreManagementMobile.Models;

public class PromotionListData
{
    public List<Promotion> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
