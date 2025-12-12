using BOSGlobal.Crm.Domain.Enums;

namespace BOSGlobal.Crm.Application.Interfaces;

public interface IRoleRepository
{
    Task<IReadOnlyList<UserRole>> GetAllRolesAsync();
}
