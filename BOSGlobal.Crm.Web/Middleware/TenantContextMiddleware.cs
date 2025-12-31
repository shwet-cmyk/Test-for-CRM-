using System.Security.Claims;
using BOSGlobal.Crm.Application.Interfaces;
using BOSGlobal.Crm.Domain.Enums;
using BOSGlobal.Crm.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BOSGlobal.Crm.Web.Middleware;

public class TenantContextMiddleware
{
    private readonly RequestDelegate _next;

    public TenantContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext, CrmDbContext dbContext)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirst("tenantId")?.Value;
            if (Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                ((Services.TenantContext)tenantContext).TenantId = tenantId;
                var tenantName = await dbContext.Tenants!.Where(t => t.Id == tenantId).Select(t => t.Name).FirstOrDefaultAsync();
                ((Services.TenantContext)tenantContext).TenantName = tenantName;
            }

            var realmValue = context.User.FindFirst("realm")?.Value;
            if (Enum.TryParse<Realm>(realmValue, out var realm))
            {
                ((Services.TenantContext)tenantContext).Realm = realm;
            }

            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                ((Services.TenantContext)tenantContext).UserId = userId;
            }
        }

        await _next(context);
    }
}
