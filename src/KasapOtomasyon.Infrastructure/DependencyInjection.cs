using KasapOtomasyon.Application.Interfaces;
using KasapOtomasyon.Application.Interfaces.Adapters;
using KasapOtomasyon.Infrastructure.Adapters;
using KasapOtomasyon.Infrastructure.Data;
using KasapOtomasyon.Infrastructure.Licensing;
using KasapOtomasyon.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace KasapOtomasyon.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration? configuration = null)
    {
        // 1. Logger Setup
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File(
                path: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KasapOtomasyon", "Logs", "kasap_log_.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30)
            .CreateLogger();

        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

        // 2. DbContext Setup (LocalDB / SQLite flexible support)
        var connectionString = configuration?.GetConnectionString("DefaultConnection");
        var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KasapOtomasyon");
        if (!Directory.Exists(appDataFolder))
        {
            Directory.CreateDirectory(appDataFolder);
        }
        var sqliteDbPath = Path.Combine(appDataFolder, "KasapOtomasyon.db");
        var sqliteConnStr = $"Data Source={sqliteDbPath}";

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = sqliteConnStr;
            services.AddDbContext<KasapDbContext>(options => options.UseSqlite(connectionString));
        }
        else if (connectionString.Contains("Server=") || connectionString.Contains("Data Source=(localdb)"))
        {
            services.AddDbContext<KasapDbContext>(options => options.UseSqlServer(connectionString));
        }
        else
        {
            services.AddDbContext<KasapDbContext>(options => options.UseSqlite(connectionString));
        }

        // 3. Hardware & Licensing
        services.AddSingleton<HardwareFingerprintProvider>();
        services.AddScoped<RsaLicenseValidator>();
        services.AddScoped<ILicenseService, LicenseService>();

        // 4. Hardware Adapters
        services.AddSingleton<MockScaleAdapter>();
        services.AddSingleton<CasScaleAdapter>();
        services.AddSingleton<ITeraziAdapter, MockScaleAdapter>(); // Defaults to simulator for instant usability

        // 5. External Integration Adapters
        services.AddSingleton<ISmsService, MockSmsService>();
        services.AddSingleton<IEInvoicerAdapter, MockEInvoicerAdapter>();

        // 6. Application Business Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBarcodeService, BarcodeService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<IProductionService, ProductionService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IFinanceService, FinanceService>();
        services.AddScoped<IPurchaseInvoiceService, PurchaseInvoiceService>();
        services.AddScoped<ICashRegisterService, CashRegisterService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IBackupService, BackupService>();
        services.AddScoped<ISlaughterhouseService, SlaughterhouseService>();
        services.AddScoped<ITraceabilityService, TraceabilityService>();
        services.AddScoped<IForensicAnomalyService, ForensicAnomalyService>();
        services.AddScoped<IScaleIntegrationService, ScaleIntegrationService>();
        services.AddScoped<IGs1LabelService, Gs1LabelService>();
        services.AddScoped<IAnimalPassportPdfService, AnimalPassportPdfService>();
        services.AddSingleton<ILocalizationService, LocalizationService>();

        // Enterprise Meat ERP Modules (Phases 3-16)
        services.AddScoped<IMassBalanceAndCuttingService, MassBalanceAndCuttingService>();
        services.AddScoped<IFoodRecallAndHaccpService, FoodRecallAndHaccpService>();
        services.AddScoped<ITrueCostEngineService, TrueCostEngineService>();
        services.AddScoped<IAntiFraudEngineService, AntiFraudEngineService>();
        services.AddScoped<IMeatProcessingService, MeatProcessingService>();
        services.AddScoped<IWmsPalletLogisticsService, WmsPalletLogisticsService>();
        services.AddScoped<IShelfLifeReprocessingService, ShelfLifeReprocessingService>();

        return services;
    }
}
