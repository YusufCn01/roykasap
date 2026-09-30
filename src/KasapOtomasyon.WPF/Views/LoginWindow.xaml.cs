using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using KasapOtomasyon.WPF.ViewModels;

namespace KasapOtomasyon.WPF.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;
    private bool _isPasswordShown;

    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        PasswordInput.Password = "admin123";
        _viewModel.Password = "admin123";

        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(LoginViewModel.Password))
            {
                if (PasswordInput.Password != _viewModel.Password)
                {
                    PasswordInput.Password = _viewModel.Password ?? string.Empty;
                }
                if (PasswordRevealInput.Text != _viewModel.Password)
                {
                    PasswordRevealInput.Text = _viewModel.Password ?? string.Empty;
                }
            }
        };
    }

    private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (_viewModel.Password != PasswordInput.Password)
        {
            _viewModel.Password = PasswordInput.Password;
        }
    }

    private void TogglePasswordEye_Click(object sender, RoutedEventArgs e)
    {
        _isPasswordShown = !_isPasswordShown;
        if (_isPasswordShown)
        {
            PasswordRevealInput.Text = PasswordInput.Password;
            PasswordRevealInput.Visibility = Visibility.Visible;
            PasswordInput.Visibility = Visibility.Collapsed;
        }
        else
        {
            PasswordInput.Password = PasswordRevealInput.Text;
            PasswordInput.Visibility = Visibility.Visible;
            PasswordRevealInput.Visibility = Visibility.Collapsed;
        }
    }

    private void StandardLoginTab_Click(object sender, RoutedEventArgs e)
    {
        StandardLoginPanel.Visibility = Visibility.Visible;
        PinLoginPanel.Visibility = Visibility.Collapsed;

        BtnStandardTab.Background = (Brush)FindResource("BrushAccent");
        BtnStandardTab.Foreground = Brushes.White;

        BtnPinTab.Background = Brushes.Transparent;
        BtnPinTab.Foreground = (Brush)FindResource("BrushTextSecondary");
    }

    private void PinLoginTab_Click(object sender, RoutedEventArgs e)
    {
        StandardLoginPanel.Visibility = Visibility.Collapsed;
        PinLoginPanel.Visibility = Visibility.Visible;

        BtnPinTab.Background = (Brush)FindResource("BrushAccent");
        BtnPinTab.Foreground = Brushes.White;

        BtnStandardTab.Background = Brushes.Transparent;
        BtnStandardTab.Foreground = (Brush)FindResource("BrushTextSecondary");
    }

    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.Application.Current.Shutdown();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (PinLoginPanel.Visibility == Visibility.Visible)
            {
                _ = _viewModel.PinLoginCommand.ExecuteAsync(null);
            }
            else
            {
                _ = _viewModel.LoginCommand.ExecuteAsync(null);
            }
        }
    }
}
