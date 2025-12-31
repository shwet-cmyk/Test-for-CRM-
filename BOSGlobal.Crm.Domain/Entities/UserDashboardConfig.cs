using System.ComponentModel.DataAnnotations;

namespace BOSGlobal.Crm.Domain.Entities;

public class UserDashboardConfig
{
    public Guid Id { get; set; }
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;
    public Guid WidgetId { get; set; }
    [MaxLength(256)]
    public string? TitleOverride { get; set; }
    public string? LayoutJson { get; set; }
    public string? FiltersJson { get; set; }
    public bool IsRemoved { get; set; }
    public int OrderIndex { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}
