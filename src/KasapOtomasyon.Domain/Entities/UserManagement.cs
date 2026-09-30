using KasapOtomasyon.Domain.Enums;

namespace KasapOtomasyon.Domain.Entities;

public class User : BaseEntity, IPlantScoped
{
    public int TenantId { get; set; } = 1;
    public int CompanyId { get; set; } = 1;
    public int PlantId { get; set; } = 1;

    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PinCodeHash { get; set; } // 4-digit quick cashier login PIN
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public int RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;
    public string PreferredLanguage { get; set; } = "tr-TR"; // tr-TR, en-GB, pl-PL
    public DateTime? LastLoginAt { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public virtual ICollection<UserPreference> Preferences { get; set; } = new List<UserPreference>();
}

public class Role : BaseEntity, ITenantScoped
{
    public int TenantId { get; set; } = 1;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public UserRoleType RoleType { get; set; } = UserRoleType.PosCashier;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();
}

public class RolePermission : BaseEntity
{
    public int RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;
    public string PermissionKey { get; set; } = string.Empty; // e.g. "carcass.override_weight", "slaughter.execute", "pos.sale", "audit.investigate"
    public bool CanView { get; set; } = true;
    public bool CanCreate { get; set; } = true;
    public bool CanEdit { get; set; } = true;
    public bool CanDelete { get; set; } = true;
}

public class AuditLog : BaseEntity, IPlantScoped
{
    public int TenantId { get; set; } = 1;
    public int CompanyId { get; set; } = 1;
    public int PlantId { get; set; } = 1;

    public int? UserId { get; set; }
    public virtual User? User { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // "LOGIN", "CARCASS_WEIGHT_OVERRIDE", "SCALE_READ", "SLAUGHTER_EXECUTE", "MASS_BALANCE_CHECK"
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? Details { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? Reason { get; set; }
    public string? SupervisorApprover { get; set; }
    public string? IpAddress { get; set; }
    public string? WorkstationMachineId { get; set; }
    public string? ApplicationVersion { get; set; } = "2.0.0";
    public string Sha256Signature { get; set; } = string.Empty; // Cryptographic tamper-evidence
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class UserPreference : BaseEntity
{
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;
    public string PreferenceKey { get; set; } = string.Empty; // "Theme", "Language", "QuickScalePort", "DefaultPlantId"
    public string PreferenceValue { get; set; } = string.Empty;
}
