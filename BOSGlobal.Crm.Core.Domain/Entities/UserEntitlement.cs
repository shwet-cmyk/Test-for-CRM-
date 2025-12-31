namespace BOSGlobal.Crm.Domain.Entities;

public class UserEntitlement
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public int ModuleId { get; set; }
    public DateTime GrantedAtUtc { get; set; } = DateTime.UtcNow;
    public string? GrantedByUserId { get; set; }

    public Module? Module { get; set; }
    public Tenant? Tenant { get; set; }
}
