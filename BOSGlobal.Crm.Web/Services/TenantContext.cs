using BOSGlobal.Crm.Application.Interfaces;
using BOSGlobal.Crm.Domain.Enums;

namespace BOSGlobal.Crm.Web.Services;

public class TenantContext : ITenantContext
{
    public Guid? TenantId { get; internal set; }
    public string? TenantName { get; internal set; }
    public Realm? Realm { get; internal set; }
    public string? UserId { get; internal set; }
}
