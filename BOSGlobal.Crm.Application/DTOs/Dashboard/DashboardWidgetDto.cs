namespace BOSGlobal.Crm.Application.DTOs.Dashboard;

public class DashboardWidgetDto
{
    public Guid WidgetId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string GraphType { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public string? LayoutJson { get; set; }
    public string? FiltersJson { get; set; }
    public int OrderIndex { get; set; }
    public bool FromRoleDefault { get; set; }
}
