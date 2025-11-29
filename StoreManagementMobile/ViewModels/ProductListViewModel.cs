using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input; // 🔥 THÊM: Để tạo Command cho ImmediateSearchAsync
using StoreManagementMobile.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using System;
using System.Threading;

namespace StoreManagementMobile.Presentation
{
    public partial class ProductListViewModel : ObservableObject
    {
        private readonly HttpClient _http = new HttpClient();
        
        // Đã sửa: Đổi giá trị từ localhost sang 10.0.2.2 để hoạt động trên Emulator
        private string API_IMAGE = "http://10.0.2.2:5000"; 

        // Thêm: Để xử lý Debounce cho chức năng tìm kiếm
        private CancellationTokenSource _searchCts; 
        // 🔥 ĐÃ SỬA: Debounce tiêu chuẩn là 500ms (0.5 giây)
        private const int SEARCH_DEBOUNCE_MS = 500; 
        private CancellationTokenSource _debounceCts;
        private CancellationTokenSource _immediateCts;

        [ObservableProperty]
        private ObservableCollection<CategoryResponse> _categories = new();

        [ObservableProperty]
        private int _selectedCategoryId = 0;

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; } = 1;

        public string SortBy { get; set; } = string.Empty;
        public bool SortDesc { get; set; } = false;

        
        private Dictionary<int, string> _categoryNameMap = new();
        // Giữ lại mẫu product cho đến khi load API xong
        private readonly List<ProductResponse> _fullProductList; 

        [ObservableProperty]
        private ObservableCollection<ProductResponse> _items = new ObservableCollection<ProductResponse>();

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public ProductListViewModel()
        {
            // Dữ liệu mẫu (chỉ dùng tạm, LoadProductsAsync sẽ ghi đè)
            _fullProductList = CreateSampleProducts(); 
            Items = new ObservableCollection<ProductResponse>(_fullProductList);
            Task.Run(async () =>
            {
                await LoadCategoriesAsync();
                await LoadProductsAsync(); 
            });
        }

        private void EnsureAbsoluteImageUrl(ProductResponse product)
        {
            if (!string.IsNullOrEmpty(product.ImageUrl) && product.ImageUrl.StartsWith("/"))
            {
                product.ImageUrl = $"{API_IMAGE}{product.ImageUrl}";
            }
        }

        private List<ProductResponse> CreateSampleProducts()
        {
            return new List<ProductResponse>
            {
                new ProductResponse { ProductId = 1, ProductName = "Coca Cola lon 330ml", Price = 31483.38m, Unit = "Thùng", ImageUrl = $"{API_IMAGE}/images/products/product_1.jpg", CategoryId = 1 },
                new ProductResponse { ProductId = 10, ProductName = "Socola KitKat Gói Lớn", Price = 139959.00m, Unit = "Gói", ImageUrl = $"{API_IMAGE}/images/products/product_10.jpg", CategoryId = 2 },
                new ProductResponse { ProductId = 11, ProductName = "Nước Mắm Nam Ngư 500ml", Price = 51792.00m, Unit = "Chai", ImageUrl = $"{API_IMAGE}/images/products/product_11.jpg", CategoryId = 3 },
                new ProductResponse { ProductId = 12, ProductName = "Bia Heineken lon", Price = 450000m, Unit = "Thùng", ImageUrl = $"{API_IMAGE}/images/products/product_1.jpg", CategoryId = 1 },
                new ProductResponse { ProductId = 13, ProductName = "Kẹo Alpenliebe", Price = 35000m, Unit = "Gói", ImageUrl = $"{API_IMAGE}/images/products/product_10.jpg", CategoryId = 2 },
                new ProductResponse { ProductId = 14, ProductName = "Dầu Ăn Tường An", Price = 80000m, Unit = "Chai", ImageUrl = $"{API_IMAGE}/images/products/product_11.jpg", CategoryId = 3 },
            };
        }

