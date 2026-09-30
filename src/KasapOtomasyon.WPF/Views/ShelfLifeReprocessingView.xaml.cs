using System.Windows.Controls;
using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.WPF.ViewModels;

namespace KasapOtomasyon.WPF.Views;

public partial class ShelfLifeReprocessingView : UserControl
{
    public ShelfLifeReprocessingView()
    {
        InitializeComponent();
    }

    private void BatchesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is ShelfLifeReprocessingViewModel vm && sender is DataGrid grid)
        {
            if (grid.SelectedItem is ExpiringBatchDto batch)
            {
                _ = vm.SelectBatch(batch);
            }
        }
    }

    private void RecipesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is ShelfLifeReprocessingViewModel vm && sender is ListBox lb)
        {
            if (lb.SelectedItem is ProcessingRecipeDto recipe)
            {
                _ = vm.SelectRecipe(recipe);
            }
        }
    }

    private void RawMeatQuantityTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (DataContext is ShelfLifeReprocessingViewModel vm)
        {
            _ = vm.RecalculateSimulationAsync();
        }
    }
}
