using System.Windows.Controls;
using System.Windows.Input;
using KasapOtomasyon.WPF.ViewModels;

namespace KasapOtomasyon.WPF.Views;

public partial class PosView : UserControl
{
    public PosView()
    {
        InitializeComponent();
    }

    private void BarcodeTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (DataContext is PosViewModel vm)
            {
                _ = vm.ProcessBarcodeAsync();
            }
        }
    }
}
