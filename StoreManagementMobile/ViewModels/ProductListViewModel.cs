using CommunityToolkit.Mvvm.ComponentModel;
using StoreManagementMobile.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using System; // Đã thêm System

namespace StoreManagementMobile.Presentation
{
    public partial class ProductListViewModel : ObservableObject
    {
        private readonly HttpClient _http = new HttpClient();
        
        // 🔥 ĐÃ SỬA: Đổi giá trị từ localhost sang 10.0.2.2 để hoạt động trên Emulator
        private string API_IMAGE = "http://10.0.2.2:5000"; 

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; } = 1;

        public string SortBy { get; set; } = string.Empty;
        public bool SortDesc { get; set; } = false;

        public int SelectedCategoryId { get; set; } = 0;

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
            _fullProductList = CreateSampleProducts();
            Items = new ObservableCollection<ProductResponse>(_fullProductList);
        }

        // 🔥 ĐÃ THÊM: Helper để đảm bảo ImageUrl là tuyệt đối bằng cách dùng API_IMAGE
        private void EnsureAbsoluteImageUrl(ProductResponse product)
        {
            // Nếu ImageUrl tồn tại và là đường dẫn tương đối (bắt đầu bằng '/'),
            // thì nối với API_IMAGE.
            if (!string.IsNullOrEmpty(product.ImageUrl) && product.ImageUrl.StartsWith("/"))
            {
                product.ImageUrl = $"{API_IMAGE}{product.ImageUrl}";
            }
        }

        // 🔥 ĐÃ SỬA: Dữ liệu mẫu dùng API_IMAGE
        private List<ProductResponse> CreateSampleProducts()
        {
            return new List<ProductResponse>
            {
                new ProductResponse { ProductId = 1, ProductName = "Coca Cola lon 330ml", Price = 31483.38m, Unit = "Thùng", ImageUrl = $"{API_IMAGE}/images/products/product_1.jpg" },
                new ProductResponse { ProductId = 10, ProductName = "Socola KitKat Gói Lớn", Price = 139959.00m, Unit = "Gói", ImageUrl = $"{API_IMAGE}/images/products/product_10.jpg" },
                new ProductResponse { ProductId = 11, ProductName = "Nước Mắm Nam Ngư 500ml", Price = 51792.00m, Unit = "Chai", ImageUrl = $"{API_IMAGE}/images/products/product_11.jpg" },
                new ProductResponse { ProductId = 10, ProductName = "Socola KitKat Gói Lớn", Price = 139959.00m, Unit = "Gói", ImageUrl = $"{API_IMAGE}/images/products/product_10.jpg" },
                new ProductResponse { ProductId = 10, ProductName = "Socola KitKat Gói Lớn", Price = 139959.00m, Unit = "Gói", ImageUrl = $"{API_IMAGE}/images/products/product_10.jpg" },
                new ProductResponse { ProductId = 10, ProductName = "Socola KitKat Gói Lớn", Price = 139959.00m, Unit = "Gói", ImageUrl = $"{API_IMAGE}/images/products/product_10.jpg" },
                new ProductResponse { ProductId = 10, ProductName = "Socola KitKat Gói Lớn", Price = 139959.00m, Unit = "Gói", ImageUrl = $"{API_IMAGE}/images/products/product_10.jpg" },
                new ProductResponse { ProductId = 10, ProductName = "Socola KitKat Gói Lớn", Price = 139959.00m, Unit = "Gói", ImageUrl = $"{API_IMAGE}/images/products/product_10.jpg" },
                new ProductResponse { ProductId = 10, ProductName = "Socola KitKat Gói Lớn", Price = 139959.00m, Unit = "Gói", ImageUrl = $"{API_IMAGE}/images/products/product_10.jpg" },
                new ProductResponse { ProductId = 10, ProductName = "Socola KitKat Gói Lớn", Price = 139959.00m, Unit = "Gói", ImageUrl = $"{API_IMAGE}/images/products/product_10.jpg" },
                new ProductResponse { ProductId = 10, ProductName = "Socola KitKat Gói Lớn", Price = 139959.00m, Unit = "Gói", ImageUrl = $"{API_IMAGE}/images/products/product_10.jpg" },
                new ProductResponse { ProductId = 10, ProductName = "Socola KitKat Gói Lớn", Price = 139959.00m, Unit = "Gói", ImageUrl = $"{API_IMAGE}/images/products/product_10.jpg" },
                new ProductResponse { ProductId = 10, ProductName = "Socola KitKat Gói Lớn", Price = 139959.00m, Unit = "Gói", ImageUrl = $"{API_IMAGE}/images/products/product_10.jpg" },
            };
        }

        // -------------------------------
        // 🔥 BUILD URL ĐÚNG API (ĐÃ SỬ DỤNG API_IMAGE)
        // -------------------------------
        private string BuildApiUrl()
        {
            var baseUrl = $"{API_IMAGE}/api/Products"; // 🔥 SỬA: Dùng API_IMAGE
            var url = $"{baseUrl}?pageNumber={PageNumber}&pageSize={PageSize}";

            // Sort
            if (!string.IsNullOrEmpty(SortBy))
            {
                url += $"&sortBy={SortBy}&sortDesc={SortDesc.ToString().ToLower()}";
            }

            // Category
            if (SelectedCategoryId > 0)
                url += $"&categoryId={SelectedCategoryId}";

            return url;
        }


        // -------------------------------
        // 🔥 LOAD 1 TRANG (ĐÃ ÁP DỤNG FIX IMAGE URL)
        // -------------------------------
        public async Task LoadProductsAsync()
        {
            // Ngăn chặn việc gọi API nếu đang load
            if (IsLoading) return;

            // Xóa thông báo lỗi cũ và reset trang về 1
            _errorMessage = string.Empty;
            PageNumber = 1;

            try
            {
                IsLoading = true;

                // 1. Dựng URL API
                string url = BuildApiUrl();

                // 2. Gọi API để lấy dữ liệu JSON
                var json = await _http.GetStringAsync(url);

                // 3. Deserialize dữ liệu JSON
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<PagedResult<ProductResponse>>>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                // 4. Xóa danh sách cũ và cập nhật danh sách mới
                Items.Clear();

                if (apiResponse?.Data?.Items != null)
                {
                    foreach (var p in apiResponse.Data.Items)
                    {
                        EnsureAbsoluteImageUrl(p); // 🔥 SỬ DỤNG API_IMAGE ĐỂ FIX URL ẢNH
                        Items.Add(p);
                    }

                    // Cập nhật số trang dựa vào API trả về
                    TotalPages = apiResponse.Data.TotalPages;

                    if (Items.Count == 0)
                    {
                        _errorMessage = "Không tìm thấy sản phẩm nào phù hợp.";
                    }
                }
                else
                {
                    _errorMessage = "Không tìm thấy dữ liệu sản phẩm.";
                    TotalPages = 1;
                }
            }
            catch (HttpRequestException ex)
            {
                // Ghi lỗi ra Logcat
                Debug.WriteLine($"[HTTP_ERROR] LoadProductsAsync failed: {ex.Message} Status: {ex.StatusCode}");
                // Hiển thị lỗi ra UI
                _errorMessage = $"Lỗi kết nối máy chủ ({ex.StatusCode}). Vui lòng kiểm tra đường dẫn API.";
            }
            catch (JsonException ex)
            {
                // Ghi lỗi ra Logcat
                Debug.WriteLine($"[JSON_ERROR] LoadProductsAsync failed to parse JSON: {ex.Message}");
                // Hiển thị lỗi ra UI
                _errorMessage = $"Lỗi định dạng dữ liệu trả về từ máy chủ.";
            }
            catch (Exception ex)
            {
                // Ghi lỗi ra Logcat
                Debug.WriteLine($"[GENERAL_ERROR] LoadProductsAsync failed: {ex.Message}");
                // Hiển thị lỗi ra UI
                _errorMessage = $"Đã xảy ra lỗi không xác định: {ex.Message}";
            }
            finally
            {
                // 5. Kết thúc quá trình loading
                IsLoading = false;
            }
        }


        // -------------------------------
        // 🔥 REFRESH = LOAD LẠI
        // -------------------------------
        public async Task RefreshProducts()
        {
            await LoadProductsAsync();
        }

        // -------------------------------
        // 🔥 FILTER CATEGORY
        // -------------------------------
        public async Task ApplyCategoryFilter()
        {
            PageNumber = 1;
            await LoadProductsAsync();
        }

        // -------------------------------
        // 🔥 SORT (UI gọi)
        // -------------------------------
        public async Task ApplySortingAsync(string sortField, bool desc)
        {
            SortBy = sortField;
            SortDesc = desc;
            PageNumber = 1;
            await LoadProductsAsync();
        }

        // -------------------------------
        // 🔥 LOAD THÊM TRANG (ĐÃ ÁP DỤNG FIX IMAGE URL)
        // -------------------------------
        public async Task LoadMoreProductsAsync()
        {
            if (IsLoading || PageNumber >= TotalPages) return;

            // 🔥 SỬA: Dùng _errorMessage
            _errorMessage = string.Empty;

            try
            {
                IsLoading = true;
                PageNumber++;

                string url = BuildApiUrl();
                var json = await _http.GetStringAsync(url);

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<PagedResult<ProductResponse>>>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (apiResponse?.Data?.Items != null)
                {
                    foreach (var p in apiResponse.Data.Items)
                    {
                        EnsureAbsoluteImageUrl(p); // 🔥 SỬ DỤNG API_IMAGE ĐỂ FIX URL ẢNH
                        Items.Add(p);
                    }
                }
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is JsonException)
            {
                PageNumber--; // Quay lại trang cũ

                // 🔥 ĐÃ THÊM: Ghi lỗi ra Logcat
                Debug.WriteLine($"[LOAD_MORE_ERROR] Failed to load page {PageNumber + 1}: {ex.GetType().Name} - {ex.Message}");

                if (ex is HttpRequestException)
                {
                    // 🔥 SỬA: Dùng _errorMessage
                    _errorMessage = "Lỗi kết nối khi tải thêm. Vui lòng thử lại.";
                }
                else if (ex is JsonException)
                {
                    // 🔥 SỬA: Dùng _errorMessage
                    _errorMessage = "Lỗi dữ liệu khi tải thêm trang.";
                }
            }
            catch (Exception ex)
            {
                PageNumber--;
                // 🔥 ĐÃ THÊM: Ghi lỗi ra Logcat
                Debug.WriteLine($"[LOAD_MORE_GENERAL_ERROR]: {ex.Message}");

                // 🔥 SỬA: Dùng _errorMessage
                _errorMessage = $"Lỗi không xác định khi tải thêm: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }


        // -------------------------------
        // 🔥 SEARCH LOCAL (LIVE)
        // -------------------------------
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