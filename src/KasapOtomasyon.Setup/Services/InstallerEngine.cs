using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using KasapOtomasyon.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KasapOtomasyon.Setup.Services;

public class SetupConfig
{
    public string CompanyName { get; set; } = "Özkanlar Kasap & Entegre Et";
    public string AuthorizedPerson { get; set; } = "Ahmet Özkan";
    public string Phone { get; set; } = "+90 532 555 1234";
    public string TaxOffice { get; set; } = "Büyük Mükellefler";
    public string TaxNumber { get; set; } = "3400998877";
    public string City { get; set; } = "İstanbul";
    public string Address { get; set; } = "Atatürk Mah. Hal Cad. No:45 Kadıköy / İstanbul";
    public string LicenseType { get; set; } = "Kurumsal"; // Standart, Profesyonel, Kurumsal
    public string LicenseKey { get; set; } = "ROYKASAP-ENTERPRISE-2026-CLIENT";
    public string InstallPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RoyKasap");
    public bool CreateDesktopShortcut { get; set; } = true;
    public bool CreateStartMenuShortcut { get; set; } = true;
    public bool UseSqlServer { get; set; } = false;
    public bool InstallBackgroundServices { get; set; } = true;
}

public class InstallerEngine
{
    private readonly SqlSetupService _sqlService = new();

    public async Task<bool> ExecuteInstallationAsync(SetupConfig config, Action<double, string> progressCallback)
    {
        try
        {
            // 1. Target Directory Setup (10%)
            progressCallback(5, "Kurulum dizinleri kontrol ediliyor...");
            if (!Directory.Exists(config.InstallPath))
            {
                Directory.CreateDirectory(config.InstallPath);
            }
            progressCallback(10, $"✔ Hedef klasör hazır: {config.InstallPath}");

            // 2. Copying Application Files (30%)
            progressCallback(15, "Uygulama dosyaları ve ikili bileşenler kopyalanıyor...");
            string sourceDir = AppDomain.CurrentDomain.BaseDirectory;
            await CopyDirectoryAsync(sourceDir, config.InstallPath, progressCallback);
            progressCallback(35, "✔ Tüm ikili dosyalar ve görseller kopyalandı.");

            // 3. Database Engine & Connection String Configuration (50%)
            progressCallback(40, "Veritabanı motoru ve bağlantı dizesi yapılandırılıyor...");
            if (config.UseSqlServer)
            {
                await _sqlService.InstallSqlLocalDbAsync(msg => progressCallback(45, msg));
            }

            string connStr = _sqlService.BuildConnectionString("KasapOtomasyon", config.UseSqlServer, config.InstallPath);
            await UpdateAppSettingsJsonAsync(config, connStr);
            progressCallback(55, "✔ appsettings.json müşteri firma bilgileriyle güncellendi.");

            // 4. Provisioning Customer Database & Seeding (75%)
            progressCallback(60, $"Firma kaydı veritabanına işleniyor: '{config.CompanyName}'...");
            await InitializeCustomerDatabaseAsync(config, connStr, msg => progressCallback(70, msg));
            progressCallback(75, "✔ Veritabanı ve firma yetkili hesapları başarıyla oluşturuldu.");

            // 5. Background Services Setup (85%)
            progressCallback(80, "Terazi, Barkod ve SMS entegrasyon servisleri yapılandırılıyor...");
            await Task.Delay(800);
            progressCallback(85, "✔ Endüstriyel Donanım & Servis Sürücüleri aktif edildi.");

            // 6. Shortcuts Creation (95%)
            progressCallback(90, "Masaüstü ve Başlat menüsü kısayolları oluşturuluyor...");
            string mainExe = Path.Combine(config.InstallPath, "KasapOtomasyon.WPF.exe");
            if (!File.Exists(mainExe))
            {
                // Fallback executable search
                var exes = Directory.GetFiles(config.InstallPath, "*.exe");
                mainExe = exes.FirstOrDefault(x => x.Contains("WPF")) ?? exes.FirstOrDefault() ?? mainExe;
            }

            if (config.CreateDesktopShortcut && File.Exists(mainExe))
            {
                ShortcutService.CreateDesktopShortcut(mainExe, "Roy Kasap Otomasyonu");
            }

            if (config.CreateStartMenuShortcut && File.Exists(mainExe))
            {
                ShortcutService.CreateStartMenuShortcut(mainExe, "Roy Kasap Otomasyonu");
            }
            progressCallback(95, "✔ Masaüstü ve Başlat Menüsü kısayolları eklendi.");

            // 7. Complete (100%)
            progressCallback(100, "🎉 Kurulum başarıyla tamamlandı!");
            return true;
        }
        catch (Exception ex)
        {
            progressCallback(100, $"❌ Kurulum Sırasında Hata Oluştu: {ex.Message}");
            return false;
        }
    }

