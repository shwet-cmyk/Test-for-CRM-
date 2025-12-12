using BOSGlobal.Crm.Application.DTOs;
using BOSGlobal.Crm.Application.Interfaces;

namespace BOSGlobal.Crm.Application.Services;

public class UserQueryService
{
    private readonly IUserRepository _userRepository;

    public UserQueryService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        var user = await _userRepository.FindByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        var roles = await _userRepository.GetRolesAsync(user);

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Roles = roles.Select(r => r.ToString()).ToList(),
            IsActive = user.IsActive
        };
    }
}
