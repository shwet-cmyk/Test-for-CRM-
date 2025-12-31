using BOSGlobal.Crm.Application.DTOs;
using BOSGlobal.Crm.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BOSGlobal.Crm.Infrastructure.Services;

public class ModuleAccessService : IModuleAccessService
{
    private readonly Identity.CrmDbContext _db;

    public ModuleAccessService(Identity.CrmDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ModuleDto>> GetModulesForUserAsync(string userId, Guid tenantId)
    {
        var modules = await _db.UserEntitlements!
            .Where(ue => ue.UserId == userId && ue.TenantId == tenantId)
            .Include(ue => ue.Module)
            .Select(ue => ue.Module!)
            .Distinct()
            .ToListAsync();

        return modules.Select(m => new ModuleDto
        {
            Id = m.Id,
            Key = m.Key,
            Name = m.Name,
            Description = m.Description
        });
    }
}
