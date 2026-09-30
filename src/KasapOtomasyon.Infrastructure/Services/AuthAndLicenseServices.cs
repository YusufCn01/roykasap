using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.Application.Interfaces;
using KasapOtomasyon.Domain.Entities;
using KasapOtomasyon.Infrastructure.Data;
using KasapOtomasyon.Infrastructure.Licensing;
using Microsoft.EntityFrameworkCore;

namespace KasapOtomasyon.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly KasapDbContext _context;

    public AuthService(KasapDbContext context)
    {
        _context = context;
    }

    public async Task<UserDto?> LoginAsync(string username, string password)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.Trim().ToLower() && u.IsActive);

        if (user == null)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            RoleName = user.Role.Name,
            RoleType = user.Role.RoleType,
            Permissions = user.Role.Permissions.Select(p => p.PermissionKey).ToList()
        };
    }

    public async Task<UserDto?> LoginWithPinAsync(string pinCode)
    {
        var users = await _context.Users
            .Include(u => u.Role)
            .ThenInclude(r => r.Permissions)
            .Where(u => u.IsActive && !string.IsNullOrEmpty(u.PinCodeHash))
            .ToListAsync();

        foreach (var user in users)
        {
            if (BCrypt.Net.BCrypt.Verify(pinCode, user.PinCodeHash))
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    FullName = user.FullName,
                    RoleName = user.Role.Name,
                    RoleType = user.Role.RoleType,
                    Permissions = user.Role.Permissions.Select(p => p.PermissionKey).ToList()
                };
            }
        }

        return null;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || !BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
            return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        return await _context.Users
            .Include(u => u.Role)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                FullName = u.FullName,
                RoleName = u.Role.Name,
                RoleType = u.Role.RoleType
            })
            .ToListAsync();
    }

    public async Task<bool> SaveUserAsync(User user, string? password, string? pin)
    {
        if (!string.IsNullOrEmpty(password))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

        if (!string.IsNullOrEmpty(pin))
            user.PinCodeHash = BCrypt.Net.BCrypt.HashPassword(pin);

        if (user.Id == 0)
        {
            await _context.Users.AddAsync(user);
        }
        else
        {
            _context.Users.Update(user);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Role>> GetAllRolesAsync()
    {
        return await _context.Roles.Include(r => r.Permissions).ToListAsync();
    }

    public async Task<bool> HasPermissionAsync(int userId, string permissionKey)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return false;
        if (user.Role.RoleType == Domain.Enums.UserRoleType.Yonetici) return true;

        return user.Role.Permissions.Any(p => p.PermissionKey == permissionKey && p.CanView);
    }
}

public class LicenseService : ILicenseService
{
    private readonly KasapDbContext _context;
    private readonly HardwareFingerprintProvider _fingerprintProvider;
    private readonly RsaLicenseValidator _validator;

    public LicenseService(KasapDbContext context, HardwareFingerprintProvider fingerprintProvider, RsaLicenseValidator validator)
    {
        _context = context;
        _fingerprintProvider = fingerprintProvider;
        _validator = validator;
    }

    public Task<MachineFingerprintDto> GetMachineFingerprintAsync()
    {
        return Task.FromResult(_fingerprintProvider.GetMachineFingerprint());
    }

    public async Task<LicenseValidationResultDto> ValidateLicenseAsync()
    {
        return await _validator.ValidateCurrentLicenseAsync();
    }

    public async Task<bool> ActivateLicenseFileAsync(string licenseFilePathOrContent)
    {
        string content = licenseFilePathOrContent;
        if (File.Exists(licenseFilePathOrContent))
        {
            content = await File.ReadAllTextAsync(licenseFilePathOrContent);
        }

        return await _validator.ActivateLicenseAsync(content);
    }

    public async Task<bool> ActivateTrialAsync()
    {
        var fp = _fingerprintProvider.GetMachineFingerprint();
        var result = await _validator.CreateAndActivateTrialLicenseAsync(fp.Fingerprint);
        return result.IsValid;
    }

    public async Task<LicenseRecord?> GetCurrentLicenseRecordAsync()
    {
        return await _context.LicenseRecords
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public string GetPublicKeyXml()
    {
        return RsaKeyConstants.PublicKeyXml;
    }
}
