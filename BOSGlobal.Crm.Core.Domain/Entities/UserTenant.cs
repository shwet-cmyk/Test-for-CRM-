using BOSGlobal.Crm.Domain.Enums;

namespace BOSGlobal.Crm.Domain.Entities;

public class UserTenant
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public Realm Realm { get; set; }
    public bool IsAdmin { get; set; }

    public Tenant? Tenant { get; set; }
}
