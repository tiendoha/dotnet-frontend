using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using StoreManagementMobile.Presentation;
using StoreManagementMobile.Models; // Giả định ProductResponse nằm trong Models
using System.Threading.Tasks; // Cần thiết cho Task.Run và async/await
using System.Diagnostics; // Cần thiết cho Debug.WriteLine

namespace StoreManagementMobile.Presentation
{
    public sealed partial class ProductListPage : Page
    {
        // Giả định ProductListViewModel đã được định nghĩa và có các phương thức LoadProductsAsync, 
        // LoadCategoriesAsync, RefreshProducts, LoadMoreProductsAsync, ApplySortingAsync, ImmediateSearchAsync
        public ProductListViewModel ViewModel { get; set; } = new ProductListViewModel();

        public ProductListPage()
        {
            this.InitializeComponent();
            this.DataContext = ViewModel;

            // Thiết lập chế độ sắp xếp mặc định nếu SortOptions đã được đặt tên trong XAML 
            // và chứa các RadioButton (Giữ lại logic ban đầu của người dùng)
            // Tuy nhiên, việc này nên được đảm bảo trong XAML (SelectedItem) hoặc ViewModel.
            // Nếu SortOptions.SelectedItem được set trong XAML, nó sẽ được áp dụng.
            if (SortOptions.SelectedItem is RadioButton initialRadioButton)
            {
                ApplySortFromTag(initialRadioButton.Tag.ToString());
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            // Đảm bảo tất cả các thao tác load dữ liệu ban đầu được chạy
            await ViewModel.LoadProductsAsync();
            await ViewModel.LoadCategoriesAsync();
            // Việc RefreshProducts có thể không cần thiết nếu LoadProductsAsync đã tải lần đầu
            // Nhưng giữ lại theo yêu cầu của code gốc
            await ViewModel.RefreshProducts(); 
        }

        // -------------------------------
        // XỬ LÝ SEARCHBOX KHI NHẤN ENTER
        // -------------------------------
        private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true; 
                
                // Gọi hàm tìm kiếm ngay lập tức trong ViewModel
                // Chuyển sang Task.Run để tránh cảnh báo sync context nếu cần, nhưng lưu ý
                // mọi cập nhật UI phải được xử lý bên trong ViewModel hoặc Dispatcher.
                Task.Run(async () => await ViewModel.ImmediateSearchAsync());
            }
        }

        // -------------------------------
        // HÀM HỖ TRỢ XỬ LÝ TAG CỦA RADIOBUTTON
        // -------------------------------
        private void ApplySortFromTag(string tag)
        {
            // Tag có dạng "FieldName|bool_desc" (ví dụ: "Price|true")
            var parts = tag.Split('|');
            if (parts.Length == 2 && bool.TryParse(parts[1], out bool sortDesc))
            {
                string sortBy = parts[0];
                
                // Cập nhật thuộc tính trong ViewModel
                ViewModel.SortBy = sortBy;
                ViewModel.SortDesc = sortDesc;

                // Gọi hàm áp dụng sắp xếp và tải lại sản phẩm trong ViewModel
                // Dùng Task.Run để tránh deadlock nếu ViewModel.ApplySortingAsync chưa tối ưu.
                Task.Run(async () => await ViewModel.ApplySortingAsync(sortBy, sortDesc));
            }
        }
        private void SearchBox_KeyUp(object sender, KeyRoutedEventArgs e)
            {
                if (e.Key == Windows.System.VirtualKey.Enter)   
                {
                    Debug.WriteLine("ENTER pressed → ImmediateSearch");
                    ViewModel.ImmediateSearchCommand.Execute(null);
                }
            }

        // -------------------------------
        // 🔥 HÀM XỬ LÝ NHẤN NÚT ÁP DỤNG TRONG FLYOUT (Đã hợp nhất logic)
        // -------------------------------
        private void ApplySort_Click(object sender, RoutedEventArgs e)
        {
            // 1. Xử lý Sắp xếp (Sort)
            if (SortOptions.SelectedItem is RadioButton selectedRadioButton)
            {
                ApplySortFromTag(selectedRadioButton.Tag.ToString());
            }
            
            // 2. Việc lọc theo danh mục (ComboBox) đã được ViewModel xử lý
            // thông qua TwoWay Binding của SelectedCategoryId. Khi ApplySortFromTag được gọi,
            // nó sẽ kích hoạt lại LoadProductsAsync trong ViewModel, bao gồm cả CategoryId hiện tại.

            // 3. Đóng Flyout (Giả định FilterSortFlyout là tên control Flyout đã đặt trong XAML)
            if (FilterSortFlyout.IsOpen)
            {
                 FilterSortFlyout.Hide();
            }
        }

        // ----------------------------------------------------
        // 🔥 LOGIC CHUYỂN HƯỚNG SANG PRODUCT DETAIL PAGE
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
                    // Giả định ProductDetailPage đã được định nghĩa
                    this.Frame.Navigate(typeof(ProductDetailPage), selectedProduct);
                }
            }
        }

        // ----------------------------------------------------
        // XỬ LÝ LOAD MORE KHI CUỘN ĐẾN CUỐI (Đã hợp nhất và tối ưu)
        // ----------------------------------------------------
        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (e.IsIntermediate) return; // Chỉ xử lý khi quá trình cuộn kết thúc

            if (sender is ScrollViewer scrollViewer)
            {
                double verticalOffset = scrollViewer.VerticalOffset;
                double extentHeight = scrollViewer.ExtentHeight;
                double viewportHeight = scrollViewer.ViewportHeight;

                // Kiểm tra nếu người dùng cuộn đến 50px cuối cùng của ScrollableHeight
                if (!ViewModel.IsLoading && 
                    ViewModel.PageNumber < ViewModel.TotalPages &&
                    scrollViewer.ScrollableHeight > 0 && // Đảm bảo có thể cuộn
                    verticalOffset >= scrollViewer.ScrollableHeight - 50) 
                {
                    Debug.WriteLine("[INFO] Triggering Load More...");
                    
                    // Chạy LoadMoreProductsAsync trên background thread
                    // Quan trọng: ViewModel.LoadMoreProductsAsync phải xử lý cập nhật Products ObservableCollection 
                    // một cách an toàn (ví dụ: dùng Dispatcher Queue)
                    Task.Run(async () => await ViewModel.LoadMoreProductsAsync());
                }
            }
        }
    }
}