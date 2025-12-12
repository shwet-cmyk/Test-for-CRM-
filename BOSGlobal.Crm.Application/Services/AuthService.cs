using BOSGlobal.Crm.Application.DTOs;
using BOSGlobal.Crm.Application.Interfaces;

namespace BOSGlobal.Crm.Application.Services;

public class AuthService : IAuthService
{
    private readonly IIdentityGateway _identityGateway;

    public AuthService(IIdentityGateway identityGateway)
    {
        _identityGateway = identityGateway;
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

        return await _identityGateway.LoginAsync(request);
    }

    public Task LogoutAsync()
    {
        return _identityGateway.LogoutAsync();
    }
}
