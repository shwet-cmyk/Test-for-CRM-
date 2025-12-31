using BOSGlobal.Crm.Application.DTOs.Security;
using BOSGlobal.Crm.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BOSGlobal.Crm.Web.Controllers;

[ApiController]
[Route("api/security/v1/captcha")]
public class CaptchaController : ControllerBase
{
    private readonly ISecurityGatewayService _securityGatewayService;

    public CaptchaController(ISecurityGatewayService securityGatewayService)
    {
        _securityGatewayService = securityGatewayService;
    }

    [HttpPost("verify")]
    public async Task<ActionResult<CaptchaVerifyResponse>> VerifyAsync([FromBody] CaptchaVerifyRequest request, CancellationToken cancellationToken)
    {
        request.RemoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        request.UserAgent = Request.Headers.UserAgent.ToString();

        var result = await _securityGatewayService.VerifyCaptchaAsync(request, cancellationToken);
        if (!result.Success)
        {
            return Problem(detail: result.Reason ?? "Captcha validation failed", statusCode: StatusCodes.Status400BadRequest, title: "Captcha validation failed");
        }

        return Ok(result);
    }
}
