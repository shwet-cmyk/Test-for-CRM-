using BOSGlobal.Crm.Domain.Entities;

namespace BOSGlobal.Crm.Application.Interfaces;

public interface IRoleRightsRepository
{
    Task<IEnumerable<RoleMaster>> GetAllRolesAsync();
    Task<IEnumerable<RoleRight>> GetRightsForRoleAsync(int roleMasterId);
    Task SetRightAsync(int roleMasterId, string permissionKey, bool isAllowed);
}
