using KasapOtomasyon.Domain.Enums;

namespace KasapOtomasyon.Application.DTOs;

public class AnimalIntakeDto
{
    public int Id { get; set; }
    public string EarTagNumber { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
    public AnimalType AnimalType { get; set; }
    public string Breed { get; set; } = string.Empty;
    public AnimalGender Gender { get; set; }
    public int AgeMonths { get; set; }
    public decimal LiveWeightKg { get; set; }
    public decimal PurchasePrice { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public string WaybillNumber { get; set; } = string.Empty;
    public string FarmOrigin { get; set; } = string.Empty;
    public DateTime ArrivalDate { get; set; }
    public int SlaughterOrderNumber { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public VeterinaryCheckStatus VeterinaryStatus { get; set; }
    public bool IsApprovedForSlaughter { get; set; }
    public SlaughterStatus SlaughterStatus { get; set; }
    public string? BlockReason { get; set; }
}

public class VeterinaryCheckDto
{
    public int Id { get; set; }
    public int AnimalIntakeId { get; set; }
    public string EarTagNumber { get; set; } = string.Empty;
    public string VeterinarianName { get; set; } = string.Empty;
    public string DiplomaNumber { get; set; } = string.Empty;
    public DateTime CheckDate { get; set; }
    public bool IsAntemortem { get; set; }
    public VeterinaryCheckStatus Status { get; set; }
    public bool IsApprovedForSlaughter { get; set; }
    public decimal BodyTemperature { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string? QuarantineNotes { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
}

public class SlaughterRecordDto
{
    public int Id { get; set; }
    public string SlaughterNumber { get; set; } = string.Empty;
    public string CarcassNumber { get; set; } = string.Empty;
    public int AnimalIntakeId { get; set; }
    public string EarTagNumber { get; set; } = string.Empty;
    public DateTime SlaughterDate { get; set; }
    public string ButcherPersonName { get; set; } = string.Empty;
    public string VeterinarianName { get; set; } = string.Empty;
    
    public decimal LiveWeightKg { get; set; }
    public decimal HotCarcassWeightKg { get; set; }
    public decimal ColdCarcassWeightKg { get; set; }
    public decimal HeadWeightKg { get; set; }
    public decimal HideWeightKg { get; set; }
    public decimal OffalWeightKg { get; set; }
    public decimal FatWeightKg { get; set; }
    public decimal BoneWeightKg { get; set; }
    public decimal SlaughterWasteKg { get; set; }
    
    public decimal CarcassYieldPercentage { get; set; }
    public decimal TotalSlaughterCost { get; set; }
    public decimal CarcassCostPerKg { get; set; }
    public string ColdStorageLocation { get; set; } = string.Empty;
    public SlaughterStatus Status { get; set; }
}

public class TraceabilityNodeItemDto
{
    public string StepName { get; set; } = string.Empty; // "Hayvan Kabul", "Veteriner Muayene", "Kesimhane", "Karkas Tartım", "Parçalama BOM", "Satış"
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string OperatorOrPerson { get; set; } = string.Empty;
    public string StatusBadge { get; set; } = "Onaylandı";
}

public class BidirectionalTraceabilityTreeDto
{
    public string EarTagNumber { get; set; } = string.Empty;
    public string CarcassNumber { get; set; } = string.Empty;
    public string BreedAndType { get; set; } = string.Empty;
    public string FarmAndOrigin { get; set; } = string.Empty;
    public string ProducerName { get; set; } = string.Empty;
    
    public decimal LiveWeightKg { get; set; }
    public decimal CarcassWeightKg { get; set; }
    public decimal YieldPercentage { get; set; }
    
    public List<TraceabilityNodeItemDto> LifecycleSteps { get; set; } = new();
    public List<ProductionOutputDto> DebonedCuts { get; set; } = new();
    public List<string> DistributedSaleReceipts { get; set; } = new();
}

public class ForensicAnomalyDto
{
    public int Id { get; set; }
    public string AnomalyCode { get; set; } = string.Empty;
    public AnomalyType Type { get; set; }
    public string Severity { get; set; } = "Danger";
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public decimal ExpectedQuantity { get; set; }
    public decimal ActualQuantity { get; set; }
    public decimal DifferenceQuantity { get; set; }
    public decimal FinancialLossEstimated { get; set; }
    public DateTime DetectedDate { get; set; }
    public bool IsResolved { get; set; }
}

public class SlaughterServiceInvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string ProducerName { get; set; } = string.Empty;
    public string EarTagNumber { get; set; } = string.Empty;
    public decimal TotalServiceFee { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingBalance { get; set; }
    public bool IsPaid { get; set; }
}

public class WasteLogDto
{
    public int Id { get; set; }
    public string WasteNumber { get; set; } = string.Empty;
    public WasteCauseType CauseType { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string LotOrCarcassNumber { get; set; } = string.Empty;
    public decimal WeightKg { get; set; }
    public decimal TotalCost { get; set; }
    public string ResponsiblePerson { get; set; } = string.Empty;
    public string ApprovedByPerson { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime LogDate { get; set; }
}

public class CarcassDeboningCutDto
{
    public int Id { get; set; }
    public int SlaughterRecordId { get; set; }
    public string CarcassNumber { get; set; } = string.Empty;
    public string EarTagNumber { get; set; } = string.Empty;
    public string AnatomicalRegion { get; set; } = string.Empty;
    public string CutName { get; set; } = string.Empty;
    public decimal WeightKg { get; set; }
    public decimal YieldPercentage { get; set; }
    public string QualityGrade { get; set; } = string.Empty;
    public decimal UnitCostEstimated { get; set; }
    public decimal TotalCutValue { get; set; }
    public string LotNumber { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string TargetStorageLocation { get; set; } = string.Empty;
    public DateTime CutDate { get; set; }
    public string MasterButcher { get; set; } = string.Empty;
}
