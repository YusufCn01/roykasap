using KasapOtomasyon.Domain.Entities;
using KasapOtomasyon.Domain.Enums;

namespace KasapOtomasyon.Application.DTOs;

public class ExpiringBatchDto
{
    public int StockItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string LotNumber { get; set; } = string.Empty;
    public string ShelfLocation { get; set; } = string.Empty;
    public decimal CurrentQuantityKg { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalValue => Math.Round(CurrentQuantityKg * UnitCost, 2);
    
    public DateTime? GeneralExpiryDate { get; set; }
    public DateTime ShelfDisplayExpiryDate { get; set; }
    public double DaysRemaining { get; set; }
    
    public string UrgencyCode { get; set; } = "NORMAL"; // CRITICAL, WARNING, NORMAL
    public string UrgencyBadge { get; set; } = string.Empty;
    public string SuggestedAction { get; set; } = string.Empty;
}

public class ProcessingRecipeDto
{
    public int Id { get; set; }
    public string RecipeCode { get; set; } = string.Empty;
    public string RecipeName { get; set; } = string.Empty;
    public RecipeProductCategory Category { get; set; }
    public string CategoryDisplayName { get; set; } = string.Empty;
    public int? TargetProductId { get; set; }
    public string TargetProductName { get; set; } = string.Empty;
    public decimal StandardBatchWeightKg { get; set; } = 100.0m;
    public decimal ExpectedYieldPercentage { get; set; } = 92.0m;
    public int ShelfLifeExtensionDays { get; set; } = 45;
    public string Description { get; set; } = string.Empty;
    public string ProcessInstructions { get; set; } = string.Empty;
    public List<ProcessingRecipeItemDto> Items { get; set; } = new();
}

public class ProcessingRecipeItemDto
{
    public int Id { get; set; }
    public RecipeItemType ItemType { get; set; }
    public string ItemTypeDisplayName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal PercentageRatio { get; set; }
    public decimal QuantityPerBatchKg { get; set; }
    public decimal StandardUnitCost { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class CustomRecipeInputDto
{
    public int Id { get; set; }
    public string RecipeCode { get; set; } = string.Empty;
    public string RecipeName { get; set; } = string.Empty;
    public RecipeProductCategory Category { get; set; } = RecipeProductCategory.FreshSausage;
    public int? TargetProductId { get; set; }
    public string TargetProductName { get; set; } = string.Empty;
    public decimal StandardBatchWeightKg { get; set; } = 100.0m;
    public decimal ExpectedYieldPercentage { get; set; } = 95.0m;
    public int ShelfLifeExtensionDays { get; set; } = 14;
    public string Description { get; set; } = string.Empty;
    public string ProcessInstructions { get; set; } = string.Empty;
    public List<CustomRecipeItemInputDto> Items { get; set; } = new();
}

public class CustomRecipeItemInputDto
{
    public int Id { get; set; }
    public RecipeItemType ItemType { get; set; } = RecipeItemType.SpiceMix;
    public string ItemName { get; set; } = string.Empty;
    public decimal PercentageRatio { get; set; } = 5.0m;
    public decimal StandardUnitCost { get; set; } = 50.0m;
    public string Description { get; set; } = string.Empty;
}

public class ReprocessingRequiredIngredientDto
{
    public string ItemName { get; set; } = string.Empty;
    public RecipeItemType ItemType { get; set; }
    public decimal PercentageRatio { get; set; }
    public decimal RequiredWeightKg { get; set; }
    public decimal UnitCost { get; set; }
    public decimal SubtotalCost => Math.Round(RequiredWeightKg * UnitCost, 2);
    public string Description { get; set; } = string.Empty;
}

public class ReprocessingSimulationResultDto
{
    public decimal InputMeatKg { get; set; }
    public decimal InputMeatCostPerKg { get; set; }
    public decimal TotalInputCost { get; set; }
    public decimal AuxiliaryIngredientsCost { get; set; }
    public decimal TotalBatchCost { get; set; }
    public decimal ExpectedYieldPercentage { get; set; }
    public decimal OutputProductKg { get; set; }
    public decimal NewUnitCostPerKg { get; set; }
    public int ShelfLifeExtensionDays { get; set; }
    public DateTime NewCalculatedExpiryDate { get; set; }
    public List<ReprocessingRequiredIngredientDto> RequiredIngredients { get; set; } = new();
}

public class ExecuteReprocessingRequestDto
{
    public int StockItemId { get; set; }
    public int RecipeId { get; set; }
    public decimal RawMeatUsedKg { get; set; }
    public string OperatorName { get; set; } = "Kasap / Şarküteri Şefi";
    public string CustomNotes { get; set; } = string.Empty;
    public DateTime? OverrideNewExpiryDate { get; set; }
}

public class ReprocessingExecutionResultDto
{
    public bool Success { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string GeneratedLotNumber { get; set; } = string.Empty;
    public string GeneratedBarcode { get; set; } = string.Empty;
    public string OutputProductName { get; set; } = string.Empty;
    public decimal OutputWeightKg { get; set; }
    public decimal NewUnitCost { get; set; }
    public DateTime NewExpiryDate { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class MeatProcessingEquipmentDto
{
    public int Id { get; set; }
    public string EquipmentCode { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public string EquipmentType { get; set; } = string.Empty;
    public string CapacityInfo { get; set; } = string.Empty;
    public string OperatingStatus { get; set; } = string.Empty;
    public DateTime LastSanitizationDate { get; set; }
    public string SanitizedBy { get; set; } = string.Empty;
    public string SanitizingAgent { get; set; } = string.Empty;
    public bool IsSanitizedAndReady { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public string? Notes { get; set; }
}

public class AuxiliaryMaterialStockDto
{
    public int Id { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public RecipeItemType ItemType { get; set; }
    public string ItemTypeDisplayName { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
    public string UnitOfMeasure { get; set; } = "KG";
    public decimal UnitCost { get; set; }
    public decimal MinStockLevel { get; set; }
    public decimal TotalValue { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string StorageCondition { get; set; } = string.Empty;
    public bool IsLowStock { get; set; }
}

public class ReprocessingQualityCheckDto
{
    public int Id { get; set; }
    public string InspectionNumber { get; set; } = string.Empty;
    public string BatchLotNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal MeasuredPh { get; set; }
    public decimal MeasuredWaterActivityAw { get; set; }
    public decimal MeasuredCoreTempCelsius { get; set; }
    public string QualityVerdict { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public bool IsAutoBlocked { get; set; }
    public string InspectorName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime InspectionDate { get; set; }
}

public class RecordQualityCheckRequestDto
{
    public string BatchLotNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal MeasuredPh { get; set; } = 5.60m;
    public decimal MeasuredWaterActivityAw { get; set; } = 0.88m;
    public decimal MeasuredCoreTempCelsius { get; set; } = 2.4m;
    public string InspectorName { get; set; } = "Kasap Kalite Sorumlusu";
    public string? Notes { get; set; }
}

public class FinancialRoiSummaryDto
{
    public decimal TotalSalvagedMeatKg { get; set; }
    public decimal TotalSalvagedValueLira { get; set; }
    public decimal TotalReprocessedOutputValueLira { get; set; }
    public decimal TotalNetProfitAddedLira { get; set; }
    public int ReprocessedBatchesCount { get; set; }
    public int CriticalBatchesSavedCount { get; set; }
    public decimal WastePreventionEfficiencyPercent { get; set; } = 96.5m;
}

public class LegalThermalLabelDto
{
    public string ProductName { get; set; } = string.Empty;
    public decimal NetWeightKg { get; set; }
    public decimal UnitPriceLira { get; set; }
    public decimal TotalPriceLira => Math.Round(NetWeightKg * UnitPriceLira, 2);
    public string LotNumber { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string PluCode { get; set; } = string.Empty;
    public DateTime PackagingDate { get; set; } = DateTime.UtcNow;
    public DateTime ExpiryDate { get; set; }
    public string IngredientsList { get; set; } = string.Empty;
    public string AllergenWarning { get; set; } = string.Empty;
    public decimal EnergyKcal { get; set; } = 285.0m;
    public decimal ProteinG { get; set; } = 18.5m;
    public decimal FatG { get; set; } = 22.0m;
    public decimal SaturatedFatG { get; set; } = 9.5m;
    public decimal SaltG { get; set; } = 2.1m;
    public string StorageCondition { get; set; } = "+0°C ile +4°C arasında muhafaza ediniz.";
    public string BusinessApprovalNo { get; set; } = "TR-34-K-009841";
    public string ManufacturerName { get; set; } = "Roy Kasap Entegre Et Tesisleri A.Ş.";
    public string OriginCountry { get; set; } = "Türkiye / Menşe: Yerli Besi";
}

public class SmartMarkdownRequestDto
{
    public int StockItemId { get; set; }
    public decimal DiscountPercentage { get; set; } = 20.0m; // %20 İndirim
    public string MarkdownReason { get; set; } = "Reyon SKT Yaklaşması (Son 24 Saat)";
    public string OperatorName { get; set; } = "Reyon Şefi";
}

public class SmartMarkdownResultDto
{
    public bool Success { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal OriginalPriceLira { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal NewDiscountedPriceLira { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class SucukCuringBatchDto
{
    public int Id { get; set; }
    public string CuringLotNumber { get; set; } = string.Empty;
    public string RecipeName { get; set; } = string.Empty;
    public string ChamberRoomCode { get; set; } = string.Empty;
    public decimal InitialGreenWeightKg { get; set; }
    public decimal CurrentWeightKg { get; set; }
    public decimal TargetDryWeightKg { get; set; }
    public decimal CurrentMoistureLossPercentage { get; set; }
    public decimal InitialPh { get; set; }
    public decimal CurrentPh { get; set; }
    public decimal ChamberTemperatureCelsius { get; set; }
    public decimal ChamberHumidityRh { get; set; }
    public int ElapsedDays { get; set; }
    public int TargetCuringDays { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsReadyForSale { get; set; }
    public string ResponsibleButcher { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string ChamberSummary => $"{ChamberTemperatureCelsius:N1}°C / %{ChamberHumidityRh:N0}";
}

public class RecordCuringMeasurementRequestDto
{
    public int BatchId { get; set; }
    public decimal MeasuredWeightKg { get; set; }
    public decimal MeasuredPh { get; set; }
    public decimal ChamberTemp { get; set; } = 15.0m;
    public decimal ChamberHumidity { get; set; } = 78.0m;
    public string ButcherNotes { get; set; } = string.Empty;
}

public class OrganolepticSensoryCheckDto
{
    public int Id { get; set; }
    public string BatchLotNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int SmellScore { get; set; } = 5; // 1-5
    public int TextureElasticityScore { get; set; } = 5; // 1-5
    public int ColorAppearanceScore { get; set; } = 5; // 1-5
    public decimal AverageScore => Math.Round((SmellScore + TextureElasticityScore + ColorAppearanceScore) / 3.0m, 1);
    public bool IsPassed => SmellScore >= 3 && TextureElasticityScore >= 3 && ColorAppearanceScore >= 3;
    public string Verdict { get; set; } = "Onaylandı (Duyusal Muayene Geçerli)";
    public string InspectorName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime InspectionDate { get; set; } = DateTime.UtcNow;
}

public class RecordSensoryCheckRequestDto
{
    public string BatchLotNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int SmellScore { get; set; } = 5;
    public int TextureElasticityScore { get; set; } = 5;
    public int ColorAppearanceScore { get; set; } = 5;
    public string InspectorName { get; set; } = "Usta Kasap";
    public string Notes { get; set; } = string.Empty;
}

public class ReprocessingDisposalRecordDto
{
    public int Id { get; set; }
    public string ProtocolNumber { get; set; } = string.Empty;
    public string BatchLotNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal DisposedQuantityKg { get; set; }
    public decimal EstimatedFinancialLossLira { get; set; }
    public string DisposalReason { get; set; } = string.Empty;
    public decimal MeasuredPh { get; set; }
    public string SensoryFailureNotes { get; set; } = string.Empty;
    public string FirstApprover { get; set; } = string.Empty;
    public string SecondApprover { get; set; } = string.Empty;
    public DateTime ApprovalDate { get; set; }
    public string RenderingCompanyName { get; set; } = string.Empty;
    public string WaybillNumber { get; set; } = string.Empty;
    public string DisposalStatus { get; set; } = string.Empty;
}

public class CreateDisposalRecordRequestDto
{
    public string BatchLotNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal DisposedQuantityKg { get; set; }
    public decimal EstimatedLossLira { get; set; }
    public decimal MeasuredPh { get; set; } = 6.45m;
    public string Reason { get; set; } = "Kritik pH > 6.20 ve Kokuşma (İmha)";
    public string FirstApprover { get; set; } = "Mustafa Usta (Reyon Şefi)";
    public string SecondApprover { get; set; } = "Dr. Vet. Mehmet Demir (HACCP Sorumlusu)";
    public string RenderingCompany { get; set; } = "Eko-Bertaraf Lisanslı Rendering A.Ş.";
    public string WaybillNumber { get; set; } = "IRS-2026-001";
}

public class ButcherWasteIncentiveDto
{
    public int Id { get; set; }
    public string ButcherName { get; set; } = string.Empty;
    public string RoleTitle { get; set; } = string.Empty;
    public string PeriodMonth { get; set; } = string.Empty;
    public decimal TotalRescuedMeatKg { get; set; }
    public decimal TotalNetValueCreatedLira { get; set; }
    public decimal IncentiveRatePercentage { get; set; }
    public decimal EarnedBonusLira { get; set; }
    public int TotalBatchesReprocessed { get; set; }
    public string BadgeTitle { get; set; } = string.Empty;
    public decimal ZeroWasteScore => Math.Round(88.0m + Math.Min(11.5m, TotalRescuedMeatKg / 25.0m), 1);
}

public class FastScanBatchResultDto
{
    public bool Found { get; set; }
    public string BarcodeOrLot { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal CurrentStockKg { get; set; }
    public double DaysRemaining { get; set; }
    public string UrgencyCode { get; set; } = "NORMAL";
    public string RecommendedAction { get; set; } = string.Empty;
}
