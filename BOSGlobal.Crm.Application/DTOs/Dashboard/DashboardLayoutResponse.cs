namespace BOSGlobal.Crm.Application.DTOs.Dashboard;

public class DashboardLayoutResponse
{
    public IEnumerable<DashboardWidgetDto> Widgets { get; set; } = Array.Empty<DashboardWidgetDto>();
    public IEnumerable<string> GlobalFilters { get; set; } = Array.Empty<string>();
    public bool UsedDefaultLayout { get; set; }
}
