using BOSGlobal.Crm.Domain.Entities;
using BOSGlobal.Crm.Domain.Enums;

namespace BOSGlobal.Crm.Application.Interfaces;

public interface IUserRepository
{
    Task<AppUser?> FindByEmailAsync(string email);

    Task<bool> CheckPasswordAsync(AppUser user, string password);

    Task<IReadOnlyList<UserRole>> GetRolesAsync(AppUser user);
}
