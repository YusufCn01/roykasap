using System.Windows.Controls;
using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.WPF.ViewModels;

namespace KasapOtomasyon.WPF.Views;

public partial class FinanceView : UserControl
{
    public FinanceView()
    {
        InitializeComponent();
    }

    private void CustomerDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is FinanceViewModel vm && sender is DataGrid dg && dg.SelectedItem is CustomerDto cust)
        {
            _ = vm.SelectCustomerAsync(cust);
        }
    }
}
