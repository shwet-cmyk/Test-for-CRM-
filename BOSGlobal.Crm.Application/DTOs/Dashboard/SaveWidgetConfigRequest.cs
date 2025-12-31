using System.ComponentModel.DataAnnotations;

namespace BOSGlobal.Crm.Application.DTOs.Dashboard;

public class SaveWidgetConfigRequest
{
    [Required]
    public Guid WidgetId { get; set; }
    public string? Title { get; set; }
    public string? LayoutJson { get; set; }
    public string? FiltersJson { get; set; }
    public int? OrderIndex { get; set; }
}
