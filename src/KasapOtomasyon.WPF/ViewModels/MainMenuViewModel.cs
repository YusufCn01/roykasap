using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.Application.Interfaces;
using KasapOtomasyon.WPF.Services;

namespace KasapOtomasyon.WPF.ViewModels;

public class MenuCardItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#B3261E";
    public string Shortcut { get; set; } = string.Empty;
    public string? BadgeText { get; set; }
    public string BadgeColorHex { get; set; } = "#EF4444";
    public bool IsBadgeVisible => !string.IsNullOrEmpty(BadgeText);
    public bool IsLocked { get; set; }
}

public partial class MainMenuViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly NotificationCenterService _notificationCenter;
    private readonly IReportService _reportService;
    private readonly ILocalizationService _locService;

    [ObservableProperty]
    private ObservableCollection<MenuCardItem> menuCards = new();

    [ObservableProperty]
    private string globalSearchText = string.Empty;

    [ObservableProperty]
    private DashboardSummaryDto summaryStats = new();

    public MainMenuViewModel(
        INavigationService navigationService,
        NotificationCenterService notificationCenter,
        IReportService reportService,
        ILocalizationService locService)
    {
        _navigationService = navigationService;
        _notificationCenter = notificationCenter;
        _reportService = reportService;
        _locService = locService;

        _locService.LanguageChanged += (s, lang) =>
        {
            _ = LoadMenuAsync();
            OnPropertyChanged(string.Empty);
        };

        _ = LoadMenuAsync();
    }

    public async Task LoadMenuAsync()
    {
        await _notificationCenter.RefreshNotificationsAsync();
        try
        {
            SummaryStats = await _reportService.GetDashboardSummaryAsync();
        }
        catch { }

        var cards = new List<MenuCardItem>
        {
            new()
            {
                Id = "POS",
                Title = _locService.Get("menu.pos.title"),
                Subtitle = _locService.Get("menu.pos.desc"),
                Icon = "🥩",
                ColorHex = "#B3261E",
                Shortcut = "F2",
                BadgeText = null
            },
            new()
            {
                Id = "PRODUCTION",
                Title = _locService.Get("menu.production.title"),
                Subtitle = _locService.Get("menu.production.desc"),
                Icon = "🔪",
                ColorHex = "#991B1B",
                Shortcut = "F3",
                BadgeText = SummaryStats.ActiveLotCount > 0 ? $"{SummaryStats.ActiveLotCount} Karkas" : null,
                BadgeColorHex = "#3B82F6"
            },
            new()
            {
                Id = "STOCK",
                Title = _locService.Get("menu.stock.title"),
                Subtitle = _locService.Get("menu.stock.desc"),
                Icon = "📦",
                ColorHex = "#7F1D1D",
                Shortcut = "F4",
                BadgeText = _notificationCenter.StockBadgeCount > 0 ? $"{_notificationCenter.StockBadgeCount} Kritik" : null,
                BadgeColorHex = "#EF4444"
            },
            new()
            {
                Id = "REPROCESSING",
                Title = _locService.Get("menu.reprocessing.title"),
                Subtitle = _locService.Get("menu.reprocessing.desc"),
                Icon = "🥩",
                ColorHex = "#B45309",
                Shortcut = "",
                BadgeText = _locService.Get("reprocessing.tab.incentives"),
                BadgeColorHex = "#16A34A"
            },
            new()
            {
                Id = "PRODUCTS",
                Title = _locService.Get("menu.products.title"),
                Subtitle = _locService.Get("menu.products.desc"),
                Icon = "🏷",
                ColorHex = "#831843",
                Shortcut = "F5"
            },
            new()
            {
                Id = "FINANCE",
                Title = _locService.Get("menu.finance.title"),
                Subtitle = _locService.Get("menu.finance.desc"),
                Icon = "💰",
                ColorHex = "#701A75",
                Shortcut = "F6",
                BadgeText = _notificationCenter.FinanceBadgeCount > 0 ? "Kasa Kapalı" : null,
                BadgeColorHex = "#F59E0B"
            },
            new()
            {
                Id = "CUSTOMERS",
                Title = _locService.Get("menu.customers.title"),
                Subtitle = _locService.Get("menu.customers.desc"),
                Icon = "👥",
                ColorHex = "#4C1D95",
                Shortcut = "F7"
            },
            new()
            {
                Id = "REPORTS",
                Title = _locService.Get("menu.reports.title"),
                Subtitle = _locService.Get("menu.reports.desc"),
                Icon = "📊",
                ColorHex = "#1E3A8A",
                Shortcut = "F9"
            },
            new()
            {
                Id = "EINVOICE",
                Title = _locService.Get("menu.einvoice.title"),
                Subtitle = _locService.Get("menu.einvoice.desc"),
                Icon = "📑",
                ColorHex = "#14532D",
                Shortcut = "F10"
            },
            new()
            {
                Id = "USERS",
                Title = _locService.Get("menu.users.title"),
                Subtitle = _locService.Get("menu.users.desc"),
                Icon = "👤",
                ColorHex = "#1E293B",
                Shortcut = ""
            },
            new()
            {
                Id = "LICENSE",
                Title = _locService.Get("menu.license.title"),
                Subtitle = _locService.Get("menu.license.desc"),
                Icon = "🔑",
                ColorHex = "#312E81",
                Shortcut = "",
                BadgeText = _notificationCenter.LicenseBadgeCount > 0 ? "Süre Yaklaştı" : null,
                BadgeColorHex = "#F59E0B"
            },
            new()
            {
                Id = "SETTINGS",
                Title = _locService.Get("menu.settings.title"),
                Subtitle = _locService.Get("menu.settings.desc"),
                Icon = "⚙",
                ColorHex = "#334155",
                Shortcut = ""
            }
        };

        MenuCards = new ObservableCollection<MenuCardItem>(cards);
    }

    [RelayCommand]
    private void OpenModule(string moduleId)
    {
        // Event/Action handled by MainWindow router
        ModuleSelected?.Invoke(moduleId);
    }

    public event Action<string>? ModuleSelected;
}
