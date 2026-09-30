namespace KasapOtomasyon.Domain.Entities;

public enum HaccpHazardType
{
    Biological = 1,  // Mikrobiyolojik (E. coli, Salmonella vb.)
    Chemical = 2,    // Kimyasal / İlaç kalıntısı
    Physical = 3,    // Fiziksel (Metal, Kemik kıymığı, Yabancı madde)
    Temperature = 4  // Soğuk zincir sıcaklık ihlali
}

public class HaccpControlPoint : BaseEntity, IPlantScoped
{
    public int TenantId { get; set; } = 1;
    public int CompanyId { get; set; } = 1;
    public int PlantId { get; set; } = 1;

    public string CcpCode { get; set; } = "CCP-1"; // CCP-1, CCP-2, CCP-3
    public string Name { get; set; } = "Karkas 24 Saat İç Sıcaklığı & pH Kontrolü";
    public string ProcessStep { get; set; } = "Soğutma ve Dinlendirme";
    public HaccpHazardType HazardType { get; set; } = HaccpHazardType.Temperature;
    public string HazardDescription { get; set; } = "Yetersiz soğutmada patojen mikroorganizma üremesi";
    
    public decimal CriticalMinLimit { get; set; } = 0.0m;
    public decimal CriticalMaxLimit { get; set; } = 4.0m; // Max 4°C
    public string UnitOfLimit { get; set; } = "°C";
    
    public int MonitoringFrequencyMinutes { get; set; } = 60; // 60 dakikada bir ölçüm
    public string CorrectiveActionProcedure { get; set; } = "Ürünü derhal Karantina/Hold durumuna al, şoklama odasına sevk et.";

    public virtual ICollection<HaccpInspectionRecord> Inspections { get; set; } = new List<HaccpInspectionRecord>();
}

public class HaccpInspectionRecord : BaseEntity, IPlantScoped
{
    public int TenantId { get; set; } = 1;
    public int CompanyId { get; set; } = 1;
    public int PlantId { get; set; } = 1;

    public int HaccpControlPointId { get; set; }
    public virtual HaccpControlPoint HaccpControlPoint { get; set; } = null!;

    public string TargetIdentifier { get; set; } = string.Empty; // KRK-2026-000152 veya SH-01
    public decimal MeasuredValue { get; set; }
    public bool IsViolation { get; set; } = false;
    public bool AutoHoldTriggered { get; set; } = false;
    public string InspectorName { get; set; } = "Dr. Vet. Mehmet Demir";
    public string? CorrectiveActionNotes { get; set; }
    public DateTime InspectionTimestamp { get; set; } = DateTime.UtcNow;
}

public enum RecallRiskLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    CriticalClass1 = 4 // İnsan sağlığı için acil hayati tehlike (Geri Çağırma Sınıf 1)
}

public enum RecallStatus
{
    Investigation = 1,
    ActiveQuarantine = 2, // Satış ve Sevkiyat Otomatik Bloke Edildi
    PublicNoticeIssued = 3,
    RecoveryInProgress = 4,
    ResolvedAndClosed = 5
}

public class FoodRecallCase : BaseEntity, ICompanyScoped
{
    public int TenantId { get; set; } = 1;
    public int CompanyId { get; set; } = 1;

    public string CaseNumber { get; set; } = string.Empty; // RCL-2026-0001
    public string Title { get; set; } = string.Empty;
    public string TriggerReason { get; set; } = string.Empty; // Örn: Çiftlik antibiyotik arınma süresi ihlali veya mikrobiyolojik bulgu
    public RecallRiskLevel RiskLevel { get; set; } = RecallRiskLevel.CriticalClass1;
    public RecallStatus Status { get; set; } = RecallStatus.ActiveQuarantine;

    public string RootCauseEarTag { get; set; } = string.Empty;
    public string RootCauseCarcassNumber { get; set; } = string.Empty;
    public string RootCauseLotNumber { get; set; } = string.Empty;

    public int AffectedLotCount { get; set; } = 0;
    public int AffectedCustomerCount { get; set; } = 0;
    public decimal TotalRecalledWeightKg { get; set; } = 0m;
    public decimal EstimatedFinancialLoss { get; set; } = 0m;

    public string InitiatedByUserName { get; set; } = "Kalite Müdürü";
    public DateTime InitiatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedDate { get; set; }
    public string? ClosureResolution { get; set; }

    public virtual ICollection<FoodRecallItem> AffectedItems { get; set; } = new List<FoodRecallItem>();
}

public enum RecallItemState
{
    QuarantinedInWarehouse = 1, // Depoda Bloke Edildi
    BlockedInShipment = 2,       // Sevkiyatta Durduruldu
    CustomerNotified = 3,        // Müşteriye Bildirildi
    ReturnedToPlant = 4,         // İade Tesise Ulaştı
    CondemnedAndDestroyed = 5    // Resmi İmha Edildi
}

public class FoodRecallItem : BaseEntity, ICompanyScoped
{
    public int TenantId { get; set; } = 1;
    public int CompanyId { get; set; } = 1;

    public int FoodRecallCaseId { get; set; }
    public virtual FoodRecallCase FoodRecallCase { get; set; } = null!;

    public string LotNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal QuantityKg { get; set; }
    public string WarehouseLocation { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? InvoiceNumber { get; set; }
    public RecallItemState ItemState { get; set; } = RecallItemState.QuarantinedInWarehouse;
    public string? ResolutionNotes { get; set; }
}
