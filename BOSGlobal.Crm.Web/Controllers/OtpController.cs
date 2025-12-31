using BOSGlobal.Crm.Application.DTOs.Security;
using BOSGlobal.Crm.Application.Exceptions;
using BOSGlobal.Crm.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BOSGlobal.Crm.Web.Controllers;

[ApiController]
[Route("api/security/v1/otp")]
public class OtpController : ControllerBase
{
    private readonly ISecurityGatewayService _securityGatewayService;
    private readonly ILogger<OtpController> _logger;

    public OtpController(ISecurityGatewayService securityGatewayService, ILogger<OtpController> logger)
    {
        _securityGatewayService = securityGatewayService;
        _logger = logger;
    }

    [HttpPost("send")]
    public async Task<ActionResult<OtpSendResponse>> SendAsync([FromBody] OtpSendRequest request, CancellationToken cancellationToken)
        {
            request.RemoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            try
            {
                var result = await _securityGatewayService.SendOtpAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (SecurityGatewayException ex)
            {
                _logger.LogWarning(ex, "OTP send failed for {Destination}", request.Destination);
                return Problem(detail: ex.Message, statusCode: ex.StatusCode, title: "OTP send denied");
            }
        }

    [HttpPost("verify")]
    public async Task<ActionResult<OtpVerifyResponse>> VerifyAsync([FromBody] OtpVerifyRequest request, CancellationToken cancellationToken)
    {
        request.RemoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _securityGatewayService.VerifyOtpAsync(request, cancellationToken);
        if (!result.Success)
        {
            return Problem(detail: result.FailureReason ?? "Verification failed", statusCode: StatusCodes.Status400BadRequest, title: "OTP verification failed");
        }

        return Ok(result);
    }

    [HttpPost("resend")]
    public async Task<ActionResult<OtpSendResponse>> ResendAsync([FromBody] OtpResendRequest request, CancellationToken cancellationToken)
        {
            request.RemoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            try
            {
                var result = await _securityGatewayService.ResendOtpAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (SecurityGatewayException ex)
            {
                _logger.LogWarning(ex, "OTP resend failed for correlation {CorrelationId}", request.CorrelationId);
                return Problem(detail: ex.Message, statusCode: ex.StatusCode, title: "OTP resend denied");
            }
        }
}
