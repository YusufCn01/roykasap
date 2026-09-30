using System.Windows.Controls;
using KasapOtomasyon.WPF.ViewModels;

namespace KasapOtomasyon.WPF.Views;

public partial class ProductionView : UserControl
{
    public ProductionView()
    {
        InitializeComponent();
    }

    private void LotComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is ProductionViewModel vm)
        {
            vm.ApplyTemplateToSelectedLot();
        }
    }

    private void TemplateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is ProductionViewModel vm)
        {
            vm.ApplyTemplateToSelectedLot();
        }
    }

    private void WeightTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (DataContext is ProductionViewModel vm)
        {
            vm.RecalculateSimulatedYield();
        }
    }
}
