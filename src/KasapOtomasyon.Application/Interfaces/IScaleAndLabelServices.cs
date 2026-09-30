namespace KasapOtomasyon.Application.Interfaces;

public class ScaleReadingResult
{
    public decimal WeightKg { get; set; }
    public decimal TareKg { get; set; }
    public decimal NetWeightKg { get; set; }
    public bool IsStable { get; set; }
    public string Unit { get; set; } = "kg";
    public string ScalePortOrIp { get; set; } = "COM1";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public interface IScaleIntegrationService
{
    Task<ScaleReadingResult> ReadLiveWeightAsync(string scalePort = "COM1", decimal fallbackTarget = 0);
    Task<bool> ZeroScaleAsync(string scalePort = "COM1");
    Task<bool> TareScaleAsync(string scalePort = "COM1", decimal tareKg = 0);
}

public class Gs1MeatLabelData
{
    public string GtinCode { get; set; } = "08680000100152";
    public string LotNumber { get; set; } = "PRC-2026-001";
    public decimal NetWeightKg { get; set; }
    public DateTime SlaughterDate { get; set; }
    public DateTime PackagingDate { get; set; } = DateTime.UtcNow;
    public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddDays(14);
    public string EarTagNumber { get; set; } = string.Empty;
    public string CutName { get; set; } = string.Empty;
    public string QualityGrade { get; set; } = "1. Sınıf Lüks";
    public string ConformationClass { get; set; } = "R";
    public string CountryOfOrigin { get; set; } = "TR";
    public string SlaughterhouseCode { get; set; } = "TR-34-MEZ-089";
    public string StorageTemperature { get; set; } = "0°C - +4°C";
}

public interface IGs1LabelService
{
    string GenerateGs1128BarcodeString(Gs1MeatLabelData label);
    string GenerateQrTraceabilityUrl(string earTag, string carcassNumber, string lotNumber);
    string GenerateZplPrintCommand(Gs1MeatLabelData label);
}
