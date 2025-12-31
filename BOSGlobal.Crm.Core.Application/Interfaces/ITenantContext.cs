using BOSGlobal.Crm.Domain.Enums;

namespace BOSGlobal.Crm.Application.Interfaces;

public interface ITenantContext
{
    Guid? TenantId { get; }
    string? TenantName { get; }
    Realm? Realm { get; }
    string? UserId { get; }
}
