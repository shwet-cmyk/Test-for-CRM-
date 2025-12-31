using BOSGlobal.Crm.Application.Interfaces;
using BOSGlobal.Crm.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BOSGlobal.Crm.Web.Authorization;

public class HasModuleAccessHandler : AuthorizationHandler<HasModuleAccessRequirement>
{
    private readonly ITenantContext _tenantContext;
    private readonly CrmDbContext _dbContext;

    public HasModuleAccessHandler(ITenantContext tenantContext, CrmDbContext dbContext)
    {
        _tenantContext = tenantContext;
        _dbContext = dbContext;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, HasModuleAccessRequirement requirement)
    {
        if (_tenantContext.TenantId is null || string.IsNullOrEmpty(_tenantContext.UserId))
        {
            return;
        }

        var hasAccess = await _dbContext.UserEntitlements!
            .Include(ue => ue.Module)
            .AnyAsync(ue => ue.UserId == _tenantContext.UserId
                            && ue.TenantId == _tenantContext.TenantId
                            && ue.Module!.Key == requirement.ModuleKey);

        if (hasAccess)
        {
            context.Succeed(requirement);
        }
    }
}
