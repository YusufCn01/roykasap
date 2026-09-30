using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.Application.Interfaces;

namespace KasapOtomasyon.WPF.Services;

public class AppNotificationItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Category { get; set; } = "General"; // "Stock", "Finance", "License", "Production"
    public string Severity { get; set; } = "Warning"; // "Info", "Warning", "Danger"
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsRead { get; set; }
}

public partial class NotificationCenterService : ObservableObject
{
    private readonly IStockService _stockService;
    private readonly ILicenseService _licenseService;
    private readonly ICashRegisterService _cashService;

    [ObservableProperty]
    private ObservableCollection<AppNotificationItem> notifications = new();

    [ObservableProperty]
    private int unreadCount;

    // Card Badge Counters
    [ObservableProperty]
    private int stockBadgeCount;

    [ObservableProperty]
    private int financeBadgeCount;

    [ObservableProperty]
    private int licenseBadgeCount;

    [ObservableProperty]
    private int productionBadgeCount;

    public NotificationCenterService(IStockService stockService, ILicenseService licenseService, ICashRegisterService cashService)
    {
        _stockService = stockService;
        _licenseService = licenseService;
        _cashService = cashService;
    }

    public async Task RefreshNotificationsAsync()
    {
        var items = new List<AppNotificationItem>();

        try
        {
            // 1. Stock / SKT Alerts
            var crits = await _stockService.GetCriticalStockAlertsAsync();
            var expiries = await _stockService.GetCriticalExpiryAlertsAsync();

            StockBadgeCount = crits.Count + expiries.Count;

            foreach (var e in expiries)
            {
                items.Add(new AppNotificationItem
                {
                    Title = "⏰ Kritik SKT / Taze Et Uyarısı",
                    Message = $"{e.ProductName} (Parti: {e.LotNumber ?? "-"}) ürününün son kullanma tarihine 2 günden az kaldı!",
                    Category = "Stock",
                    Severity = "Danger"
                });
            }

            foreach (var c in crits)
            {
                items.Add(new AppNotificationItem
                {
                    Title = "⚠ Minimum Seviye Altı Stok",
                    Message = $"{c.ProductName} stok miktarı ({c.CurrentQuantity:N2} kg) kritik eşiğin altına indi.",
                    Category = "Stock",
                    Severity = "Warning"
                });
            }

            // 2. License Warning
            var lic = await _licenseService.ValidateLicenseAsync();
            if (lic.IsWarningRequired)
            {
                LicenseBadgeCount = lic.RemainingDays <= 7 ? 1 : 0;
                items.Add(new AppNotificationItem
                {
                    Title = "🔑 Lisans Süre Uyarısı",
                    Message = lic.WarningBannerText,
                    Category = "License",
                    Severity = lic.RemainingDays <= 3 ? "Danger" : "Warning"
                });
            }
            else
            {
                LicenseBadgeCount = 0;
            }

            // 3. Cash Session Status
            var activeSession = await _cashService.GetActiveSessionAsync();
            if (activeSession == null)
            {
                FinanceBadgeCount = 1;
                items.Add(new AppNotificationItem
                {
                    Title = "💰 Kasa Açık Değil",
                    Message = "Bugün için aktif bir kasa açılış oturumu bulunmuyor. Lütfen gün açılışı yapın.",
                    Category = "Finance",
                    Severity = "Info"
                });
            }
            else
            {
                FinanceBadgeCount = 0;
            }
        }
        catch { }

        Notifications = new ObservableCollection<AppNotificationItem>(items);
        UnreadCount = items.Count(n => !n.IsRead);
    }

    public void MarkAllAsRead()
    {
        foreach (var n in Notifications)
        {
            n.IsRead = true;
        }
        UnreadCount = 0;
    }
}
