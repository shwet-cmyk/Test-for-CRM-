using System.ComponentModel.DataAnnotations;

namespace BOSGlobal.Crm.Domain.Entities;

public class WidgetDataCache
{
    public Guid Id { get; set; }
    public Guid WidgetId { get; set; }
    [MaxLength(450)]
    public string? UserId { get; set; }
    [MaxLength(256)]
    public string FiltersHash { get; set; } = string.Empty;
    public string DataJson { get; set; } = string.Empty;
    public DateTime CachedUtc { get; set; }
    public DateTime ExpiresUtc { get; set; }
}
