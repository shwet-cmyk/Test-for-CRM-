using BOSGlobal.Crm.Application.DTOs;

namespace BOSGlobal.Crm.Application.Interfaces;

public interface IIdentityGateway
{
    Task<LoginResultDto> LoginAsync(LoginRequestDto request);
    
    Task<LoginResultDto> RegisterAsync(RegisterRequestDto request);

    Task<LoginResultDto> VerifyPhoneOtpAsync(string email, string code);

    Task<LoginResultDto> VerifyTwoFactorAsync(TwoFactorRequestDto request);

    Task LogoutAsync();

    // 2FA / authenticator setup helpers
    Task<string> GenerateAuthenticatorQrAsync(string userId);
    Task<bool> EnableAuthenticatorAsync(string userId, string verificationCode);
    Task<IEnumerable<string>> GenerateRecoveryCodesAsync(string userId);
    
    // Terminate any existing active session for the user (admin or self-requested) so a new session can be created.
    Task TerminateActiveSessionAsync(string userIdOrEmail);

    Task<IEnumerable<TenantSummaryDto>> GetTenantsForCurrentUserAsync();

    Task<LoginResultDto> SwitchTenantAsync(Guid tenantId);
}
