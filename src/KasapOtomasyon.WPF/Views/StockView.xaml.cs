using System.Windows.Controls;
using KasapOtomasyon.Domain.Entities;
using KasapOtomasyon.WPF.ViewModels;

namespace KasapOtomasyon.WPF.Views;

public partial class StockView : UserControl
{
    public StockView()
    {
        InitializeComponent();
    }

    private void WarehouseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is StockViewModel vm && sender is ComboBox cb && cb.SelectedItem is Warehouse wh)
        {
            _ = vm.FilterWarehouseAsync(wh);
        }
    }
}
