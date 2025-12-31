using System.ComponentModel.DataAnnotations;

namespace BOSGlobal.Crm.Domain.Entities;

public class WidgetLibrary
{
    public Guid Id { get; set; }
    [MaxLength(128)]
    public string Key { get; set; } = string.Empty;
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;
    [MaxLength(128)]
    public string Category { get; set; } = string.Empty;
    [MaxLength(64)]
    public string GraphType { get; set; } = string.Empty;
    [MaxLength(256)]
    public string DataSource { get; set; } = string.Empty;
    public string? DefaultConfigJson { get; set; }
    public string? DefaultFiltersJson { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}
