using BOSGlobal.Crm.Application.DTOs;

namespace BOSGlobal.Crm.Application.Interfaces;

public interface IIdentityGateway
{
    Task<LoginResultDto> LoginAsync(LoginRequestDto request);

    Task LogoutAsync();
}
