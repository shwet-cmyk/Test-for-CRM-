using BOSGlobal.Crm.Application.DTOs.Dashboard;

namespace BOSGlobal.Crm.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardLayoutResponse> GetDashboardAsync(string userId, CancellationToken cancellationToken = default);
    Task<DashboardWidgetDto> SaveWidgetAsync(string userId, SaveWidgetConfigRequest request, CancellationToken cancellationToken = default);
    Task RemoveWidgetAsync(string userId, Guid widgetId, CancellationToken cancellationToken = default);
    Task<string> GetWidgetDataAsync(string userId, Guid widgetId, string? filtersJson, CancellationToken cancellationToken = default);
}
