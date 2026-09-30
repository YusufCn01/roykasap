namespace KasapOtomasyon.Domain.Entities;

/// <summary>
/// Sucuk ve fermente şarküteri kurutma / fermantasyon odası parti takip varlığı.
/// </summary>
public class SucukCuringBatch : BaseEntity, IPlantScoped
{
    public int TenantId { get; set; } = 1;
    public int CompanyId { get; set; } = 1;
    public int PlantId { get; set; } = 1;

    public string CuringLotNumber { get; set; } = string.Empty; // CUR-2026-SCK-01
    public string RecipeName { get; set; } = "Geleneksel Fermente Kasap Sucuğu";
    public string ChamberRoomCode { get; set; } = "ODA-FERM-01"; // Olgunlaşma Odası 1
    
    public decimal InitialGreenWeightKg { get; set; } // Yaş Dolum Ağırlığı (örn. 100 kg)
    public decimal CurrentWeightKg { get; set; }      // Güncel Ağırlık (örn. 91.5 kg)
    public decimal TargetDryWeightKg { get; set; }    // Hedef Ağırlık (örn. 88 kg - %12 kuruma firesi)
    
    public decimal CurrentMoistureLossPercentage => InitialGreenWeightKg > 0 
        ? Math.Round(((InitialGreenWeightKg - CurrentWeightKg) / InitialGreenWeightKg) * 100m, 1) 
        : 0;

    public decimal InitialPh { get; set; } = 5.65m;  // Dolum anındaki et pH'ı
    public decimal CurrentPh { get; set; } = 5.15m;  // Fermantasyon asitlik pH'ı (hedef 4.90 - 5.20)
    
    public decimal ChamberTemperatureCelsius { get; set; } = 15.2m; // İdeal +14°C - +16°C
    public decimal ChamberHumidityRh { get; set; } = 78.5m;          // İdeal %75 - %85 RH
    
    public int ElapsedDays { get; set; } = 4;
    public int TargetCuringDays { get; set; } = 10;
    
    private string _status = "Olgunlaşıyor (Fermantasyonda)";
    public string Status 
    { 
        get => IsReadyForSale ? "Tamamlandı (Satışa Uygun)" : _status; 
        set => _status = value; 
    }
    public bool IsReadyForSale => CurrentWeightKg <= TargetDryWeightKg && CurrentPh <= 5.30m;
    public string ResponsibleButcher { get; set; } = "Mustafa Usta";
    public string? Notes { get; set; }
}

/// <summary>
/// Gıda güvenliği gereği pH > 6.20 veya organoleptik testten kalan etler için resmi imha & bertaraf tutanağı.
/// </summary>
public class ReprocessingDisposalRecord : BaseEntity, IPlantScoped
{
    public int TenantId { get; set; } = 1;
    public int CompanyId { get; set; } = 1;
    public int PlantId { get; set; } = 1;

    public string ProtocolNumber { get; set; } = string.Empty; // IMHA-2026-0913-001
    public string BatchLotNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal DisposedQuantityKg { get; set; }
    public decimal EstimatedFinancialLossLira { get; set; }
    
    public string DisposalReason { get; set; } = "Kritik pH > 6.20 Limiti Aşıldı (Kokuşma / Bozulma Tespiti)";
    public decimal MeasuredPh { get; set; }
    public string SensoryFailureNotes { get; set; } = string.Empty;
    
    // Çift onay kuralı (Fraud önleme)
    public string FirstApprover { get; set; } = "Mustafa Usta (Reyon / Kasap Şefi)";
    public string SecondApprover { get; set; } = "Dr. Vet. Mehmet Demir (HACCP Sorumlusu)";
    public DateTime ApprovalDate { get; set; } = DateTime.UtcNow;
    
    // Lisanslı bertaraf firması bilgisi
    public string RenderingCompanyName { get; set; } = "Biyo-Atık & Rendering A.Ş. (Bakanlık Lisanslı)";
    public string WaybillNumber { get; set; } = "IRS-2026-98112";
    public string DisposalStatus { get; set; } = "İmha Edildi & Belgelendi";
}

/// <summary>
/// Kasap ve reyon personeli fire önleme prim ve teşvik puanlama varlığı.
/// </summary>
public class ButcherWasteIncentive : BaseEntity, IPlantScoped
{
    public int TenantId { get; set; } = 1;
    public int CompanyId { get; set; } = 1;
    public int PlantId { get; set; } = 1;

    public string ButcherName { get; set; } = string.Empty; // Mustafa Usta
    public string RoleTitle { get; set; } = "Usta Kasap / Şarküteri Şefi";
    public string PeriodMonth { get; set; } = "Eylül 2026";
    
    public decimal TotalRescuedMeatKg { get; set; }   // Kurtardığı et kg
    public decimal TotalNetValueCreatedLira { get; set; } // İşletmeye kazandırdığı katma değer TL
    public decimal IncentiveRatePercentage { get; set; } = 2.0m; // %2 prim oranı
    public decimal EarnedBonusLira => Math.Round(TotalNetValueCreatedLira * (IncentiveRatePercentage / 100m), 2);
    
    public int TotalBatchesReprocessed { get; set; }
    public string BadgeTitle { get; set; } = "🥇 Altın Kasap (Sıfır Fire Şampiyonu)";
}
