using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using StoreManagementMobile.Presentation;
using StoreManagementMobile.Models; // 🔥 CẦN ĐẢM BẢO MODEL NÀY ĐƯỢC THÊM

namespace StoreManagementMobile.Presentation
{
    public sealed partial class ProductListPage : Page
    {
        public ProductListViewModel ViewModel { get; set; } = new ProductListViewModel();

        public ProductListPage()
        {
            this.InitializeComponent();
            this.DataContext = ViewModel;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ViewModel.LoadProductsAsync();
        }

        private async void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var sv = sender as ScrollViewer;
            if (!e.IsIntermediate &&
                sv.VerticalOffset >= sv.ScrollableHeight - 50)
            {
                await ViewModel.LoadMoreProductsAsync();
            }
        }

        private async void ApplySort_Click(object sender, RoutedEventArgs e)
        {
            var selected = SortOptions.SelectedItem as RadioButton;
            if (selected == null) return;

            string[] parts = selected.Tag.ToString().Split('|');
            string sortBy = parts[0];
            bool desc = bool.Parse(parts[1]);

            ViewModel.SortBy = sortBy;
            ViewModel.SortDesc = desc;

            await ViewModel.LoadProductsAsync();

            // Hide flyout
            if (btnFilter.Flyout != null)
                btnFilter.Flyout.Hide();
        }

        // ----------------------------------------------------
        // 🔥 ĐÃ THÊM LOGIC CHUYỂN HƯỚNG SANG PRODUCT DETAIL PAGE
        // ----------------------------------------------------
        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            // 1. Kiểm tra sender có phải là Button không
            if (sender is Button button)
            {
                // 2. Lấy đối tượng ProductResponse từ DataContext của Button
                if (button.DataContext is ProductResponse selectedProduct)
                {
                    // 3. Điều hướng đến trang chi tiết, truyền đối tượng sản phẩm đi kèm
                    this.Frame.Navigate(typeof(ProductDetailPage), selectedProduct);
                }
            }
        }
    }
}