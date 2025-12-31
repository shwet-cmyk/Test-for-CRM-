namespace BOSGlobal.Crm.Application.DTOs.Dashboard;

public class WidgetDataResponse
{
    public Guid WidgetId { get; set; }
    public string DataJson { get; set; } = string.Empty;
    public DateTime CachedUtc { get; set; }
}
