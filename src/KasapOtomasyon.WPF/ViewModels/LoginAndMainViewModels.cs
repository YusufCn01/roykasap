using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.Application.Interfaces;
using KasapOtomasyon.Application.Interfaces.Adapters;
using KasapOtomasyon.WPF.Services;

namespace KasapOtomasyon.WPF.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly ILicenseService _licenseService;
    private readonly ILocalizationService _locService;

    [ObservableProperty]
    private string username = "admin";

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string pinCode = string.Empty;

    [ObservableProperty]
    private bool isPinMode;

    [ObservableProperty]
    private bool isPasswordVisible;

    [ObservableProperty]
    private bool rememberMe = true;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private LicenseValidationResultDto? licenseInfo;

    [ObservableProperty]
    private string companyName = "Roy Kasap & Entegre Et";

    public event Action<UserDto>? LoginSuccessful;

    public LoginViewModel(IAuthService authService, ILicenseService licenseService, ILocalizationService locService)
    {
        _authService = authService;
        _licenseService = licenseService;
        _locService = locService;
        _ = LoadLicenseStatusAsync();
    }

    [RelayCommand]
    public void SwitchLanguage(string code)
    {
        _locService.SetLanguage(code);
    }

    public async Task LoadLicenseStatusAsync()
    {
        try
        {
            LicenseInfo = await _licenseService.ValidateLicenseAsync();
            if (LicenseInfo != null && !string.IsNullOrWhiteSpace(LicenseInfo.CompanyName) && LicenseInfo.CompanyName != "Deneme Sürümü Kullanıcısı")
            {
                CompanyName = LicenseInfo.CompanyName;
            }
            else
            {
                var rec = await _licenseService.GetCurrentLicenseRecordAsync();
                if (rec != null && !string.IsNullOrWhiteSpace(rec.CompanyName))
                {
                    CompanyName = rec.CompanyName;
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lisans kontrolü hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Kullanıcı adı ve şifre gereklidir.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var user = await _authService.LoginAsync(Username, Password);
            if (user != null)
            {
                LoginSuccessful?.Invoke(user);
            }
            else
            {
                ErrorMessage = "Hatalı kullanıcı adı veya şifre!";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Giriş hatası: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task PinLoginAsync()
    {
        if (string.IsNullOrWhiteSpace(PinCode))
        {
            ErrorMessage = "Kasiyer PIN kodu giriniz.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var user = await _authService.LoginWithPinAsync(PinCode);
            if (user != null)
            {
                LoginSuccessful?.Invoke(user);
            }
            else
            {
                ErrorMessage = "Geçersiz PIN Kodu!";
                PinCode = string.Empty;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"PIN Giriş hatası: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ClearPin()
    {
        PinCode = string.Empty;
    }

    [RelayCommand]
    private void TogglePinMode(bool isPin)
    {
        IsPinMode = isPin;
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    private void TogglePasswordVisibility()
    {
        IsPasswordVisible = !IsPasswordVisible;
    }

    [RelayCommand]
    private void AppendPin(string digit)
    {
        if (PinCode.Length < 6)
        {
            PinCode += digit;
            if (PinCode.Length == 4)
            {
                _ = PinLoginAsync();
            }
        }
    }

    [RelayCommand]
    public void SelectDemoRole(string role)
    {
        switch (role.ToLower())
        {
            case "admin":
                Username = "admin";
                Password = "admin123";
                PinCode = "1234";
                break;
            case "kasap":
                Username = "kasap";
                Password = "kasap123";
                PinCode = "5678";
                break;
            case "vet":
                Username = "vet";
                Password = "vet123";
                PinCode = "9999";
                break;
            case "kasiyer":
                Username = "kasiyer";
                Password = "kasiyer123";
                PinCode = "0000";
                break;
        }
        ErrorMessage = string.Empty;
    }
}

public partial class MainShellViewModel : ObservableObject
{
    private readonly ILicenseService _licenseService;
    private readonly ITeraziAdapter _scaleAdapter;
    private readonly ILocalizationService _locService;
    public INavigationService NavService { get; }
    public NotificationCenterService NotificationCenter { get; }

    [ObservableProperty]
    private UserDto currentUser = null!;

    [ObservableProperty]
    private LicenseValidationResultDto licenseStatus = null!;

    [ObservableProperty]
    private string currentTimeString = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");

    [ObservableProperty]
    private bool isScaleConnected;

    [ObservableProperty]
    private string scaleLiveStatusText = "Terazi: Bağlı (Simülasyon Modu)";

    // Multi-tenant & Localization Header Properties
    [ObservableProperty]
    private string activePlantText = "🏢 İstanbul Mezbaha & İşleme Tesisi (PLN-IST-01)";

    [ObservableProperty]
    private string currentLanguageCode = "tr-TR";

    public IReadOnlyList<LanguageInfo> SupportedLanguages => _locService.SupportedLanguages;
    public LanguageInfo CurrentLanguage => _locService.CurrentLanguage;

    [RelayCommand]
    public void SwitchLanguage(string languageCode)
    {
        _locService.SetLanguage(languageCode);
        CurrentLanguageCode = _locService.CurrentLanguageCode;
        ShowToast(_locService.Get("common.success"), $"{_locService.CurrentLanguage.FlagEmoji} {_locService.CurrentLanguage.DisplayName}");
    }

    [ObservableProperty]
    private string toastTitle = string.Empty;

    [ObservableProperty]
    private string toastMessage = string.Empty;

    [ObservableProperty]
    private bool isToastVisible;

    [ObservableProperty]
    private bool isShortcutsModalOpen;

    [ObservableProperty]
    private bool isNotificationDrawerOpen;

    [ObservableProperty]
    private bool isFullscreen = true;

    [ObservableProperty]
    private bool isDarkTheme = true;

    public event Action? LogoutRequested;
    public event Action? ToggleWindowStateRequested;
    public event Action? MinimizeWindowRequested;
    public event Action? CloseWindowRequested;

    // Sub-ViewModels (Dedicated Full Pages)
    public MainMenuViewModel MainMenuVM { get; }
    public PosViewModel PosVM { get; }
    public ProductionViewModel ProductionVM { get; }
    public SlaughterhouseViewModel SlaughterhouseVM { get; }
    public ProductManagementViewModel ProductVM { get; }
    public StockViewModel StockVM { get; }
    public FinanceViewModel FinanceVM { get; }
    public CustomerAccountsViewModel CustomerVM { get; }
    public PurchaseInvoiceViewModel PurchaseInvoiceVM { get; }
    public ReportsViewModel ReportsVM { get; }
    public EInvoiceViewModel EInvoiceVM { get; }
    public UserManagementViewModel UserVM { get; }
    public LicenseViewModel LicenseVM { get; }
    public SettingsViewModel SettingsVM { get; }
    public ShelfLifeReprocessingViewModel ReprocessingVM { get; }

    [ObservableProperty]
    private ObservableObject currentViewModel = null!;

    public MainShellViewModel(
        ILicenseService licenseService,
        ITeraziAdapter scaleAdapter,
        ILocalizationService locService,
        INavigationService navService,
        NotificationCenterService notificationCenter,
        MainMenuViewModel mainMenuVM,
        PosViewModel posVM,
        ProductionViewModel productionVM,
        SlaughterhouseViewModel slaughterhouseVM,
        ProductManagementViewModel productVM,
        StockViewModel stockVM,
        FinanceViewModel financeVM,
        CustomerAccountsViewModel customerVM,
        PurchaseInvoiceViewModel purchaseInvoiceVM,
        ReportsViewModel reportsVM,
        EInvoiceViewModel eInvoiceVM,
        UserManagementViewModel userVM,
        LicenseViewModel licenseVM,
        SettingsViewModel settingsVM,
        ShelfLifeReprocessingViewModel reprocessingVM)
    {
        _licenseService = licenseService;
        _scaleAdapter = scaleAdapter;
        _locService = locService;
        NavService = navService;
        NotificationCenter = notificationCenter;

        MainMenuVM = mainMenuVM;
        PosVM = posVM;
        ProductionVM = productionVM;
        SlaughterhouseVM = slaughterhouseVM;
        ProductVM = productVM;
        StockVM = stockVM;
        FinanceVM = financeVM;
        CustomerVM = customerVM;
        PurchaseInvoiceVM = purchaseInvoiceVM;
        ReportsVM = reportsVM;
        EInvoiceVM = eInvoiceVM;
        UserVM = userVM;
        LicenseVM = licenseVM;
        SettingsVM = settingsVM;
        ReprocessingVM = reprocessingVM;

        // Route hub clicks to dedicated full pages
        MainMenuVM.ModuleSelected += OnModuleSelected;

        // Synchronize NavigationService to root CurrentViewModel
        NavService.CurrentPageChanged += () =>
        {
            CurrentViewModel = NavService.CurrentViewModel;
        };

        // Set Main Menu as the starting hub
        NavService.SetMainMenuViewModel(MainMenuVM);
        CurrentViewModel = MainMenuVM;

        // Clock timer
        var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        timer.Tick += (s, e) => CurrentTimeString = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
        timer.Start();

        _locService.LanguageChanged += (s, lang) =>
        {
            CurrentLanguageCode = _locService.CurrentLanguageCode;
            UpdateHeaderStatuses();
            OnPropertyChanged(string.Empty);
        };

        _ = InitializeStatusAsync();
    }

    public void UpdateHeaderStatuses()
    {
        ActivePlantText = _locService.Get("shell.plantShort");
        ScaleLiveStatusText = IsScaleConnected
            ? _locService.Get("shell.scaleReady", _scaleAdapter.BrandName)
            : _locService.Get("shell.scaleConnected");
    }

    [ObservableProperty]
    private string companyName = "Roy Kasap & Entegre Et";

    public async Task InitializeStatusAsync()
    {
        try
        {
            LicenseStatus = await _licenseService.ValidateLicenseAsync();
            if (LicenseStatus != null && !string.IsNullOrWhiteSpace(LicenseStatus.CompanyName) && LicenseStatus.CompanyName != "Deneme Sürümü Kullanıcısı")
            {
                CompanyName = LicenseStatus.CompanyName;
            }
            else
            {
                var rec = await _licenseService.GetCurrentLicenseRecordAsync();
                if (rec != null && !string.IsNullOrWhiteSpace(rec.CompanyName))
                {
                    CompanyName = rec.CompanyName;
                }
            }

            ActivePlantText = $"🏢 {CompanyName} (PLN-IST-01)";
            IsScaleConnected = _scaleAdapter.IsConnected;
            UpdateHeaderStatuses();
            await NotificationCenter.RefreshNotificationsAsync();
        }
        catch { }
    }

    public void OnModuleSelected(string moduleId)
    {
        try
        {
            switch (moduleId?.ToUpperInvariant())
            {
                case "POS":
                    NavService.NavigateTo(PosVM, "menu.pos.title");
                    break;
                case "PRODUCTION":
                    NavService.NavigateTo(ProductionVM, "menu.production.title");
                    break;
                case "REPROCESSING":
                case "REYON_SKT":
                case "SALVAGE":
                    NavService.NavigateTo(ReprocessingVM, "menu.reprocessing.title");
                    break;
                case "SLAUGHTER":
                    NavService.NavigateTo(SlaughterhouseVM, "menu.slaughter.title");
                    break;
                case "STOCK":
                    NavService.NavigateTo(StockVM, "menu.stock.title");
                    break;
                case "PRODUCTS":
                    NavService.NavigateTo(ProductVM, "menu.products.title");
                    break;
                case "PURCHASE_INVOICE":
                case "ALIM_FATURASI":
                    NavService.NavigateTo(PurchaseInvoiceVM, "menu.invoices.title");
                    break;
                case "FINANCE":
                    NavService.NavigateTo(FinanceVM, "menu.finance.title");
                    break;
                case "CUSTOMERS":
                    NavService.NavigateTo(CustomerVM, "menu.customers.title");
                    break;
                case "REPORTS":
                    NavService.NavigateTo(ReportsVM, "menu.reports.title");
                    break;
                case "EINVOICE":
                    NavService.NavigateTo(EInvoiceVM, "menu.einvoice.title");
                    break;
                case "USERS":
                    NavService.NavigateTo(UserVM, "menu.users.title");
                    break;
                case "LICENSE":
                    NavService.NavigateTo(LicenseVM, "menu.license.title");
                    break;
                case "SETTINGS":
                    NavService.NavigateTo(SettingsVM, "menu.settings.title");
                    break;
                default:
                    NavService.NavigateToMainMenu();
                    break;
            }
        }
        catch (Exception ex)
        {
            ShowToast("Hata", $"Modül açılırken hata: {ex.Message}");
        }
    }

    public void ShowToast(string title, string message)
    {
        ToastTitle = title;
        ToastMessage = message;
        IsToastVisible = true;

        var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(4) };
        timer.Tick += (s, e) =>
        {
            IsToastVisible = false;
            timer.Stop();
        };
        timer.Start();
    }

    [RelayCommand]
    private void GoHome()
    {
        NavService.NavigateToMainMenu();
    }

    [RelayCommand]
    private void GoBack()
    {
        NavService.GoBack();
    }

    [RelayCommand]
    private void ToggleShortcuts()
    {
        IsShortcutsModalOpen = !IsShortcutsModalOpen;
    }

    [RelayCommand]
    private void ToggleNotifications()
    {
        IsNotificationDrawerOpen = !IsNotificationDrawerOpen;
        if (IsNotificationDrawerOpen)
        {
            _ = NotificationCenter.RefreshNotificationsAsync();
        }
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        IsDarkTheme = !IsDarkTheme;
        // Theme switching helper can update resource dictionaries
    }

    [RelayCommand]
    private void ToggleFullscreen()
    {
        ToggleWindowStateRequested?.Invoke();
    }

    [RelayCommand]
    private void MinimizeWindow()
    {
        MinimizeWindowRequested?.Invoke();
    }

    [RelayCommand]
    private void CloseWindow()
    {
        CloseWindowRequested?.Invoke();
    }

    [RelayCommand]
    private void Logout()
    {
        LogoutRequested?.Invoke();
    }
}
