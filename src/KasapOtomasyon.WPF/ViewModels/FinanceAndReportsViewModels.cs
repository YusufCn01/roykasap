using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.Application.Interfaces;
using KasapOtomasyon.Application.Interfaces.Adapters;
using KasapOtomasyon.Domain.Entities;
using Microsoft.Win32;

namespace KasapOtomasyon.WPF.ViewModels;

public partial class FinanceViewModel : ObservableObject
{
    private readonly IFinanceService _financeService;
    private readonly ICashRegisterService _cashService;

    [ObservableProperty]
    private ObservableCollection<CustomerDto> customers = new();

    [ObservableProperty]
    private CustomerDto? selectedCustomer;

    [ObservableProperty]
    private ObservableCollection<CustomerTransaction> customerStatement = new();

    [ObservableProperty]
    private CashSessionSummaryDto? activeCashSession;

    [ObservableProperty]
    private decimal newOpeningBalance = 1500m;

    [ObservableProperty]
    private decimal closingActualCount;

    [ObservableProperty]
    private string closingNotes = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public FinanceViewModel(IFinanceService financeService, ICashRegisterService cashService)
    {
        _financeService = financeService;
        _cashService = cashService;
        _ = LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        try
        {
            var list = await _financeService.GetAllCustomersAsync();
            Customers = new ObservableCollection<CustomerDto>(list);
            if (list.Any())
            {
                SelectedCustomer = list.First();
                await LoadCustomerStatementAsync();
            }

            ActiveCashSession = await _cashService.GetActiveSessionAsync();
        }
        catch { }
    }

    [RelayCommand]
    public async Task SelectCustomerAsync(CustomerDto? customer)
    {
        SelectedCustomer = customer;
        await LoadCustomerStatementAsync();
    }

    private async Task LoadCustomerStatementAsync()
    {
        if (SelectedCustomer == null) return;
        var st = await _financeService.GetCustomerStatementAsync(SelectedCustomer.Id);
        CustomerStatement = new ObservableCollection<CustomerTransaction>(st);
    }

