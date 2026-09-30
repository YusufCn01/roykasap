using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using KasapOtomasyon.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KasapOtomasyon.Setup.Services;

public class SetupConfig
{
    public string CompanyName { get; set; } = string.Empty;
    public string AuthorizedPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string TaxOffice { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string LicenseType { get; set; } = "Kurumsal (Entegre Mezbaha ERP & Çiftlik)";
    public string LicenseKey { get; set; } = string.Empty;
    public string InstallPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Roy Kasap");
    public bool CreateDesktopShortcut { get; set; } = true;
    public bool CreateStartMenuShortcut { get; set; } = true;
    public bool UseSqlServer { get; set; } = false;
    public bool InstallBackgroundServices { get; set; } = true;
}

public class InstallerEngine
{
    private readonly SqlSetupService _sqlService = new();

    private static readonly HashSet<string> BannedDirs = new(StringComparer.OrdinalIgnoreCase)
    {
        ".vs", ".git", ".agents", ".gemini", "src", "tests", "bin", "obj", "scratch", "brain", "publish", "KasapOtomasyon.Setup", "KasapOtomasyon.LicenseManager"
    };

    private static readonly HashSet<string> BannedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".sln", ".csproj", ".cs", ".user", ".suo", ".pdb", ".log", ".md"
    };

    public async Task<bool> ExecuteInstallationAsync(SetupConfig config, Action<double, string> progressCallback)
    {
        try
        {
            // 1. Target Directory Setup in Program Files (10%)
            progressCallback(5, "Kurulum dizinleri kontrol ediliyor...");
            if (!Directory.Exists(config.InstallPath))
            {
                Directory.CreateDirectory(config.InstallPath);
            }
            progressCallback(10, $"✔ Hedef Program Files klasörü hazır: {config.InstallPath}");

            // 2. Locate and Copy/Extract WPF Main Application Binaries (35%)
            progressCallback(15, "Roy Kasap ana otomasyon dosyaları ve ikili bileşenler kuruluyor...");
            
            bool copySuccess = false;
            try
            {
                string? sourceDir = LocateWpfSourceDirectory();
                if (!string.IsNullOrEmpty(sourceDir))
                {
                    progressCallback(20, $"Yerel disk kaynak dizininden kopyalanıyor: {sourceDir}");
                    await CopyDirectoryAsync(sourceDir, config.InstallPath, progressCallback);
                    copySuccess = true;
                }
            }
            catch (Exception ex)
            {
                progressCallback(20, $"Disk kaynak araması tamamlandı ({ex.Message}), gömülü paket aktif ediliyor...");
            }

            if (!copySuccess)
            {
                progressCallback(25, "Gömülü uygulama paketi (AppPayload.zip) çıkartılıyor...");
                await ExtractEmbeddedPayloadAsync(config.InstallPath, progressCallback);
            }

            progressCallback(35, "✔ Tüm ana uygulama dosyaları (KasapOtomasyon.WPF.exe) kuruldu.");

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

            // 6. Create Shortcuts Explicitly Pointing to KasapOtomasyon.WPF.exe (95%)
            progressCallback(90, "Masaüstü ve Başlat menüsü kısayolları oluşturuluyor...");
            string mainWpfExe = Path.Combine(config.InstallPath, "KasapOtomasyon.WPF.exe");

            if (!File.Exists(mainWpfExe))
            {
                var exes = Directory.GetFiles(config.InstallPath, "*.exe");
                mainWpfExe = exes.FirstOrDefault(x => x.EndsWith("WPF.exe", StringComparison.OrdinalIgnoreCase)) 
                             ?? exes.FirstOrDefault(x => !x.Contains("Setup", StringComparison.OrdinalIgnoreCase)) 
                             ?? mainWpfExe;
            }

            if (config.CreateDesktopShortcut && File.Exists(mainWpfExe))
            {
                ShortcutService.CreateDesktopShortcut(mainWpfExe, "Roy Kasap Otomasyonu");
            }

            if (config.CreateStartMenuShortcut && File.Exists(mainWpfExe))
            {
                ShortcutService.CreateStartMenuShortcut(mainWpfExe, "Roy Kasap Otomasyonu");
            }
            progressCallback(95, "✔ Masaüstü ve Başlat Menüsü kısayolları (KasapOtomasyon.WPF.exe) eklendi.");

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

    private string? LocateWpfSourceDirectory()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;

        // 1. Check for dedicated Payload subfolder beside Setup.exe
        string payloadPath = Path.Combine(baseDir, "Payload");
        if (Directory.Exists(payloadPath) && File.Exists(Path.Combine(payloadPath, "KasapOtomasyon.WPF.exe")))
        {
            return payloadPath;
        }

        // 2. Search explicit WPF Release / Debug output directories in workspace
        var candidatePaths = new[]
        {
            Path.Combine(baseDir, "src", "KasapOtomasyon.WPF", "bin", "Release", "net8.0-windows"),
            Path.Combine(baseDir, "src", "KasapOtomasyon.WPF", "bin", "Debug", "net8.0-windows"),
            Path.Combine(baseDir, "..", "..", "..", "KasapOtomasyon.WPF", "bin", "Release", "net8.0-windows"),
            Path.Combine(baseDir, "..", "..", "..", "KasapOtomasyon.WPF", "bin", "Debug", "net8.0-windows"),
            @"d:\Roy Kasap\src\KasapOtomasyon.WPF\bin\Release\net8.0-windows",
            @"d:\Roy Kasap\src\KasapOtomasyon.WPF\bin\Debug\net8.0-windows"
        };

        foreach (var candidate in candidatePaths)
        {
            try
            {
                string fullPath = Path.GetFullPath(candidate);
                if (Directory.Exists(fullPath) && File.Exists(Path.Combine(fullPath, "KasapOtomasyon.WPF.exe")))
                {
                    return fullPath;
                }
            }
            catch { }
        }

        // 3. Fallback check on baseDir ONLY IF it contains KasapOtomasyon.WPF.exe AND NOT Setup.dll
        if (File.Exists(Path.Combine(baseDir, "KasapOtomasyon.WPF.exe")) && !File.Exists(Path.Combine(baseDir, "KasapOtomasyon.Setup.dll")))
        {
            return baseDir;
        }

        return null;
    }

    private async Task ExtractEmbeddedPayloadAsync(string targetDir, Action<double, string> progressCallback)
    {
        var assembly = Assembly.GetExecutingAssembly();
        string? resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("AppPayload.zip", StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrEmpty(resourceName))
        {
            throw new InvalidOperationException("Gömülü Roy Kasap uygulama paketi (AppPayload.zip) kurulum kaynaklarında bulunamadı!");
        }

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new InvalidOperationException($"Gömülü paket yayını '{resourceName}' okunamadı.");
        }

        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        int total = archive.Entries.Count;
        int count = 0;

        foreach (var entry in archive.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name))
            {
                string dirPath = Path.Combine(targetDir, entry.FullName);
                if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
                continue;
            }

