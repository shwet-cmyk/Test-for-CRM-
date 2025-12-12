using BOSGlobal.Crm.Application.DTOs;

namespace BOSGlobal.Crm.Application.Interfaces;

public interface IIdentityGateway
{
    Task<LoginResultDto> LoginAsync(LoginRequestDto request);
    
    Task<LoginResultDto> RegisterAsync(RegisterRequestDto request);

    Task<LoginResultDto> VerifyPhoneOtpAsync(string email, string code);

    Task<LoginResultDto> VerifyTwoFactorAsync(TwoFactorRequestDto request);

    Task LogoutAsync();
}
