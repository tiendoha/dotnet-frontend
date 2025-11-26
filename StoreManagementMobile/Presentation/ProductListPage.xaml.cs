using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace StoreManagementMobile.Presentation;

public sealed partial class ProductListPage : Page
{
    public ProductListViewModel ViewModel { get; } = new ProductListViewModel();

    public ProductListPage()
    {
        this.InitializeComponent();
        DataContext = ViewModel;
        _ = ViewModel.LoadProductsAsync(); // async void → dùng _ = ...
    }

    private void ApplySort_Click(object sender, RoutedEventArgs e)
    {
        if (SortOptions.SelectedItem is RadioButton item && item.Tag is string tag)
        {
            var parts = tag.Split('|');
            string sortField = parts[0];
            bool desc = bool.Parse(parts[1]);
            ViewModel.ApplySorting(sortField, desc);
        }

        FilterSortFlyout.Hide();
    }

    private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
    {
        var sv = sender as ScrollViewer;
        if (!e.IsIntermediate && sv.VerticalOffset >= sv.ScrollableHeight - 100)
        {
            _ = ViewModel.LoadMoreProductsAsync();
        }
    }
}
