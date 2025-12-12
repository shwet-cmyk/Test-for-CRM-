using BOSGlobal.Crm.Application.DTOs;
using BOSGlobal.Crm.Application.Interfaces;

namespace BOSGlobal.Crm.Application.Services;

public class AuthService : IAuthService
{
    private readonly IIdentityGateway _identityGateway;
    private readonly IRecaptchaService _recaptchaService;

    public AuthService(IIdentityGateway identityGateway, IRecaptchaService recaptchaService)
    {
        _identityGateway = identityGateway;
        _recaptchaService = recaptchaService;
    }

    public async Task<LoginResultDto> LoginAsync(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return new LoginResultDto { Success = false, ErrorMessage = "Email is required." };
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return new LoginResultDto { Success = false, ErrorMessage = "Password is required." };
        }

        // Verify reCAPTCHA token (if configured)
        var recaptchaOk = await _recaptchaService.VerifyTokenAsync(request.RecaptchaToken);
        if (!recaptchaOk)
        {
            return new LoginResultDto { Success = false, ErrorMessage = "reCAPTCHA verification failed." };
        }

        return await _identityGateway.LoginAsync(request);
    }

    public Task LogoutAsync()
    {
        return _identityGateway.LogoutAsync();
    }

    public async Task<LoginResultDto> RegisterAsync(RegisterRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return new LoginResultDto { Success = false, ErrorMessage = "Email is required." };
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return new LoginResultDto { Success = false, ErrorMessage = "Password is required." };
        }

        var recaptchaOk = await _recaptchaService.VerifyTokenAsync(request.RecaptchaToken);
        if (!recaptchaOk)
        {
            return new LoginResultDto { Success = false, ErrorMessage = "reCAPTCHA verification failed." };
        }

        return await _identityGateway.RegisterAsync(request);
    }

    public Task<LoginResultDto> VerifyPhoneOtpAsync(string email, string code)
    {
        return _identityGateway.VerifyPhoneOtpAsync(email, code);
    }

    public Task<LoginResultDto> VerifyTwoFactorAsync(TwoFactorRequestDto request)
    {
        return _identityGateway.VerifyTwoFactorAsync(request);
    }
}
