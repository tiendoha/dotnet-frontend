using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using StoreManagementMobile.Presentation;

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

        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            // TODO: navigate to product detail page
        }
    }
}
