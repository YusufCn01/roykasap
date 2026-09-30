using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasapOtomasyon.Setup.Services;

namespace KasapOtomasyon.Setup.ViewModels;

public partial class SetupWizardViewModel : ObservableObject
{
    private readonly InstallerEngine _engine = new();

    [ObservableProperty]
    private int currentStep = 1;

    // Step 1: License Agreement
    [ObservableProperty]
    private bool isLicenseAccepted;

    // Step 2: Customer / Company Details
    [ObservableProperty]
    private string companyName = "Özkanlar Kasap & Entegre Et San. Tic. Ltd. Şti.";

    [ObservableProperty]
    private string authorizedPerson = "Ahmet Özkan";

    [ObservableProperty]
    private string phone = "+90 532 555 1234";

    [ObservableProperty]
    private string taxOffice = "Büyük Mükellefler";

    [ObservableProperty]
    private string taxNumber = "3400998877";

    [ObservableProperty]
    private string city = "İstanbul";

    [ObservableProperty]
    private string address = "Atatürk Mah. Hal Cad. No:45 Kadıköy / İstanbul";

    [ObservableProperty]
    private string selectedLicenseType = "Kurumsal";

    public List<string> LicenseTypes { get; } = new()
    {
        "Standart Perakende Kasap",
        "Gurme Şarküteri & Et İşleme",
        "Kurumsal (Entegre Mezbaha ERP & Çiftlik)"
    };

    // Step 3: Setup Options & Target Path
    [ObservableProperty]
    private string installPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RoyKasap");

    [ObservableProperty]
    private bool createDesktopShortcut = true;

    [ObservableProperty]
    private bool createStartMenuShortcut = true;

    [ObservableProperty]
    private bool useSqlServer = false;

    [ObservableProperty]
    private bool installBackgroundServices = true;

    // Step 4: Installation Progress & Logs
    [ObservableProperty]
    private double progressPercentage = 0;

    [ObservableProperty]
    private string statusMessage = "Kurulum başlatılmaya hazır.";

    [ObservableProperty]
    private bool isInstalling;

    [ObservableProperty]
    private bool isInstallationFinished;

    [ObservableProperty]
    private bool isSuccess;

    public ObservableCollection<string> InstallationLogs { get; } = new();

    // Step 5: Completion & Launch
    [ObservableProperty]
    private bool launchAfterFinish = true;

    public event Action? CloseRequested;

    public SetupWizardViewModel()
    {
        InstallationLogs.Add("Roy Kasap Otomasyonu Kurulum Sihirbazı Başlatıldı.");
        InstallationLogs.Add($"Tarih/Saat: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
    }

    [RelayCommand]
    private void GoNext()
    {
        if (CurrentStep == 1 && !IsLicenseAccepted)
        {
            System.Windows.MessageBox.Show("Devam edebilmek için lütfen lisans sözleşmesini onaylayınız.", "Lisans Onayı", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (CurrentStep == 2 && string.IsNullOrWhiteSpace(CompanyName))
        {
            System.Windows.MessageBox.Show("Lütfen müşteri firma adını giriniz.", "Firma Adı Gereklidir", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (CurrentStep < 4)
        {
            CurrentStep++;
            if (CurrentStep == 4)
            {
                _ = RunInstallerAsync();
            }
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        if (CurrentStep > 1 && !IsInstalling)
        {
            CurrentStep--;
        }
    }

    [RelayCommand]
    private void BrowseInstallPath()
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Roy Kasap Kurulum Klasörünü Seçin",
            SelectedPath = InstallPath
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            InstallPath = dialog.SelectedPath;
        }
    }

    [RelayCommand]
    private async Task RunInstallerAsync()
    {
        IsInstalling = true;
        IsInstallationFinished = false;
        ProgressPercentage = 0;
        InstallationLogs.Clear();
        Log($"=== ROY KASAP OTOMASYONU KURULUMU BAŞLATILDI ===");
        Log($"Firma Ünvanı: {CompanyName}");
        Log($"Yetkili Kişi: {AuthorizedPerson}");
        Log($"Hedef Klasör: {InstallPath}");

        var config = new SetupConfig
        {
            CompanyName = CompanyName.Trim(),
            AuthorizedPerson = AuthorizedPerson.Trim(),
            Phone = Phone.Trim(),
            TaxOffice = TaxOffice.Trim(),
            TaxNumber = TaxNumber.Trim(),
            City = City.Trim(),
            Address = Address.Trim(),
            LicenseType = SelectedLicenseType,
            InstallPath = InstallPath.Trim(),
            CreateDesktopShortcut = CreateDesktopShortcut,
            CreateStartMenuShortcut = CreateStartMenuShortcut,
            UseSqlServer = UseSqlServer,
            InstallBackgroundServices = InstallBackgroundServices
        };

        IsSuccess = await _engine.ExecuteInstallationAsync(config, (pct, msg) =>
        {
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                ProgressPercentage = pct;
                StatusMessage = msg;
                Log(msg);
            });
        });

        IsInstalling = false;
        IsInstallationFinished = true;

        if (IsSuccess)
        {
            CurrentStep = 5;
        }
    }

    [RelayCommand]
    private void FinishSetup()
    {
        if (LaunchAfterFinish)
        {
            try
            {
                string mainExe = Path.Combine(InstallPath, "KasapOtomasyon.WPF.exe");
                if (!File.Exists(mainExe))
                {
                    var exes = Directory.GetFiles(InstallPath, "*.exe");
                    mainExe = exes.FirstOrDefault(x => x.Contains("WPF")) ?? exes.FirstOrDefault() ?? mainExe;
                }

                if (File.Exists(mainExe))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = mainExe,
                        WorkingDirectory = InstallPath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Uygulama başlatılırken hata oluştu: {ex.Message}", "Başlatma Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        CloseRequested?.Invoke();
    }

    [RelayCommand]
    private void CancelSetup()
    {
        if (IsInstalling)
        {
            var result = System.Windows.MessageBox.Show("Kurulum işlemi devam ediyor. Çıkmak istediğinize emin misiniz?", "Kurulum İptali", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;
        }

        CloseRequested?.Invoke();
    }

    private void Log(string message)
    {
        InstallationLogs.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
    }
}