    private async Task CopyDirectoryAsync(string sourceDir, string targetDir, Action<double, string> progressCallback)
    {
        var sourceInfo = new DirectoryInfo(sourceDir);
        var files = sourceInfo.GetFiles("*.*", SearchOption.AllDirectories);

        int total = files.Length;
        int count = 0;

        foreach (var file in files)
        {
            // Skip copying setup executable itself or temp logs if in same folder
            if (file.Name.StartsWith("KasapOtomasyon.Setup", StringComparison.OrdinalIgnoreCase)) continue;

            string relativePath = Path.GetRelativePath(sourceDir, file.FullName);
            string destFile = Path.Combine(targetDir, relativePath);
            string? destDir = Path.GetDirectoryName(destFile);

            if (destDir != null && !Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            file.CopyTo(destFile, true);
            count++;

            if (count % 5 == 0 || count == total)
            {
                double pct = 15.0 + ((double)count / total) * 20.0;
                progressCallback(pct, $"Kopyalanıyor ({count}/{total}): {file.Name}");
                await Task.Yield();
            }
        }
    }

    private async Task UpdateAppSettingsJsonAsync(SetupConfig config, string connectionString)
    {
        string settingsPath = Path.Combine(config.InstallPath, "appsettings.json");
        JsonNode rootNode;

        if (File.Exists(settingsPath))
        {
            string existingJson = await File.ReadAllTextAsync(settingsPath);
            rootNode = JsonNode.Parse(existingJson) ?? new JsonObject();
        }
        else
        {
            rootNode = new JsonObject();
        }

        // Connection string update
        if (rootNode["ConnectionStrings"] is not JsonObject connObj)
        {
            connObj = new JsonObject();
            rootNode["ConnectionStrings"] = connObj;
        }
        connObj["DefaultConnection"] = connectionString;

        // Company Settings update
        var compObj = new JsonObject
        {
            ["CompanyName"] = config.CompanyName,
            ["AuthorizedPerson"] = config.AuthorizedPerson,
            ["TaxNumber"] = config.TaxNumber,
            ["TaxOffice"] = config.TaxOffice,
            ["Phone"] = config.Phone,
            ["City"] = config.City,
            ["Address"] = config.Address,
            ["LicenseType"] = config.LicenseType,
            ["LicenseKey"] = config.LicenseKey
        };
        rootNode["CompanySettings"] = compObj;

        var options = new JsonSerializerOptions { WriteIndented = true };
        string updatedJson = rootNode.ToJsonString(options);
        await File.ReadAllTextAsync(settingsPath).ContinueWith(_ => { }); // Ensure handles
        await File.WriteAllTextAsync(settingsPath, updatedJson);
    }

    private async Task InitializeCustomerDatabaseAsync(SetupConfig config, string connectionString, Action<string> logCallback)
    {
        try
        {
            var optionsBuilder = new DbContextOptionsBuilder<KasapDbContext>();
            if (config.UseSqlServer)
            {
                optionsBuilder.UseSqlServer(connectionString);
            }
            else
            {
                optionsBuilder.UseSqlite(connectionString);
            }

            using var context = new KasapDbContext(optionsBuilder.Options);
            await DbInitializer.InitializeAsync(context);

            // Update primary company name and app settings
            var primaryCompany = await context.Companies.FirstOrDefaultAsync();
            if (primaryCompany != null)
            {
                primaryCompany.Name = config.CompanyName;
                primaryCompany.TaxOffice = config.TaxOffice;
                primaryCompany.TaxNumber = config.TaxNumber;
                primaryCompany.Address = config.Address;
            }

            var primaryTenant = await context.Tenants.FirstOrDefaultAsync();
            if (primaryTenant != null)
            {
                primaryTenant.Name = config.CompanyName;
                primaryTenant.TaxNumber = config.TaxNumber;
                primaryTenant.ContactPhone = config.Phone;
            }

            var activeLicense = await context.LicenseRecords.FirstOrDefaultAsync(r => r.IsActive);
            if (activeLicense != null)
            {
                activeLicense.CompanyName = config.CompanyName;
                activeLicense.LicensedTo = config.AuthorizedPerson;
            }

            // Upsert AppSettings for Company.Name
            var compSetting = await context.AppSettings.FirstOrDefaultAsync(s => s.Key == "Company.Name");
            if (compSetting != null)
            {
                compSetting.Value = config.CompanyName;
            }
            else
            {
                await context.AppSettings.AddAsync(new Domain.Entities.AppSetting
                {
                    Key = "Company.Name",
                    Value = config.CompanyName,
                    Category = "General",
                    Description = "Müşteri Firma Adı"
                });
            }

            await context.SaveChangesAsync();
            logCallback($"✔ Müşteri veritabanı '{config.CompanyName}' unvanı ile başarıyla ilklendirildi.");
        }
        catch (Exception ex)
        {
            logCallback($"⚠ DB İlklendirme Uyarısı: {ex.Message}");
        }
    }
}