    [RelayCommand]
    public async Task SendOverdueSmsAsync(CustomerDto? customer)
    {
        if (customer == null || string.IsNullOrEmpty(customer.PhoneNumber))
        {
            MessageBox.Show("Müşterinin geçerli bir telefon numarası bulunamadı.", "Telefon Yok", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var sent = await _financeService.SendOverduePaymentSmsReminderAsync(customer.Id);
        if (sent)
        {
            StatusMessage = $"✓ {customer.Name} için SMS borç hatırlatması gönderildi.";
            MessageBox.Show($"{customer.Name} ({customer.PhoneNumber}) numarasına SMS hatırlatması başarıyla iletildi.", "SMS Gönderildi", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    public async Task OpenCashSessionAsync()
    {
        var session = await _cashService.OpenSessionAsync(1, 1, "Kasiyer", NewOpeningBalance);
        ActiveCashSession = session;
        StatusMessage = "✓ Kasa açılışı tamamlandı.";
    }

    [RelayCommand]
    public async Task CloseCashSessionAsync()
    {
        if (ActiveCashSession == null) return;

        var closed = await _cashService.CloseSessionAsync(ActiveCashSession.SessionId, ClosingActualCount, ClosingNotes);
        ActiveCashSession = closed;
        StatusMessage = $"✓ Kasa kapatıldı. Kasa Farkı: {closed.DiscrepancyAmount:N2} ₺";
        MessageBox.Show($"Kasa Başarıyla Kapatıldı!\n\nHesaplanan Bakiye: {closed.CalculatedBalance:N2} ₺\nSayım Yapılan Nakit: {closed.ActualBalance:N2} ₺\nKasa Farkı: {closed.DiscrepancyAmount:N2} ₺", "Kasa Kapanış Raporu", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}

public partial class ReportsViewModel : ObservableObject
{
    private readonly IReportService _reportService;

    [ObservableProperty]
    private DashboardSummaryDto dashboard = new();

    [ObservableProperty]
    private ObservableCollection<DailyTurnoverReportDto> turnoverReports = new();

    [ObservableProperty]
    private ObservableCollection<ProfitabilityReportDto> profitabilityReports = new();

    [ObservableProperty]
    private DateTime reportStartDate = DateTime.Today.AddDays(-30);

    [ObservableProperty]
    private DateTime reportEndDate = DateTime.Today;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CarcassYieldByBreedReportDto> breedReports = new();

    [ObservableProperty]
    private ObservableCollection<DeboningMassBalanceReportDto> massBalanceReports = new();

    [ObservableProperty]
    private ObservableCollection<HaccpQualityComplianceReportDto> haccpReports = new();

    [ObservableProperty]
    private string selectedReportTab = "TURNOVER"; // TURNOVER, PROFIT, BREED, MASS_BALANCE, HACCP

    public ReportsViewModel(IReportService reportService)
    {
        _reportService = reportService;
        _ = LoadReportsAsync();
    }

    [RelayCommand]
    public void SwitchReportTab(string tab)
    {
        SelectedReportTab = tab;
    }

    public async Task LoadReportsAsync()
    {
        try
        {
            Dashboard = await _reportService.GetDashboardSummaryAsync();

            var turnovers = await _reportService.GetDailyTurnoverReportAsync(ReportStartDate, ReportEndDate);
            TurnoverReports = new ObservableCollection<DailyTurnoverReportDto>(turnovers);

            var profits = await _reportService.GetProfitabilityReportAsync(ReportStartDate, ReportEndDate);
            ProfitabilityReports = new ObservableCollection<ProfitabilityReportDto>(profits);

            var breeds = await _reportService.GetCarcassYieldByBreedReportAsync();
            BreedReports = new ObservableCollection<CarcassYieldByBreedReportDto>(breeds);

            var massBalances = await _reportService.GetDeboningMassBalanceReportAsync();
            MassBalanceReports = new ObservableCollection<DeboningMassBalanceReportDto>(massBalances);

            var haccp = await _reportService.GetHaccpComplianceReportAsync();
            HaccpReports = new ObservableCollection<HaccpQualityComplianceReportDto>(haccp);
        }
        catch { }
    }

    [RelayCommand]
    public async Task ExportFullExecutiveExcelAsync()
    {
        try
        {
            var bytes = await _reportService.ExportFullExecutiveReportExcelAsync(ReportStartDate, ReportEndDate);
            var sfd = new SaveFileDialog
            {
                Filter = "Excel Dosyası (*.xlsx)|*.xlsx",
                FileName = $"RoyPos_Yonetici_ERP_Raporu_{DateTime.Now:yyyyMMdd}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                await File.WriteAllBytesAsync(sfd.FileName, bytes);
                StatusMessage = $"✓ Çok sayfalı kurumsal Excel raporu kaydedildi: {Path.GetFileName(sfd.FileName)}";
                MessageBox.Show("Tüm mezbaha, ciro, randıman ve kütle dengesi verileri profesyonel Excel formatında dışa aktarıldı!", "Kurumsal Rapor Hazır", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Excel export hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public async Task ExportSalesExcelAsync()
    {
        try
        {
            var bytes = await _reportService.ExportSalesToExcelAsync(ReportStartDate, ReportEndDate);
            var sfd = new SaveFileDialog
            {
                Filter = "Excel Dosyası (*.xlsx)|*.xlsx",
                FileName = $"Kasap_Satis_Raporu_{DateTime.Now:yyyyMMdd}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                await File.WriteAllBytesAsync(sfd.FileName, bytes);
                StatusMessage = $"✓ Excel raporu kaydedildi: {Path.GetFileName(sfd.FileName)}";
                MessageBox.Show("Excel raporu başarıyla dışa aktarıldı!", "Rapor Hazır", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Excel export hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public async Task ExportProductionExcelAsync()
    {
        try
        {
            var bytes = await _reportService.ExportProductionYieldToExcelAsync(null);
            var sfd = new SaveFileDialog
            {
                Filter = "Excel Dosyası (*.xlsx)|*.xlsx",
                FileName = $"Mezbaha_Randiman_Raporu_{DateTime.Now:yyyyMMdd}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                await File.WriteAllBytesAsync(sfd.FileName, bytes);
                StatusMessage = $"✓ Mezbaha randıman raporu kaydedildi: {Path.GetFileName(sfd.FileName)}";
                MessageBox.Show("Mezbaha üretim randıman raporu Excel'e aktarıldı!", "Rapor Hazır", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Excel export hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

public partial class LicenseViewModel : ObservableObject
{
    private readonly ILicenseService _licenseService;

    [ObservableProperty]
    private MachineFingerprintDto machineInfo = new();

    [ObservableProperty]
    private LicenseValidationResultDto licenseStatus = new();

    [ObservableProperty]
    private string licenseFilePath = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public LicenseViewModel(ILicenseService licenseService)
    {
        _licenseService = licenseService;
        _ = LoadLicenseDataAsync();
    }

    public async Task LoadLicenseDataAsync()
    {
        try
        {
            MachineInfo = await _licenseService.GetMachineFingerprintAsync();
            LicenseStatus = await _licenseService.ValidateLicenseAsync();
        }
        catch { }
    }

    [RelayCommand]
    public void CopyFingerprint()
    {
        Clipboard.SetText(MachineInfo.Fingerprint);
        StatusMessage = "✓ Donanım parmak izi panoya kopyalandı.";
    }

    [RelayCommand]
    public async Task LoadLicenseFileAsync()
    {
        var ofd = new OpenFileDialog
        {
            Filter = "Kasap Lisans Dosyası (*.lic)|*.lic|Tüm Dosyalar (*.*)|*.*",
            Title = "Lisans Dosyası Seçin"
        };

        if (ofd.ShowDialog() == true)
        {
            LicenseFilePath = ofd.FileName;
            var success = await _licenseService.ActivateLicenseFileAsync(LicenseFilePath);
            if (success)
            {
                await LoadLicenseDataAsync();
                StatusMessage = "✓ Yeni lisans başarıyla etkinleştirildi!";
                MessageBox.Show("Lisans dosyası başarıyla doğrulandı ve yüklendi!", "Lisans Aktif", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Lisans dosyası doğrulanamadı! Lütfen geçerli ve bu bilgisayara ait bir lisans dosyası seçin.", "Geçersiz Lisans", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    public async Task StartTrialAsync()
    {
        var success = await _licenseService.ActivateTrialAsync();
        if (success)
        {
            await LoadLicenseDataAsync();
            StatusMessage = "✓ 15 Günlük Deneme Sürümü Başlatıldı.";
        }
    }
}

public partial class SettingsViewModel : ObservableObject
{
    private readonly IBackupService _backupService;
    private readonly ITeraziAdapter _scaleAdapter;

    [ObservableProperty]
    private string selectedScalePort = "COM1";

    [ObservableProperty]
    private string selectedScaleBrand = "SIMULATOR";

    [ObservableProperty]
    private string selectedSmsProvider = "MOCK";

    [ObservableProperty]
    private string selectedEInvoiceIntegrator = "MOCK_INTEGRATOR";

    [ObservableProperty]
    private string backupDirectory = @"C:\KasapOtomasyon\Yedekler";

    [ObservableProperty]
    private ObservableCollection<string> backupFiles = new();

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public List<string> ScaleBrands => new() { "SIMULATOR", "CAS", "DIGI", "BAYKON", "GENERIC_SERIAL" };
    public List<string> ScalePorts => new() { "COM1", "COM2", "COM3", "COM4", "COM5" };
    public List<string> SmsProviders => new() { "MOCK", "NETGSM", "ILETI_MERKEZI" };
    public List<string> EInvoiceIntegrators => new() { "MOCK_INTEGRATOR", "UYUMSOFT", "LOGO", "FORIBA" };

    public SettingsViewModel(IBackupService backupService, ITeraziAdapter scaleAdapter)
    {
        _backupService = backupService;
        _scaleAdapter = scaleAdapter;
        _ = LoadBackupHistoryAsync();
    }

    public async Task LoadBackupHistoryAsync()
    {
        var list = await _backupService.GetBackupHistoryAsync();
        BackupFiles = new ObservableCollection<string>(list);
    }

    [RelayCommand]
    public async Task TestScaleConnectionAsync()
    {
        var connected = await _scaleAdapter.ConnectAsync(SelectedScalePort);
        if (connected)
        {
            var read = await _scaleAdapter.ReadWeightAsync();
            StatusMessage = $"✓ Terazi Bağlantısı Başarılı: {_scaleAdapter.BrandName} ({read.WeightKg:N3} kg)";
            MessageBox.Show($"Teraziye başarıyla bağlanıldı!\nOkunan Değer: {read.WeightKg:N3} kg", "Terazi Test", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show("Teraziye bağlanılamadı. Portu ve kablo bağlantısını kontrol edin.", "Bağlantı Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    public async Task CreateBackupAsync()
    {
        var target = Path.Combine(BackupDirectory, $"Kasap_Yedek_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        var success = await _backupService.CreateBackupAsync(target);
        if (success)
        {
            await LoadBackupHistoryAsync();
            StatusMessage = $"✓ Veritabanı yedeği alındı: {Path.GetFileName(target)}";
            MessageBox.Show($"Tüm sistem veritabanı başarıyla yedeklendi!\n\nDosya: {target}", "Yedekleme Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
