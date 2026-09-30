using System.Windows;
using System.Windows.Input;
using KasapOtomasyon.Setup.ViewModels;

namespace KasapOtomasyon.Setup;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        if (DataContext is SetupWizardViewModel vm)
        {
            vm.CloseRequested += () => Close();
        }
    }

    private void TopBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }
}