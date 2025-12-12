using BOSGlobal.Crm.Application.DTOs;
using BOSGlobal.Crm.Application.Interfaces;
using BOSGlobal.Crm.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace BOSGlobal.Crm.Infrastructure.Services;

public class IdentityGateway : IIdentityGateway
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityGateway(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async Task<LoginResultDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return new LoginResultDto { Success = false, ErrorMessage = "Invalid login attempt." };
        }

        var result = await _signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            return new LoginResultDto { Success = true };
        }

        return new LoginResultDto { Success = false, ErrorMessage = "Invalid login attempt." };
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }
}
