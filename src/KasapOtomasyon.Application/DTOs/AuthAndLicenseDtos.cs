using KasapOtomasyon.Domain.Enums;

namespace KasapOtomasyon.Application.DTOs;

public class LoginRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class PinLoginRequestDto
{
    public string PinCode { get; set; } = string.Empty;
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public UserRoleType RoleType { get; set; }
    public List<string> Permissions { get; set; } = new();
}

public class MachineFingerprintDto
{
    public string Fingerprint { get; set; } = string.Empty;
    public string CpuId { get; set; } = string.Empty;
    public string DiskSerial { get; set; } = string.Empty;
    public string MacAddress { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
}

public class LicensePayloadDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string LicensedTo { get; set; } = string.Empty;
    public string MachineFingerprint { get; set; } = string.Empty;
    public LicenseType LicenseType { get; set; } = LicenseType.Standart;
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public DateTime ValidUntil { get; set; }
    public int MaxTerminals { get; set; } = 1;
    public List<string> EnabledFeatures { get; set; } = new();
    public string Issuer { get; set; } = "Kasap Otomasyon Licensing Authority";
}

public class LicenseValidationResultDto
{
    public bool IsValid { get; set; }
    public LicenseStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public LicenseType LicenseType { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public DateTime ValidUntil { get; set; }
    public int RemainingDays { get; set; }
    public bool IsWarningRequired { get; set; } // 15, 7, 1 gün kala
    public string WarningBannerText { get; set; } = string.Empty;
    public List<string> EnabledFeatures { get; set; } = new();
    public bool CanRunMezbahaModule => EnabledFeatures.Contains("MEZBAHA_MODULE") || LicenseType == LicenseType.Profesyonel || LicenseType == LicenseType.Kurumsal;
    public bool CanRunEInvoice => EnabledFeatures.Contains("E_INVOICE") || LicenseType == LicenseType.Kurumsal;
    public bool CanRunSms => EnabledFeatures.Contains("SMS_REMINDER") || LicenseType == LicenseType.Kurumsal;
}
