using BOSGlobal.Crm.Application.DTOs;

namespace BOSGlobal.Crm.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResultDto> LoginAsync(LoginRequestDto request);

    Task<LoginResultDto> RegisterAsync(RegisterRequestDto request);

    Task<LoginResultDto> VerifyPhoneOtpAsync(string email, string code);

    Task<LoginResultDto> VerifyTwoFactorAsync(TwoFactorRequestDto request);

    Task LogoutAsync();

    // 2FA helpers - pass-through to underlying identity gateway
    Task<string> GenerateAuthenticatorQrAsync(string userId);
    Task<bool> EnableAuthenticatorAsync(string userId, string verificationCode);
    Task<IEnumerable<string>> GenerateRecoveryCodesAsync(string userId);

    Task TerminateActiveSessionAsync(string userIdOrEmail);
}