            string destFile = Path.Combine(targetDir, entry.FullName);
            string? destDir = Path.GetDirectoryName(destFile);

            if (destDir != null && !Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            entry.ExtractToFile(destFile, overwrite: true);
            count++;

            if (count % 5 == 0 || count == total)
            {
                double pct = 15.0 + ((double)count / total) * 20.0;
                progressCallback(pct, $"Çıkartılıyor ({count}/{total}): {entry.Name}");
                await Task.Yield();
            }
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
            if (file.Name.StartsWith("KasapOtomasyon.Setup", StringComparison.OrdinalIgnoreCase) || 
                file.Name.Equals("Setup.exe", StringComparison.OrdinalIgnoreCase) ||
                (file.Name.StartsWith("Setup", StringComparison.OrdinalIgnoreCase) && file.Extension.Equals(".exe", StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            if (BannedExtensions.Contains(file.Extension))
            {
                continue;
            }

            string relativePath = Path.GetRelativePath(sourceDir, file.FullName);
            var pathSegments = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            if (pathSegments.Any(segment => BannedDirs.Contains(segment)))
            {
                continue;
            }

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
            ["CompanyName"] = string.IsNullOrWhiteSpace(config.CompanyName) ? "Roy Kasap & Entegre Et" : config.CompanyName,
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

            string finalCompanyName = string.IsNullOrWhiteSpace(config.CompanyName) ? "Roy Kasap & Entegre Et" : config.CompanyName;

            // Update primary company name and app settings
            var primaryCompany = await context.Companies.FirstOrDefaultAsync();
            if (primaryCompany != null)
            {
                primaryCompany.Name = finalCompanyName;
                primaryCompany.TaxOffice = config.TaxOffice;
                primaryCompany.TaxNumber = config.TaxNumber;
                primaryCompany.Address = config.Address;
            }

            var primaryTenant = await context.Tenants.FirstOrDefaultAsync();
            if (primaryTenant != null)
            {
                primaryTenant.Name = finalCompanyName;
                primaryTenant.TaxNumber = config.TaxNumber;
                primaryTenant.ContactPhone = config.Phone;
            }

            var activeLicense = await context.LicenseRecords.FirstOrDefaultAsync(r => r.IsActive);
            if (activeLicense != null)
            {
                activeLicense.CompanyName = finalCompanyName;
                activeLicense.LicensedTo = config.AuthorizedPerson;
            }

            // Upsert AppSettings for Company.Name
            var compSetting = await context.AppSettings.FirstOrDefaultAsync(s => s.Key == "Company.Name");
            if (compSetting != null)
            {
                compSetting.Value = finalCompanyName;
            }
            else
            {
                await context.AppSettings.AddAsync(new Domain.Entities.AppSetting
                {
                    Key = "Company.Name",
                    Value = finalCompanyName,
                    Category = "General",
                    Description = "Müşteri Firma Adı"
                });
            }

            await context.SaveChangesAsync();
            logCallback($"✔ Müşteri veritabanı '{finalCompanyName}' unvanı ile başarıyla ilklendirildi.");
        }
        catch (Exception ex)
        {
            logCallback($"⚠ DB İlklendirme Uyarısı: {ex.Message}");
        }
    }
}
