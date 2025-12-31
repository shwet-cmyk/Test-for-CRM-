using BOSGlobal.Crm.Application.DTOs.Security;

namespace BOSGlobal.Crm.Application.Interfaces;

public interface ISecurityGatewayService
{
    Task<CaptchaVerifyResponse> VerifyCaptchaAsync(CaptchaVerifyRequest request, CancellationToken cancellationToken = default);
    Task<OtpSendResponse> SendOtpAsync(OtpSendRequest request, CancellationToken cancellationToken = default);
    Task<OtpVerifyResponse> VerifyOtpAsync(OtpVerifyRequest request, CancellationToken cancellationToken = default);
    Task<OtpSendResponse> ResendOtpAsync(OtpResendRequest request, CancellationToken cancellationToken = default);
}
