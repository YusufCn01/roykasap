namespace KasapOtomasyon.Domain.Entities;

public class TrueCostRollup : BaseEntity, IPlantScoped
{
    public int TenantId { get; set; } = 1;
    public int CompanyId { get; set; } = 1;
    public int PlantId { get; set; } = 1;

    public string CarcassNumber { get; set; } = string.Empty;
    public string EarTagNumber { get; set; } = string.Empty;

    // Cost Breakdown Items (Activity-Based Costing)
    public decimal AnimalPurchaseCost { get; set; }
    public decimal LivestockTransportCost { get; set; }
    public decimal SlaughterLaborCost { get; set; }
    public decimal VeterinaryInspectionCost { get; set; }
    public decimal ColdStorageChillingEnergyCost { get; set; }
    public decimal DeboningLaborCost { get; set; }
    public decimal PackagingAndLabelingCost { get; set; }
    public decimal GeneralFacilityOverheadCost { get; set; }

    // Revenue credits from By-Products (Offsetting Cost)
    public decimal HideLeatherCreditRevenue { get; set; }   // Deri Geliri (-)
    public decimal OffalOrganCreditRevenue { get; set; }    // Sakatat Geliri (-)
    public decimal BoneAndFatCreditRevenue { get; set; }    // Sanayi Yağı / Kemik (-)

    public decimal TotalGrossCost => AnimalPurchaseCost + LivestockTransportCost + SlaughterLaborCost + VeterinaryInspectionCost + 
                                     ColdStorageChillingEnergyCost + DeboningLaborCost + PackagingAndLabelingCost + GeneralFacilityOverheadCost;

    public decimal TotalByproductCredits => HideLeatherCreditRevenue + OffalOrganCreditRevenue + BoneAndFatCreditRevenue;

    public decimal NetTrueCost => TotalGrossCost - TotalByproductCredits;

    public decimal ColdCarcassWeightKg { get; set; }
    public decimal TrueCostPerCarcassKg => ColdCarcassWeightKg > 0 ? Math.Round(NetTrueCost / ColdCarcassWeightKg, 2) : 0m;
    
    public decimal TotalDebonedMeatSalesValue { get; set; }
    public decimal GrossProfitMarginAmount => TotalDebonedMeatSalesValue - NetTrueCost;
    public decimal GrossProfitMarginPercentage => NetTrueCost > 0 ? Math.Round((GrossProfitMarginAmount / NetTrueCost) * 100m, 2) : 0m;
}

public class RetailScaleBarcodeRule : BaseEntity, ICompanyScoped
{
    public int TenantId { get; set; } = 1;
    public int CompanyId { get; set; } = 1;

    public string RuleName { get; set; } = "Standart Terazi Barkod Şablonu (27 + PLU5 + Weight5)";
    public string Prefix { get; set; } = "27"; // 20, 27, 28, 29
    public int PluLength { get; set; } = 5;
    public int ValueLength { get; set; } = 5;
    public bool IsWeightType { get; set; } = true; // true = Weight (Gr), false = Price (Kuruş)
    public int DecimalPlaces { get; set; } = 3;   // 1250 -> 1.250 kg
    public bool HasCheckDigit { get; set; } = true;
}

public enum FraudRiskLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum FraudCaseStatus
{
    OpenUnderReview = 1,
    SupervisorApproved = 2,
    ConfirmedFraud = 3,
    DismissedFalsePositive = 4
}

public class FraudInvestigationCase : BaseEntity, ICompanyScoped
{
    public int TenantId { get; set; } = 1;
    public int CompanyId { get; set; } = 1;

    public string CaseCode { get; set; } = string.Empty; // FRD-2026-0001
    public string Title { get; set; } = string.Empty;
    public FraudRiskLevel RiskLevel { get; set; } = FraudRiskLevel.High;
    public FraudCaseStatus Status { get; set; } = FraudCaseStatus.OpenUnderReview;

    public string TriggerAction { get; set; } = "MANUAL_SCALE_WEIGHT_OVERRIDE";
    public string SuspectedUserName { get; set; } = string.Empty;
    public decimal DiscrepancyQuantityKg { get; set; }
    public decimal EstimatedFinancialImpact { get; set; }
    public string EvidencePayload { get; set; } = string.Empty;

    public string? AssignedInvestigator { get; set; }
    public string? ResolutionNotes { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
