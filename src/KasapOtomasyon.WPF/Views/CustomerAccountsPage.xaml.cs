using System.Windows.Controls;
using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.WPF.ViewModels;

namespace KasapOtomasyon.WPF.Views;

public partial class CustomerAccountsPage : UserControl
{
    public CustomerAccountsPage()
    {
        InitializeComponent();
    }

    private void CustomerGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is CustomerAccountsViewModel vm && sender is DataGrid dg && dg.SelectedItem is CustomerDto cust)
        {
            _ = vm.SelectCustomerAsync(cust);
        }
    }
}
