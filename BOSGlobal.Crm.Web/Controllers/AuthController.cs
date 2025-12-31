using BOSGlobal.Crm.Application.DTOs;
using BOSGlobal.Crm.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BOSGlobal.Crm.Web.Controllers;

[ApiController]
[Route("api/security/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResultDto>> LoginAsync([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        request.DeviceInfo ??= Request.Headers.UserAgent.ToString();
        request.Location ??= HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = await _authService.LoginAsync(request);
        if (!result.Success)
        {
            var status = result.ErrorCode == "RoleMissing" ? StatusCodes.Status403Forbidden : StatusCodes.Status401Unauthorized;
            return Problem(detail: result.ErrorMessage ?? "Login failed.", statusCode: status, title: "Login failed");
        }

        return Ok(result);
    }
}