        // -------------------------------
        // BUILD URL API
        // -------------------------------
        private string BuildApiUrl()
        {
            var baseUrl = $"{API_IMAGE}/api/Products";
            var url = $"{baseUrl}?pageNumber={PageNumber}&pageSize={PageSize}";

            // Sort
            if (!string.IsNullOrEmpty(SortBy))
            {
                url += $"&sortBy={SortBy}&sortDesc={SortDesc.ToString().ToLower()}";
            }

            // Category
            if (SelectedCategoryId > 0)
                url += $"&categoryId={SelectedCategoryId}";
                
            // Search
            if (!string.IsNullOrWhiteSpace(SearchQuery))
                // SỬA: Đổi từ searchQuery thành searchTerm theo API
                url += $"&searchTerm={Uri.EscapeDataString(SearchQuery.Trim())}";


            return url;
        }


        // -------------------------------
        // LOAD 1 TRANG (ĐÃ ÁP DỤNG CancellationToken)
        // -------------------------------
        public async Task LoadProductsAsync(CancellationToken cancellationToken = default)
        {
            // Tránh chạy nhiều lần cùng lúc
            if (IsLoading) return;

            _errorMessage = string.Empty;

            try
            {
                IsLoading = true;
                cancellationToken.ThrowIfCancellationRequested();

                // 1. Dựng URL API
                string url = BuildApiUrl();
                Debug.WriteLine($"[API_CALL] Loading products from: {url}");

                // 2. Gọi API để lấy dữ liệu JSON (truyền Cancellation Token vào)
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                using var response = await _http.SendAsync(request, cancellationToken);
                
                // Nếu bị hủy, nó sẽ chuyển sang catch OperationCanceledException
                cancellationToken.ThrowIfCancellationRequested(); 

                response.EnsureSuccessStatusCode(); 
                var json = await response.Content.ReadAsStringAsync(cancellationToken);


                // 3. Deserialize dữ liệu JSON
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<PagedResult<ProductResponse>>>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                // 4. Xóa danh sách cũ và cập nhật danh sách mới
                // Chỉ xóa nếu là tải trang 1 (không phải LoadMore)
                if (PageNumber == 1)
                {
                    Items.Clear();
                }

                if (apiResponse?.Data?.Items != null)
                {
                    foreach (var p in apiResponse.Data.Items)
                    {
                        EnsureAbsoluteImageUrl(p); 
                        MapCategoryName(p); 
                        Items.Add(p);
                    }

                    // Cập nhật số trang dựa vào API trả về
                    TotalPages = apiResponse.Data.TotalPages;

                    if (Items.Count == 0 && PageNumber == 1)
                    {
                        _errorMessage = "Không tìm thấy sản phẩm nào phù hợp.";
                    }
                }
                else if (PageNumber == 1)
                {
                    _errorMessage = "Không tìm thấy dữ liệu sản phẩm.";
                    TotalPages = 1;
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("[INFO] LoadProductsAsync was canceled.");
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[HTTP_ERROR] LoadProductsAsync failed: {ex.Message} Status: {ex.StatusCode}");
                _errorMessage = $"Lỗi kết nối máy chủ. Vui lòng kiểm tra đường dẫn API.";
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"[JSON_ERROR] LoadProductsAsync failed to parse JSON: {ex.Message}");
                _errorMessage = $"Lỗi định dạng dữ liệu trả về từ máy chủ.";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GENERAL_ERROR] LoadProductsAsync failed: {ex.Message}");
                _errorMessage = $"Đã xảy ra lỗi không xác định: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }


        // -------------------------------
        // REFRESH = LOAD LẠI TRANG 1
        // -------------------------------
        public async Task RefreshProducts()
        {
            // Hủy debounce đang chờ nếu có
            _searchCts?.Cancel();
            _searchCts?.Dispose();
            _searchCts = new CancellationTokenSource();

            PageNumber = 1;
            await LoadProductsAsync();
        }

        // -------------------------------
        // FILTER CATEGORY (TẢI LẠI TRANG 1)
        // -------------------------------
        public async Task ApplyCategoryFilter()
        {
            PageNumber = 1;
            await LoadProductsAsync();
        }

        // -------------------------------
        // SORT (UI gọi)
        // -------------------------------
        public async Task ApplySortingAsync(string sortField, bool desc)
        {
            SortBy = sortField;
            SortDesc = desc;
            PageNumber = 1;
            await LoadProductsAsync();
        }

        // -------------------------------
        // LOAD THÊM TRANG (Infinite Scroll)
        // -------------------------------
        public async Task LoadMoreProductsAsync()
        {
            // Sử dụng LoadProductsAsync để tái sử dụng logic xử lý lỗi và token hủy
            if (IsLoading || PageNumber >= TotalPages) return;
            
            // Tăng PageNumber trước khi gọi LoadProductsAsync
            PageNumber++;

            // Không cần CancellationToken ở đây vì nó không phải là search debounce
            await LoadProductsAsync();
            
            // Xử lý giảm PageNumber nếu có lỗi (đã làm trong LoadProductsAsync)
        }

        private void MapCategoryName(ProductResponse product)
        {
            if (_categoryNameMap.TryGetValue(product.CategoryId, out string name))
            {
                product.CategoryName = name;
            }
            else
            {
                product.CategoryName = "Không rõ";
            }
        }

        public async Task LoadCategoriesAsync()
        {
            try
            {
                string url = $"{API_IMAGE}/api/Categories?PageNumber=1&PageSize=100&sortDesc=false"; 
                var json = await _http.GetStringAsync(url);

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<PagedResult<CategoryResponse>>>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                Categories.Clear();
                _categoryNameMap.Clear();
                
                Categories.Add(new CategoryResponse { CategoryId = 0, CategoryName = "Tất cả" });
                _categoryNameMap.Add(0, "Tất cả");

                if (apiResponse?.Data?.Items != null)
                {
                    foreach (var c in apiResponse.Data.Items)
                    {
                        Categories.Add(c);
                        if (c.CategoryId > 0)
                        {
                            _categoryNameMap.TryAdd(c.CategoryId, c.CategoryName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CATEGORY_ERROR] Failed to load categories: {ex.Message}");
            }
        } 

        partial void OnSelectedCategoryIdChanged(int value)
        {
            // Hủy debounce tìm kiếm cũ (nếu có) khi người dùng đổi Category
            _searchCts?.Cancel(); 
            Task.Run(ApplyCategoryFilter);
        }

        // -------------------------------
        // 🔥 HÀM TÌM KIẾM NGAY LẬP TỨC (Khi nhấn Enter hoặc nút Search)
        // -------------------------------
       // Tự động tạo ImmediateSearchCommand
     [RelayCommand]
public async Task ImmediateSearchAsync()
{
    // Hủy token tìm kiếm tức thì cũ
    _immediateCts?.Cancel();
    _immediateCts?.Dispose();

    _immediateCts = new CancellationTokenSource();
    var token = _immediateCts.Token;

    PageNumber = 1;

    try
    {
        await LoadProductsAsync(token);
    }
    catch (OperationCanceledException)
    {
        Debug.WriteLine("[ImmediateSearch] Canceled");
    }
}



        // -------------------------------
        // HÀM TÌM KIẾM (Debounce khi đang gõ)
        // -------------------------------
       partial void OnSearchQueryChanged(string value)
{
    // Nếu SearchQuery rỗng → refresh nhanh
    if (string.IsNullOrWhiteSpace(value))
    {
        Task.Run(async () =>
        {
            PageNumber = 1;
            await LoadProductsAsync();
        });
        return;
    }

    // Hủy debounce cũ
    _debounceCts?.Cancel();
    _debounceCts?.Dispose();

    _debounceCts = new CancellationTokenSource();
    var token = _debounceCts.Token;

    Task.Run(async () =>
    {
        try
        {
            // Delay 500ms (debounce)
            await Task.Delay(SEARCH_DEBOUNCE_MS, token);

            PageNumber = 1;
            await LoadProductsAsync(token);
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine("[Debounce] Canceled");
        }
    });
}
}

}