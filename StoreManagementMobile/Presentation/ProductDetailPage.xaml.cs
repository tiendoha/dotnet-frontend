using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using StoreManagementMobile.Models; // Giả sử ProductResponse ở đây

namespace StoreManagementMobile.Presentation
{
    public sealed partial class ProductDetailPage : Page
    {
        // ... (Giữ nguyên phần này)
        public ProductDetailViewModel ViewModel { get; private set; }

        public ProductDetailPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ProductResponse product)
            {
                
              
                var frame = this.Frame;

               
                 frame?.Navigate(typeof(ProductListPage), product);
            }
        }
    }
}