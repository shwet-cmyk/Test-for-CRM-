using BOSGlobal.Crm.Application.DTOs;

namespace BOSGlobal.Crm.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResultDto> LoginAsync(LoginRequestDto request);

    Task LogoutAsync();
}
