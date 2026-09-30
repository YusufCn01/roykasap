using System.Windows;
using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.Infrastructure;
using KasapOtomasyon.Infrastructure.Data;
using KasapOtomasyon.WPF.Services;
using KasapOtomasyon.WPF.ViewModels;
using KasapOtomasyon.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KasapOtomasyon.WPF;

public partial class App : System.Windows.Application
{
    private IHost? _host;
    private LoginWindow? _loginWindow;
    private MainShellWindow? _shellWindow;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Register Global Unhandled Exception Handlers
        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            Serilog.Log.Fatal(ex, "AppDomain Unhandled Exception");
            MessageBox.Show($"Kritik Uygulama Hatası: {ex?.Message}\n\nDetay: {ex?.StackTrace}", "Kasap Otomasyonu - Kritik Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        };

        DispatcherUnhandledException += (s, args) =>
        {
            Serilog.Log.Error(args.Exception, "Dispatcher Unhandled Exception");
            MessageBox.Show($"Uygulama Hatası: {args.Exception.Message}", "Kasap Otomasyonu - Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
            args.Handled = true;
        };

        try
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Infrastructure Services (EF Core, Licensing, Serilog, Adapters, App Services)
                    services.AddInfrastructureServices(context.Configuration);

                    // Global WPF Services
                    services.AddSingleton<INavigationService, NavigationService>();
                    services.AddSingleton<NotificationCenterService>();

                    // ViewModels (Dedicated Full Pages)
                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<MainShellViewModel>();
                    services.AddTransient<MainMenuViewModel>();
                    services.AddTransient<PosViewModel>();
                    services.AddTransient<ProductionViewModel>();
                    services.AddTransient<ProductManagementViewModel>();
                    services.AddTransient<StockViewModel>();
                    services.AddTransient<FinanceViewModel>();
                    services.AddTransient<CustomerAccountsViewModel>();
                    services.AddTransient<PurchaseInvoiceViewModel>();
                    services.AddTransient<ReportsViewModel>();
                    services.AddTransient<EInvoiceViewModel>();
                    services.AddTransient<UserManagementViewModel>();
                    services.AddTransient<LicenseViewModel>();
                    services.AddTransient<SettingsViewModel>();
                    services.AddTransient<SlaughterhouseViewModel>();
                    services.AddTransient<ShelfLifeReprocessingViewModel>();

                    // Windows
                    services.AddTransient<LoginWindow>();
                    services.AddTransient<MainShellWindow>();
                })
                .Build();

            await _host.StartAsync();

            // Initialize and Seed Database
            using (var scope = _host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<KasapDbContext>();
                await DbInitializer.InitializeAsync(dbContext);
            }

            // Initialize WPF Global Localization Source for Dynamic XAML markup
            var locService = _host.Services.GetRequiredService<KasapOtomasyon.Application.Interfaces.ILocalizationService>();
            KasapOtomasyon.WPF.Markup.LocalizationSource.Instance.Initialize(locService);

            ShowLogin();
        }
        catch (Exception ex)
        {
            Serilog.Log.Fatal(ex, "Kritik Başlatma Hatası");
            MessageBox.Show($"Program açılırken bir hata oluştu:\n\n{ex.Message}\n\n{ex.StackTrace}", "Kasap Otomasyonu Başlatılamadı", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private void ShowLogin()
    {
        _shellWindow?.Close();

        var loginVm = _host!.Services.GetRequiredService<LoginViewModel>();
        loginVm.LoginSuccessful += OnLoginSuccessful;

        _loginWindow = new LoginWindow(loginVm);
        _loginWindow.Show();
    }

    private void OnLoginSuccessful(UserDto user)
    {
        var shellVm = _host!.Services.GetRequiredService<MainShellViewModel>();
        shellVm.CurrentUser = user;
        shellVm.LogoutRequested += OnLogoutRequested;

        _shellWindow = new MainShellWindow(shellVm);
        _shellWindow.Show();

        _loginWindow?.Close();
    }

    private void OnLogoutRequested()
    {
        ShowLogin();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        base.OnExit(e);
    }
}
