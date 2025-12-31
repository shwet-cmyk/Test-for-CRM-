namespace BOSGlobal.Crm.Application.DTOs;

public class TenantSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Realm { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
}
