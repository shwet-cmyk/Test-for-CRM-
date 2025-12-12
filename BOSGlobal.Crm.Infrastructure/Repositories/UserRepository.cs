using BOSGlobal.Crm.Application.Interfaces;
using BOSGlobal.Crm.Domain.Entities;
using BOSGlobal.Crm.Domain.Enums;
using BOSGlobal.Crm.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace BOSGlobal.Crm.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<AppUser?> FindByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user is null ? null : MapToDomain(user);
    }

    public async Task<bool> CheckPasswordAsync(AppUser user, string password)
    {
        var identityUser = await _userManager.FindByIdAsync(user.Id);
        if (identityUser is null)
        {
            return false;
        }

        return await _userManager.CheckPasswordAsync(identityUser, password);
    }

    public async Task<IReadOnlyList<UserRole>> GetRolesAsync(AppUser user)
    {
        var identityUser = await _userManager.FindByIdAsync(user.Id);
        if (identityUser is null)
        {
            return Array.Empty<UserRole>();
        }

        var roles = await _userManager.GetRolesAsync(identityUser);
        return roles.Select(r => Enum.TryParse<UserRole>(r, out var parsed) ? parsed : UserRole.Support).ToList();
    }

    private static AppUser MapToDomain(ApplicationUser identityUser)
    {
        return new AppUser
        {
            Id = identityUser.Id,
            Email = identityUser.Email ?? string.Empty,
            IsActive = identityUser.IsActive,
            ErpId = identityUser.ErpId,
            GstNumber = identityUser.GstNumber,
            PhoneNumber = identityUser.PhoneNumber,
            PhoneNumberConfirmed = identityUser.PhoneNumberConfirmed
        };
    }
}
