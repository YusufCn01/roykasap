using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.Domain.Enums;
using KasapOtomasyon.Infrastructure.Licensing;
using Microsoft.Win32;

namespace KasapOtomasyon.LicenseManager.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string companyName = "Örnek Kasap & Et Entegre Ltd. Şti.";

    [ObservableProperty]
    private string licensedTo = "Ahmet Yılmaz";

    [ObservableProperty]
    private string machineFingerprint = string.Empty;

    [ObservableProperty]
    private LicenseType selectedLicenseType = LicenseType.Profesyonel;

    [ObservableProperty]
    private DateTime validUntil = DateTime.Today.AddYears(1);

    [ObservableProperty]
    private int maxTerminals = 3;

    // Feature Flags
    [ObservableProperty]
    private bool featurePos = true;

    [ObservableProperty]
    private bool featureStock = true;

    [ObservableProperty]
    private bool featureFinance = true;

    [ObservableProperty]
    private bool featureMezbaha = true;

    [ObservableProperty]
    private bool featureEInvoice = true;

    [ObservableProperty]
    private bool featureSms = true;

    [ObservableProperty]
    private string generatedLicenseJson = string.Empty;

    [ObservableProperty]
    private string statusMessage = "Lisans üretmek için müşteri donanım kimliğini girin.";

    [ObservableProperty]
    private bool isSuccess = true;

    public IEnumerable<LicenseType> LicenseTypes => Enum.GetValues<LicenseType>();

    public MainViewModel()
    {
        // Try getting current machine fingerprint as initial template
        try
        {
            var provider = new HardwareFingerprintProvider();
            MachineFingerprint = provider.GetMachineFingerprint().Fingerprint;
        }
        catch { }
    }

    [RelayCommand]
    private void GenerateLicense()
    {
        if (string.IsNullOrWhiteSpace(MachineFingerprint))
        {
            StatusMessage = "Hata: Müşteri Donanım Kimliği (Machine Fingerprint) boş bırakılamaz!";
            IsSuccess = false;
            return;
        }

        var enabledFeatures = new List<string>();
        if (FeaturePos) enabledFeatures.Add("POS_SALE");
        if (FeatureStock) enabledFeatures.Add("STOCK_MANAGEMENT");
        if (FeatureFinance) enabledFeatures.Add("FINANCE");
        if (FeatureMezbaha) enabledFeatures.Add("MEZBAHA_MODULE");
        if (FeatureEInvoice) enabledFeatures.Add("E_INVOICE");
        if (FeatureSms) enabledFeatures.Add("SMS_REMINDER");

        var payload = new LicensePayloadDto
        {
            CompanyName = CompanyName,
            LicensedTo = LicensedTo,
            MachineFingerprint = MachineFingerprint.Trim(),
            LicenseType = SelectedLicenseType,
            IssuedAt = DateTime.UtcNow,
            ValidUntil = ValidUntil.ToUniversalTime(),
            MaxTerminals = MaxTerminals,
            EnabledFeatures = enabledFeatures,
            Issuer = "Kasap & Mezbaha Otomasyonu Lisans Merkezi"
        };

        var payloadJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
        var signature = RsaLicenseValidator.SignData(payloadJson, RsaKeyConstants.MasterPrivateKeyXml);

        var licenseFile = new LicenseFileStructure
        {
            PayloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payloadJson)),
            SignatureBase64 = signature
        };

        GeneratedLicenseJson = JsonSerializer.Serialize(licenseFile, new JsonSerializerOptions { WriteIndented = true });
        StatusMessage = $"✓ Lisans başarıyla üretildi! ({SelectedLicenseType} - {ValidUntil:dd.MM.yyyy} tarihine kadar geçerli)";
        IsSuccess = true;
    }

    [RelayCommand]
    private void SaveLicenseFile()
    {
        if (string.IsNullOrEmpty(GeneratedLicenseJson))
        {
            GenerateLicense();
        }

        if (string.IsNullOrEmpty(GeneratedLicenseJson)) return;

        var saveDialog = new SaveFileDialog
        {
            Filter = "Kasap Lisans Dosyası (*.lic)|*.lic|JSON Dosyası (*.json)|*.json",
            FileName = $"KasapLisans_{CompanyName.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.lic"
        };

        if (saveDialog.ShowDialog() == true)
        {
            File.WriteAllText(saveDialog.FileName, GeneratedLicenseJson);
            StatusMessage = $"✓ Lisans dosyası kaydedildi: {Path.GetFileName(saveDialog.FileName)}";
            IsSuccess = true;
            MessageBox.Show("Lisans dosyası (.lic) başarıyla oluşturuldu!\nMüşteriye iletebilirsiniz.", "Lisans Üretildi", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    private void CopyToClipboard()
    {
        if (!string.IsNullOrEmpty(GeneratedLicenseJson))
        {
            Clipboard.SetText(GeneratedLicenseJson);
            StatusMessage = "✓ Lisans metni panoya kopyalandı.";
            IsSuccess = true;
        }
    }
}
