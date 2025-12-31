using BOSGlobal.Crm.Application.Interfaces;
using BOSGlobal.Crm.Domain.Entities;
using BOSGlobal.Crm.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BOSGlobal.Crm.Infrastructure.Repositories;

public class RoleRightsRepository : IRoleRightsRepository
{
    private readonly CrmDbContext _db;

    public RoleRightsRepository(CrmDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<RoleMaster>> GetAllRolesAsync()
    {
        if (_db.RoleMasters is null)
        {
            return Enumerable.Empty<RoleMaster>();
        }

        return await _db.RoleMasters.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<RoleRight>> GetRightsForRoleAsync(int roleMasterId)
    {
        if (_db.RoleRights is null)
        {
            return Enumerable.Empty<RoleRight>();
        }

        return await _db.RoleRights.Where(r => r.RoleMasterId == roleMasterId).ToListAsync();
    }

    public async Task SetRightAsync(int roleMasterId, string permissionKey, bool isAllowed)
    {
        if (_db.RoleRights is null)
        {
            return;
        }

        var existing = await _db.RoleRights.FirstOrDefaultAsync(r => r.RoleMasterId == roleMasterId && r.PermissionKey == permissionKey);
        if (existing is null)
        {
            _db.RoleRights.Add(new RoleRight { RoleMasterId = roleMasterId, PermissionKey = permissionKey, IsAllowed = isAllowed });
        }
        else
        {
            existing.IsAllowed = isAllowed;
            _db.RoleRights.Update(existing);
        }

        await _db.SaveChangesAsync();
    }
}
