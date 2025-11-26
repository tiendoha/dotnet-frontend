using CommunityToolkit.Mvvm.ComponentModel;
using StoreManagementMobile.Models; 
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace StoreManagementMobile.Presentation
{
    public partial class ProductListViewModel : ObservableObject
    {
        private readonly HttpClient _http = new HttpClient();

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; } = 1;

        public string SortBy { get; set; } = "";
        public bool SortDesc { get; set; } = false;

        private readonly List<ProductResponse> _fullProductList;

        [ObservableProperty]
        private ObservableCollection<ProductResponse> _items = new ObservableCollection<ProductResponse>();

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private bool _isLoading = false; // UI bind trực tiếp

        public ProductListViewModel()
        {
            _fullProductList = CreateSampleProducts();
            Items = new ObservableCollection<ProductResponse>(_fullProductList);
        }

        private List<ProductResponse> CreateSampleProducts()
        {
            return new List<ProductResponse>
            {
                new ProductResponse { ProductId = 1, ProductName = "Coca Cola lon 330ml", Price = 31483.38m, Unit = "Thùng", ImageUrl = "/images/products/product_1.jpg" },
                new ProductResponse { ProductId = 10, ProductName = "Socola KitKat Gói Lớn", Price = 139959.00m, Unit = "Gói", ImageUrl = "/images/products/product_10.jpg" },
                new ProductResponse { ProductId = 11, ProductName = "Nước Mắm Nam Ngư 500ml", Price = 51792.00m, Unit = "Chai", ImageUrl = "/images/products/product_11.jpg" },
                new ProductResponse { ProductId = 12, ProductName = "Nước Tương Maggi 300ml", Price = 462539.00m, Unit = "Lon", ImageUrl = "/images/products/product_12.jpg" },
                new ProductResponse { ProductId = 13, ProductName = "Muối I-ốt Bạc Liêu", Price = 173302.00m, Unit = "Cái", ImageUrl = "/images/products/product_13.jpg" },
                new ProductResponse { ProductId = 14, ProductName = "Bột Ngọt Ajinomoto 450g", Price = 443069.00m, Unit = "Cái", ImageUrl = "/images/products/product_14.jpg" },
                new ProductResponse { ProductId = 15, ProductName = "Dầu Ăn Tường An 1L", Price = 281354.00m, Unit = "Túyp", ImageUrl = "/images/products/product_15.jpg" },
                new ProductResponse { ProductId = 16, ProductName = "Nồi Cơm Điện Sharp 1.8L", Price = 405347.00m, Unit = "Hộp", ImageUrl = "/images/products/product_16.jpg" },
                new ProductResponse { ProductId = 17, ProductName = "Ấm Siêu Tốc Sunhouse", Price = 113087.00m, Unit = "Chai", ImageUrl = "/images/products/product_17.jpg" },
            };
        }

        public async Task LoadProductsAsync()
        {
            if (IsLoading) return;

            IsLoading = true;
            PageNumber = 1;

            string url = BuildApiUrl();
            var json = await _http.GetStringAsync(url);
            var data = JsonSerializer.Deserialize<ProductPageResponse>(json);

            Items.Clear();
            foreach (var p in data?.Items ?? new List<ProductResponse>())
                Items.Add(p);

            TotalPages = data?.TotalPages ?? 1;
            IsLoading = false;
        }

        private string BuildApiUrl()
        {
            var baseUrl = "https://localhost:5000/api/products";
            var url = $"{baseUrl}?pageNumber={PageNumber}&pageSize={PageSize}&status=Active";

            if (!string.IsNullOrEmpty(SearchQuery))
                url += $"&searchTerm={SearchQuery}";

            if (!string.IsNullOrEmpty(SortBy))
                url += $"&sortBy={SortBy}&sortDesc={SortDesc}";

            return url;
        }

        public async Task RefreshProducts()
        {
            await LoadProductsAsync();
        }

        public int SelectedCategoryId { get; set; } = 0;

        public void ApplyCategoryFilter()
        {
            Items = SelectedCategoryId == 0
                ? new ObservableCollection<ProductResponse>(_fullProductList)
                : new ObservableCollection<ProductResponse>(_fullProductList.Where(p => p.CategoryId == SelectedCategoryId));
        }

        public void ApplySorting(string sortField, bool desc)
        {
            var sorted = sortField.ToLower() switch
            {
                "name" => desc ? _fullProductList.OrderByDescending(p => p.ProductName).ToList() : _fullProductList.OrderBy(p => p.ProductName).ToList(),
                "price" => desc ? _fullProductList.OrderByDescending(p => p.Price).ToList() : _fullProductList.OrderBy(p => p.Price).ToList(),
                "createdat" => desc ? _fullProductList.OrderByDescending(p => p.CreatedAt).ToList() : _fullProductList.OrderBy(p => p.CreatedAt).ToList(),
                _ => _fullProductList
            };

            Items = new ObservableCollection<ProductResponse>(sorted);
        }

        public async Task LoadMoreProductsAsync()
        {
            if (IsLoading || PageNumber >= TotalPages) return;

            PageNumber++;
            IsLoading = true;

            string url = BuildApiUrl();
            var json = await _http.GetStringAsync(url);
            var data = JsonSerializer.Deserialize<ProductPageResponse>(json);

            foreach (var p in data?.Items ?? new List<ProductResponse>())
                Items.Add(p);

            IsLoading = false;
        }

        partial void OnSearchQueryChanged(string value)
        {
            var q = value?.Trim();
            if (string.IsNullOrWhiteSpace(q))
            {
                Items = new ObservableCollection<ProductResponse>(_fullProductList);
                return;
            }

            Items = new ObservableCollection<ProductResponse>(
                _fullProductList.Where(p =>
                    (p.ProductName ?? string.Empty).Contains(q, System.StringComparison.OrdinalIgnoreCase) ||
                    (p.Barcode ?? string.Empty).Contains(q, System.StringComparison.OrdinalIgnoreCase))
            );
        }
    }
}
