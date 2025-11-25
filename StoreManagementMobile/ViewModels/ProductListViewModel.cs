using CommunityToolkit.Mvvm.ComponentModel;
using StoreManagementMobile.Models; 
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StoreManagementMobile.Presentation
{
    public partial class ProductListViewModel : ObservableObject
    {
        private readonly List<ProductResponse> _fullProductList;

        [ObservableProperty]
        private ObservableCollection<ProductResponse> _items = new ObservableCollection<ProductResponse>();

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        public ProductListViewModel()
        {
            _fullProductList = CreateSampleProducts();
            Items = new ObservableCollection<ProductResponse>(_fullProductList);
        }

        private List<ProductResponse> CreateSampleProducts()
        {
            // SỬA: Đã đổi tên thuộc tính từ ImagePath sang ImageUrl để khớp với XAML
            return new List<ProductResponse>
            {
                new ProductResponse 
                { 
                    ProductId = 1, ProductName = "Coca Cola lon 330ml", 
                    Price = 31483.38m, Unit = "Thùng", 
                    ImageUrl = "/images/products/product_1.jpg" // ĐÃ SỬA
                },
                new ProductResponse 
                { 
                    ProductId = 10, ProductName = "Socola KitKat Gói Lớn", 
                    Price = 139959.00m, Unit = "Gói", 
                    ImageUrl = "/images/products/product_10.jpg" // ĐÃ SỬA
                },
                new ProductResponse 
                { 
                    ProductId = 11, ProductName = "Nước Mắm Nam Ngư 500ml", 
                    Price = 51792.00m, Unit = "Chai", 
                    ImageUrl = "/images/products/product_11.jpg" // ĐÃ SỬA
                },
                new ProductResponse 
                { 
                    ProductId = 12, ProductName = "Nước Tương Maggi 300ml", 
                    Price = 462539.00m, Unit = "Lon", 
                    ImageUrl = "/images/products/product_12.jpg" // ĐÃ SỬA
                },
                new ProductResponse 
                { 
                    ProductId = 13, ProductName = "Muối I-ốt Bạc Liêu", 
                    Price = 173302.00m, Unit = "Cái", 
                    ImageUrl = "/images/products/product_13.jpg" // ĐÃ SỬA
                },
                new ProductResponse 
                { 
                    ProductId = 14, ProductName = "Bột Ngọt Ajinomoto 450g", 
                    Price = 443069.00m, Unit = "Cái", 
                    ImageUrl = "/images/products/product_14.jpg" // ĐÃ SỬA
                },
                new ProductResponse 
                { 
                    ProductId = 15, ProductName = "Dầu Ăn Tường An 1L", 
                    Price = 281354.00m, Unit = "Túyp", 
                    ImageUrl = "/images/products/product_15.jpg" // ĐÃ SỬA
                },
                new ProductResponse 
                { 
                    ProductId = 16, ProductName = "Nồi Cơm Điện Sharp 1.8L", 
                    Price = 405347.00m, Unit = "Hộp", 
                    ImageUrl = "/images/products/product_16.jpg" // ĐÃ SỬA
                },
                new ProductResponse 
                { 
                    ProductId = 17, ProductName = "Ấm Siêu Tốc Sunhouse", 
                    Price = 113087.00m, Unit = "Chai", 
                    ImageUrl = "/images/products/product_17.jpg" // ĐÃ SỬA
                },
            };
        }

        partial void OnSearchQueryChanged(string value)
        {
            var q = value?.Trim();

            if (string.IsNullOrWhiteSpace(q))
            {
                Items = new ObservableCollection<ProductResponse>(_fullProductList);
                return;
            }

            var filteredList = _fullProductList.Where(p => 
                (p.ProductName ?? string.Empty).Contains(q, System.StringComparison.OrdinalIgnoreCase) ||
                (p.Barcode ?? string.Empty).Contains(q, System.StringComparison.OrdinalIgnoreCase))
                .ToList();
                
            Items = new ObservableCollection<ProductResponse>(filteredList); 
        }
    }
}