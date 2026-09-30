using KasapOtomasyon.Domain.Enums;

namespace KasapOtomasyon.Domain.Entities;

public class LicenseRecord : BaseEntity
{
    public string LicenseKey { get; set; } = string.Empty;
    public string SignedPayloadJson { get; set; } = string.Empty;
    public string MachineFingerprint { get; set; } = string.Empty;
    public LicenseType LicenseType { get; set; } = LicenseType.Deneme;
    public string CompanyName { get; set; } = string.Empty;
    public string LicensedTo { get; set; } = string.Empty;
    public DateTime ValidUntil { get; set; }
    public int MaxTerminals { get; set; } = 1;
    public int ActiveTerminalsCount { get; set; } = 1;
    public string EnabledFeaturesJson { get; set; } = "[]";
    public DateTime? LastHeartbeat { get; set; }
}

public class AppSetting : BaseEntity
{
    public string Key { get; set; } = string.Empty; // e.g. "Scale.Port", "Scale.BaudRate", "Sms.Provider", "EInvoice.Integrator", "Backup.Path"
    public string Value { get; set; } = string.Empty;
    public string Category { get; set; } = "General"; // "General", "Scale", "Sms", "EInvoice", "Backup", "UI"
    public string? Description { get; set; }
}
