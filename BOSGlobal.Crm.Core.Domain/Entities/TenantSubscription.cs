namespace BOSGlobal.Crm.Domain.Entities;

public class TenantSubscription
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public int ModuleId { get; set; }
    public DateTime StartDateUtc { get; set; } = DateTime.UtcNow;
    public DateTime? EndDateUtc { get; set; }
    public bool IsActive { get; set; } = true;

    public Tenant? Tenant { get; set; }
    public Module? Module { get; set; }
}
