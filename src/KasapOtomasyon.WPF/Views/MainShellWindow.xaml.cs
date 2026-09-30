using System.Windows;
using System.Windows.Input;
using KasapOtomasyon.WPF.ViewModels;

namespace KasapOtomasyon.WPF.Views;

public partial class MainShellWindow : Window
{
    private readonly MainShellViewModel _viewModel;
    private bool _isFullscreen = true;

    public MainShellWindow(MainShellViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.ToggleWindowStateRequested += ToggleFullscreenMode;
        _viewModel.MinimizeWindowRequested += () => WindowState = WindowState.Minimized;
        _viewModel.CloseWindowRequested += () => System.Windows.Application.Current.Shutdown();
    }

    private void Logo_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _viewModel.NavService.NavigateToMainMenu();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void ToggleFullscreen_Click(object sender, RoutedEventArgs e)
    {
        ToggleFullscreenMode();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.Application.Current.Shutdown();
    }

    private void ShortcutsModalOverlay_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _viewModel.IsShortcutsModalOpen = false;
    }

    private void ToggleFullscreenMode()
    {
        _isFullscreen = !_isFullscreen;
        if (_isFullscreen)
        {
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
        }
        else
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowState = WindowState.Normal;
            Width = 1366;
            Height = 768;
        }
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        // F11: Toggle Fullscreen
        if (e.Key == Key.F11)
        {
            ToggleFullscreenMode();
            e.Handled = true;
            return;
        }

        // F1: Toggle Shortcuts Sheet
        if (e.Key == Key.F1)
        {
            _viewModel.IsShortcutsModalOpen = !_viewModel.IsShortcutsModalOpen;
            e.Handled = true;
            return;
        }

        // Ctrl+Home: Quick Home
        if (e.Key == Key.Home && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            _viewModel.NavService.NavigateToMainMenu();
            e.Handled = true;
            return;
        }

        // Global Module Keys
        if (e.Key == Key.F2)
        {
            _viewModel.NavService.NavigateTo(_viewModel.PosVM, "Satış (POS)");
            e.Handled = true;
            return;
        }
        if (e.Key == Key.F3)
        {
            _viewModel.NavService.NavigateTo(_viewModel.ProductionVM, "Üretim & Mezbaha (BOM)");
            e.Handled = true;
            return;
        }
        if (e.Key == Key.F4)
        {
            _viewModel.NavService.NavigateTo(_viewModel.StockVM, "Stok & Depo Yönetimi");
            e.Handled = true;
            return;
        }
        if (e.Key == Key.F5)
        {
            _viewModel.NavService.NavigateTo(_viewModel.ProductVM, "Ürün & Barkod Yönetimi");
            e.Handled = true;
            return;
        }
        if (e.Key == Key.F6)
        {
            _viewModel.NavService.NavigateTo(_viewModel.FinanceVM, "Finans & Kasa Yönetimi");
            e.Handled = true;
            return;
        }
        if (e.Key == Key.F7)
        {
            _viewModel.NavService.NavigateTo(_viewModel.CustomerVM, "Cari Hesaplar & Müşteriler");
            e.Handled = true;
            return;
        }
        if (e.Key == Key.F8)
        {
            if (_viewModel.NavService.CurrentViewModel is PosViewModel posVm)
            {
                _ = posVm.ReadScaleWeightAsync();
            }
            e.Handled = true;
            return;
        }
        if (e.Key == Key.F9)
        {
            _viewModel.NavService.NavigateTo(_viewModel.ReportsVM, "Yönetici Raporları & Analiz");
            e.Handled = true;
            return;
        }
        if (e.Key == Key.F10)
        {
            _viewModel.NavService.NavigateTo(_viewModel.EInvoiceVM, "E-Fatura & E-Arşiv");
            e.Handled = true;
            return;
        }

        // Escape: Go Back or close modal
        if (e.Key == Key.Escape)
        {
            if (_viewModel.IsShortcutsModalOpen)
            {
                _viewModel.IsShortcutsModalOpen = false;
                e.Handled = true;
                return;
            }
            if (_viewModel.IsNotificationDrawerOpen)
            {
                _viewModel.IsNotificationDrawerOpen = false;
                e.Handled = true;
                return;
            }
            if (!_viewModel.NavService.IsOnMainMenu)
            {
                _viewModel.NavService.GoBack();
                e.Handled = true;
                return;
            }
        }
    }
}
