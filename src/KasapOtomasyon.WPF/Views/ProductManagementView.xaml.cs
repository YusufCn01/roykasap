using System.Windows.Controls;
using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.WPF.ViewModels;

namespace KasapOtomasyon.WPF.Views;

public partial class ProductManagementView : UserControl
{
    public ProductManagementView()
    {
        InitializeComponent();
    }

    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is ProductManagementViewModel vm && sender is DataGrid grid && grid.SelectedItem is ProductDto prod)
        {
            vm.SelectProductCommand.Execute(prod);
        }
    }
}
