using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.Domain.Enums;

namespace KasapOtomasyon.Application.Interfaces;

public class AnimalPassportDocumentDto
{
    // Document Meta
    public string DocumentNumber { get; set; } = string.Empty; // PSP-TR340019283-2026
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
    public string OrganizationName { get; set; } = "ROYPOS KASAP & MEZBAHA İŞLETMESİ";
    public string OfficialFacilityCode { get; set; } = "TR-34-MEZ-0089";
    public string QrVerificationUrl { get; set; } = string.Empty;
    public string DocumentSecurityHash { get; set; } = string.Empty;

    // 1. Hayvan Kimlik ve Kabul (Pedigree & Intake)
    public string EarTagNumber { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
    public string WaybillNumber { get; set; } = string.Empty;
    public string RfidTag { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public AnimalType AnimalType { get; set; } = AnimalType.Buyukbas;
    public string Breed { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public int AgeMonths { get; set; }
    public decimal LiveWeightKg { get; set; }
    public decimal PurchasePrice { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public string FarmOrigin { get; set; } = string.Empty;
    public DateTime ArrivalDate { get; set; }

    // 2. Veteriner Antemortem Muayene Raporu
    public bool HasVetCheck { get; set; }
    public string VetDoctorName { get; set; } = string.Empty;
    public string VetDiplomaNumber { get; set; } = string.Empty;
    public string VetReportNumber { get; set; } = string.Empty;
    public DateTime VetCheckDate { get; set; }
    public decimal VetBodyTemperature { get; set; }
    public string VetDiagnosis { get; set; } = string.Empty;
    public string VetStatus { get; set; } = string.Empty;
    public bool IsApprovedForSlaughter { get; set; }

    // 3. Kesimhane, Karkas Tartım & SEUROP Kalite Sınıflandırması
    public bool HasSlaughterRecord { get; set; }
    public string SlaughterNumber { get; set; } = string.Empty;
    public string CarcassNumber { get; set; } = string.Empty;
    public DateTime SlaughterDate { get; set; }
    public string ButcherName { get; set; } = string.Empty;
    public string SlaughterLine { get; set; } = string.Empty;
    public decimal HotCarcassWeightKg { get; set; }
    public decimal ColdCarcassWeightKg { get; set; }
    public decimal CarcassYieldPercentage { get; set; }
    public decimal HeadWeightKg { get; set; }
    public decimal HideWeightKg { get; set; }
    public decimal OffalWeightKg { get; set; }
    public decimal BoneWeightKg { get; set; }
    public decimal FatWeightKg { get; set; }
    public decimal SlaughterWasteKg { get; set; }
    
    // SEUROP & Meat Quality
    public string ConformationClass { get; set; } = "R";
    public int FatCoverScore { get; set; } = 3;
    public int MarblingScore { get; set; } = 5;
    public decimal PostMortemPh24 { get; set; } = 5.65m;
    public string ColdStorageLocation { get; set; } = string.Empty;
    public string HookRailNumber { get; set; } = string.Empty;

    // Costing
    public decimal TotalSlaughterCost { get; set; }
    public decimal CarcassCostPerKg { get; set; }

    // 4. Karkas Parçalama & Anatomik Et Çıktıları
    public List<CarcassDeboningCutDto> DeboningCuts { get; set; } = new();
    public decimal TotalDebonedKg => DeboningCuts.Sum(c => c.WeightKg);
    public decimal TotalDebonedValue => DeboningCuts.Sum(c => c.TotalCutValue);
    public decimal DeboningEfficiencyYield => ColdCarcassWeightKg > 0 ? Math.Round((TotalDebonedKg / ColdCarcassWeightKg) * 100m, 1) : 0;
}

public interface IAnimalPassportPdfService
{
    Task<AnimalPassportDocumentDto> GetAnimalPassportDataAsync(string earTagOrCarcassNo);
    string GeneratePassportHtml(AnimalPassportDocumentDto data);
    Task<string> ExportAndOpenPassportPdfAsync(string earTagOrCarcassNo, string? targetFolder = null);
}
