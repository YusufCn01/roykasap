using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.Domain.Entities;

namespace KasapOtomasyon.Application.Interfaces;

public interface IAuthService
{
    Task<UserDto?> LoginAsync(string username, string password);
    Task<UserDto?> LoginWithPinAsync(string pinCode);
    Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    Task<List<UserDto>> GetAllUsersAsync();
    Task<bool> SaveUserAsync(User user, string? password, string? pin);
    Task<List<Role>> GetAllRolesAsync();
    Task<bool> HasPermissionAsync(int userId, string permissionKey);
}

public interface ILicenseService
{
    Task<MachineFingerprintDto> GetMachineFingerprintAsync();
    Task<LicenseValidationResultDto> ValidateLicenseAsync();
    Task<bool> ActivateLicenseFileAsync(string licenseFilePathOrContent);
    Task<bool> ActivateTrialAsync();
    Task<LicenseRecord?> GetCurrentLicenseRecordAsync();
    string GetPublicKeyXml();
}
