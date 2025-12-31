using BOSGlobal.Crm.Application.DTOs;

namespace BOSGlobal.Crm.Application.Interfaces;

public interface IRoleAccessService
{
    Task<RoleAccessProfileDto> BuildAccessProfileAsync(string userId, CancellationToken cancellationToken = default);
}
