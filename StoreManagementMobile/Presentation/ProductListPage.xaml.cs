using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using StoreManagementMobile.Models;

namespace StoreManagementMobile.Presentation
{
    public sealed partial class ProductListPage : Page
    {
        // Giữ nguyên ViewModel khởi tạo thủ công để test dữ liệu mẫu
        public ProductListViewModel ViewModel { get; } = new ProductListViewModel(); 

        public ProductListPage()
        {
            this.InitializeComponent();
            DataContext = ViewModel; // Gán DataContext
        }
        
        private void ProductsGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
             if (e.ClickedItem is not ProductResponse prod) return;
             Frame.Navigate(typeof(ProductDetailPage), prod.ProductId);
        }
    }
}