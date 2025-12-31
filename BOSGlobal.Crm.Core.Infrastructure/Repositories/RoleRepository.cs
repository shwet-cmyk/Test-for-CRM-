using BOSGlobal.Crm.Application.Interfaces;
using BOSGlobal.Crm.Domain.Enums;
using BOSGlobal.Crm.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace BOSGlobal.Crm.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly RoleManager<ApplicationRole> _roleManager;

    public RoleRepository(RoleManager<ApplicationRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<IReadOnlyList<UserRole>> GetAllRolesAsync()
    {
        var roles = _roleManager.Roles.ToList();
        return await Task.FromResult(roles.Select(r => Enum.TryParse<UserRole>(r.Name, out var parsed) ? parsed : UserRole.Support).ToList());
    }
}
