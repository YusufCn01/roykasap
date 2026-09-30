using KasapOtomasyon.Domain.Enums;

namespace KasapOtomasyon.Domain.Entities;

public enum CarcassPortionType
{
    Whole = 1,        // Bütün Karkas
    LeftHalf = 2,     // Sol Yarım (A)
    RightHalf = 3,    // Sağ Yarım (B)
    ForeQuarterLeft = 4,   // Sol Ön Çeyrek
    HindQuarterLeft = 5,   // Sol Arka Çeyrek
    ForeQuarterRight = 6,  // Sağ Ön Çeyrek
    HindQuarterRight = 7   // Sağ Arka Çeyrek
}

public class CarcassPortion : BaseEntity, IPlantScoped
{
    public int TenantId { get; set; } = 1;
    public int CompanyId { get; set; } = 1;
    public int PlantId { get; set; } = 1;

    public int SlaughterRecordId { get; set; }
    public virtual SlaughterRecord SlaughterRecord { get; set; } = null!;

    public string PortionCode { get; set; } = string.Empty; // CAR-TR-2026-000001-A (Sol Yarım)
    public CarcassPortionType PortionType { get; set; } = CarcassPortionType.LeftHalf;
    public decimal WeightKg { get; set; }
    public string HookRailLocation { get; set; } = "Ray-1 / Kanca-12";
    public bool IsDeboned { get; set; } = false;
    public string Gs1Barcode { get; set; } = string.Empty;
}

public class ChillingRecord : BaseEntity, IPlantScoped
{
    public int TenantId { get; set; } = 1;
    public int CompanyId { get; set; } = 1;
    public int PlantId { get; set; } = 1;

    public int SlaughterRecordId { get; set; }
    public virtual SlaughterRecord SlaughterRecord { get; set; } = null!;

    public string ColdRoomCode { get; set; } = "SH-01";
    public string RailAndHookNumber { get; set; } = "Ray-2 / Askı-08";
    public DateTime EntryTime { get; set; } = DateTime.UtcNow;
    public DateTime? ExitTime { get; set; }
    public decimal EntryHotWeightKg { get; set; }
    public decimal ExitColdWeightKg { get; set; }
    public decimal ShrinkageWeightKg => EntryHotWeightKg > ExitColdWeightKg && ExitColdWeightKg > 0 ? EntryHotWeightKg - ExitColdWeightKg : 0m;
    public decimal ShrinkagePercentage => EntryHotWeightKg > 0 && ExitColdWeightKg > 0 ? Math.Round((ShrinkageWeightKg / EntryHotWeightKg) * 100m, 2) : 0m;
    public decimal AverageRoomTemperatureCelsius { get; set; } = 1.8m;
    public bool IsShrinkageAnomalyDetected => ShrinkagePercentage > 3.0m; // Normal karkas soğuma firesi %1.5 - %2.5 arasıdır
}

public enum CuttingOrderStatus
{
    Draft = 1,
    InProgress = 2,
    MassBalanceVerified = 3, // Kütle dengesi onaylandı
    MassBalanceVarianceWarning = 4, // Açıklanamayan fark var
    Completed = 5,
    Cancelled = 6
}

public class CuttingOrder : BaseEntity, IPlantScoped
{
    public int TenantId { get; set; } = 1;
    public int CompanyId { get; set; } = 1;
    public int PlantId { get; set; } = 1;

    public string OrderCode { get; set; } = string.Empty; // CUT-2026-000001
    public int SlaughterRecordId { get; set; }
    public virtual SlaughterRecord SlaughterRecord { get; set; } = null!;
    public string CarcassNumber { get; set; } = string.Empty;
    public string EarTagNumber { get; set; } = string.Empty;

    public string MasterButcher { get; set; } = "Usta Kasap Hüseyin";
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }

    // Mass Balance Equation Inputs and Outputs
    public decimal InputCarcassWeightKg { get; set; }        // Giriş Karkas (kg)
    public decimal TotalPrimalCutsWeightKg { get; set; }     // Çıkan Asil Etler (kg)
    public decimal TotalByproductsWeightKg { get; set; }     // Sakatat / Yan Ürün (kg)
    public decimal TotalBoneAndFatWeightKg { get; set; }     // Kemik ve Yağ (kg)
    public decimal TotalWasteWeightKg { get; set; }           // Fire / Zayiat (kg)
    public decimal ProcessLossWeightKg { get; set; }          // Doğal Nem / İşlem Kaybı (kg)

    // Calculated Mass Balance Difference
    public decimal TotalOutputWeightKg => TotalPrimalCutsWeightKg + TotalByproductsWeightKg + TotalBoneAndFatWeightKg + TotalWasteWeightKg + ProcessLossWeightKg;
    public decimal MassBalanceDifferenceKg => Math.Abs(InputCarcassWeightKg - TotalOutputWeightKg);
    public decimal MassBalanceVariancePercentage => InputCarcassWeightKg > 0 ? Math.Round((MassBalanceDifferenceKg / InputCarcassWeightKg) * 100m, 2) : 0m;
    
    public bool IsMassBalanceApproved => MassBalanceVariancePercentage <= 0.5m; // Max allowable variance is 0.5%
    public CuttingOrderStatus Status { get; set; } = CuttingOrderStatus.Draft;
    public string? VarianceExplanation { get; set; }
    public string? SupervisorApprovedBy { get; set; }

    public virtual ICollection<CarcassDeboningCut> Cuts { get; set; } = new List<CarcassDeboningCut>();
}

public class IoTTelemetryLog : BaseEntity, IPlantScoped
{
    public int TenantId { get; set; } = 1;
    public int CompanyId { get; set; } = 1;
    public int PlantId { get; set; } = 1;

    public string DeviceCode { get; set; } = "IOT-TEMP-SH01";
    public string LocationName { get; set; } = "Soğuk Depo #1 (Karkas Dinlendirme)";
    public decimal TemperatureCelsius { get; set; }
    public decimal HumidityPercentage { get; set; }
    public bool IsDoorOpen { get; set; } = false;
    public bool IsPowerOn { get; set; } = true;
    public bool IsThresholdViolation { get; set; } = false;
    public string? ViolationMessage { get; set; }
    public DateTime TelemetryTimestamp { get; set; } = DateTime.UtcNow;
}
