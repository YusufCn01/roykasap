using KasapOtomasyon.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KasapOtomasyon.Infrastructure.Data;

public class KasapDbContext : DbContext
{
    public KasapDbContext(DbContextOptions<KasapDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<BarcodeDefinition> BarcodeDefinitions => Set<BarcodeDefinition>();
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<ProductPrice> ProductPrices => Set<ProductPrice>();

    public DbSet<AnimalLot> AnimalLots => Set<AnimalLot>();
    public DbSet<CuttingTemplate> CuttingTemplates => Set<CuttingTemplate>();
    public DbSet<CuttingTemplateItem> CuttingTemplateItems => Set<CuttingTemplateItem>();
    public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();
    public DbSet<ProductionInput> ProductionInputs => Set<ProductionInput>();
    public DbSet<ProductionOutput> ProductionOutputs => Set<ProductionOutput>();

    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<SalePayment> SalePayments => Set<SalePayment>();

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerTransaction> CustomerTransactions => Set<CustomerTransaction>();

    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<StockCount> StockCounts => Set<StockCount>();
    public DbSet<StockCountItem> StockCountItems => Set<StockCountItem>();
    public DbSet<WarehouseTransfer> WarehouseTransfers => Set<WarehouseTransfer>();
    public DbSet<WarehouseTransferItem> WarehouseTransferItems => Set<WarehouseTransferItem>();

    public DbSet<CashRegister> CashRegisters => Set<CashRegister>();
    public DbSet<CashSession> CashSessions => Set<CashSession>();
    public DbSet<CashTransaction> CashTransactions => Set<CashTransaction>();

    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
    public DbSet<PurchaseInvoiceLine> PurchaseInvoiceLines => Set<PurchaseInvoiceLine>();

    public DbSet<LicenseRecord> LicenseRecords => Set<LicenseRecord>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    // Enterprise Multi-Tenant & Multi-Site DbSets
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Plant> Plants => Set<Plant>();
    public DbSet<FacilityUnit> FacilityUnits => Set<FacilityUnit>();

    // Livestock & Transport
    public DbSet<Farm> Farms => Set<Farm>();
    public DbSet<TransportVehicle> TransportVehicles => Set<TransportVehicle>();
    public DbSet<TransportDriver> TransportDrivers => Set<TransportDriver>();
    public DbSet<LivestockTransportTrip> LivestockTransportTrips => Set<LivestockTransportTrip>();

    // Mezbaha ERP & Traceability DbSets
    public DbSet<AnimalBreed> AnimalBreeds => Set<AnimalBreed>();
    public DbSet<AnimalIntake> AnimalIntakes => Set<AnimalIntake>();
    public DbSet<VeterinaryCheck> VeterinaryChecks => Set<VeterinaryCheck>();
    public DbSet<SlaughterRecord> SlaughterRecords => Set<SlaughterRecord>();
    public DbSet<SlaughterServiceInvoice> SlaughterServiceInvoices => Set<SlaughterServiceInvoice>();
    public DbSet<WasteLog> WasteLogs => Set<WasteLog>();
    public DbSet<ColdStorageRoom> ColdStorageRooms => Set<ColdStorageRoom>();
    public DbSet<ForensicAnomalyRecord> ForensicAnomalyRecords => Set<ForensicAnomalyRecord>();
    public DbSet<CarcassDeboningCut> CarcassDeboningCuts => Set<CarcassDeboningCut>();

    // MES, Cutting & Mass Balance
    public DbSet<CarcassPortion> CarcassPortions => Set<CarcassPortion>();
    public DbSet<ChillingRecord> ChillingRecords => Set<ChillingRecord>();
    public DbSet<CuttingOrder> CuttingOrders => Set<CuttingOrder>();
    public DbSet<IoTTelemetryLog> IoTTelemetryLogs => Set<IoTTelemetryLog>();

    // Quality, HACCP & Food Recall
    public DbSet<HaccpControlPoint> HaccpControlPoints => Set<HaccpControlPoint>();
    public DbSet<HaccpInspectionRecord> HaccpInspectionRecords => Set<HaccpInspectionRecord>();
    public DbSet<FoodRecallCase> FoodRecallCases => Set<FoodRecallCase>();
    public DbSet<FoodRecallItem> FoodRecallItems => Set<FoodRecallItem>();

    // Processing & Recipe
    public DbSet<ProcessingRecipe> ProcessingRecipes => Set<ProcessingRecipe>();
    public DbSet<ProcessingRecipeItem> ProcessingRecipeItems => Set<ProcessingRecipeItem>();
    public DbSet<PalletSSCC> PalletSSCCs => Set<PalletSSCC>();
    public DbSet<MeatProcessingEquipment> MeatProcessingEquipments => Set<MeatProcessingEquipment>();
    public DbSet<AuxiliaryMaterialStock> AuxiliaryMaterialStocks => Set<AuxiliaryMaterialStock>();
    public DbSet<ReprocessingQualityCheck> ReprocessingQualityChecks => Set<ReprocessingQualityCheck>();
    public DbSet<SucukCuringBatch> SucukCuringBatches => Set<SucukCuringBatch>();
    public DbSet<ReprocessingDisposalRecord> ReprocessingDisposalRecords => Set<ReprocessingDisposalRecord>();
    public DbSet<ButcherWasteIncentive> ButcherWasteIncentives => Set<ButcherWasteIncentive>();

    // True Cost & Anti-Fraud
    public DbSet<TrueCostRollup> TrueCostRollups => Set<TrueCostRollup>();
    public DbSet<RetailScaleBarcodeRule> RetailScaleBarcodeRules => Set<RetailScaleBarcodeRule>();
    public DbSet<FraudInvestigationCase> FraudInvestigationCases => Set<FraudInvestigationCase>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Precision configuration for monetary and weight decimals
        foreach (var property in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetPrecision(18);
            property.SetScale(4); // Supports accurate kg (e.g. 1.255 kg) and currency amounts
        }

        // Product Relationships
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UnitOfMeasure)
                .WithMany()
                .HasForeignKey(e => e.UnitOfMeasureId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Barcode index
        modelBuilder.Entity<BarcodeDefinition>(entity =>
        {
            entity.HasIndex(e => e.Barcode);
            entity.HasOne(e => e.Product)
                .WithMany(p => p.Barcodes)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Production Order Relationships
        modelBuilder.Entity<ProductionOrder>(entity =>
        {
            entity.HasOne(e => e.AnimalLot)
                .WithMany(l => l.ProductionOrders)
                .HasForeignKey(e => e.LotId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CuttingTemplate)
                .WithMany(t => t.ProductionOrders)
                .HasForeignKey(e => e.CuttingTemplateId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Sale Relationships
        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasIndex(e => e.ReceiptNumber).IsUnique();
            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Sales)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Stock Item unique constraint on Warehouse + Product + Lot
        modelBuilder.Entity<StockItem>(entity =>
        {
            entity.HasOne(e => e.Warehouse)
                .WithMany(w => w.StockItems)
                .HasForeignKey(e => e.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.StockItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // User unique username
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Tenant Hierarchy
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Companies)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Plant>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasOne(e => e.Company)
                .WithMany(c => c.Plants)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FacilityUnit>(entity =>
        {
            entity.HasIndex(e => e.Code);
            entity.HasOne(e => e.Plant)
                .WithMany(p => p.FacilityUnits)
                .HasForeignKey(e => e.PlantId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
