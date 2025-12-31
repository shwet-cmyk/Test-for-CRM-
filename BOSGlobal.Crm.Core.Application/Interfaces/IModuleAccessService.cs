using BOSGlobal.Crm.Application.DTOs;

namespace BOSGlobal.Crm.Application.Interfaces;

public interface IModuleAccessService
{
    Task<IEnumerable<ModuleDto>> GetModulesForUserAsync(string userId, Guid tenantId);
}
