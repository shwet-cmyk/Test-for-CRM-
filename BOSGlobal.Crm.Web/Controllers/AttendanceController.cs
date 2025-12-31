using BOSGlobal.Crm.Application.DTOs.Attendance;
using BOSGlobal.Crm.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BOSGlobal.Crm.Web.Controllers;

[ApiController]
[Route("api/attendance/v1")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpPost("punch")]
    public async Task<ActionResult<PunchResponseDto>> PunchAsync([FromBody] PunchRequestDto request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var response = await _attendanceService.PunchAsync(userId, request, cancellationToken);
        if (!response.Success)
        {
            return Problem(detail: response.Message, statusCode: StatusCodes.Status400BadRequest, title: "Punch failed");
        }

        return Ok(response);
    }

    [HttpGet("rules")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<AttendanceRuleDto>>> GetRulesAsync(CancellationToken cancellationToken)
    {
        var rules = await _attendanceService.GetRulesAsync(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty, cancellationToken);
        return Ok(rules);
    }

    [HttpPost("rules")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AttendanceRuleDto>> SaveRuleAsync([FromBody] AttendanceRuleDto request, CancellationToken cancellationToken)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var rule = await _attendanceService.SaveRuleAsync(request, adminId, cancellationToken);
        return Ok(rule);
    }

    [HttpPost("override")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> OverrideAsync([FromBody] OverrideRequestDto request, CancellationToken cancellationToken)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        await _attendanceService.OverrideAsync(adminId, request, cancellationToken);
        return NoContent();
    }
}
