using BOSGlobal.Crm.Application.Interfaces;
using BOSGlobal.Crm.Domain.Enums;

namespace BOSGlobal.Crm.Maui.Services;

public class MauiTenantContext : ITenantContext
{
    public Guid? TenantId => null;

    public string? TenantName => null;

    public Realm? Realm => null;

    public string? UserId => null;
}
