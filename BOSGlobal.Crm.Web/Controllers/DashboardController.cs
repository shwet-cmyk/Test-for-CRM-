using BOSGlobal.Crm.Application.DTOs.Dashboard;
using BOSGlobal.Crm.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BOSGlobal.Crm.Web.Controllers;

[ApiController]
[Route("api/dashboard/v1")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("widgets")]
    public async Task<ActionResult<DashboardLayoutResponse>> GetWidgetsAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var layout = await _dashboardService.GetDashboardAsync(userId, cancellationToken);
        return Ok(layout);
    }

    [HttpPost("widgets")]
    public async Task<ActionResult<DashboardWidgetDto>> SaveWidgetAsync([FromBody] SaveWidgetConfigRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var widget = await _dashboardService.SaveWidgetAsync(userId, request, cancellationToken);
        return Ok(widget);
    }

    [HttpDelete("widgets/{widgetId:guid}")]
    public async Task<IActionResult> RemoveWidgetAsync([FromRoute] Guid widgetId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        await _dashboardService.RemoveWidgetAsync(userId, widgetId, cancellationToken);
        return NoContent();
    }

    [HttpPost("widgets/{widgetId:guid}/data")]
    public async Task<ActionResult<string>> GetWidgetDataAsync([FromRoute] Guid widgetId, [FromBody] object? filters, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var data = await _dashboardService.GetWidgetDataAsync(userId, widgetId, filters?.ToString(), cancellationToken);
        return Ok(data);
    }
}
