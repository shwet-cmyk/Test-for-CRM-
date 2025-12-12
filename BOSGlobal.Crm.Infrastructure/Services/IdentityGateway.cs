using Microsoft.AspNetCore.Identity;
using BOSGlobal.Crm.Application.DTOs;
using BOSGlobal.Crm.Application.Interfaces;
using BOSGlobal.Crm.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;


namespace BOSGlobal.Crm.Infrastructure.Services;

public class IdentityGateway : IIdentityGateway
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly BOSGlobal.Crm.Application.Interfaces.IPhoneOtpService _otpService;
        

    public IdentityGateway(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, BOSGlobal.Crm.Application.Interfaces.IPhoneOtpService otpService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _otpService = otpService;
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
        if (result.RequiresTwoFactor)
        {
            return new LoginResultDto { Success = false, RequiresTwoFactor = true, UserId = user.Id };
        }

        return new LoginResultDto { Success = false, ErrorMessage = "Invalid login attempt." };
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<LoginResultDto> RegisterAsync(RegisterRequestDto request)
    {
        // basic duplicate check
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
        {
            return new LoginResultDto { Success = false, ErrorMessage = "A user with this email already exists." };
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            ErpId = request.ErpId,
            GstNumber = request.GstNumber
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            return new LoginResultDto { Success = false, ErrorMessage = errors };
        }

        // Optionally send OTP to verify phone number (development stub)
        if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            try
            {
                var otp = await _otpService.SendOtpAsync(user.PhoneNumber);
                // For now we write the OTP to the console for dev/testing; in production integrate SMS provider.
                Console.WriteLine($"OTP for {user.Email}: {otp}");
            }
            catch
            {
                // ignore OTP send failures for now
            }
        }

        // Sign in the user after registration
        await _signInManager.SignInAsync(user, isPersistent: false);

        return new LoginResultDto { Success = true };
    }

    public async Task<LoginResultDto> VerifyPhoneOtpAsync(string email, string code)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return new LoginResultDto { Success = false, ErrorMessage = "User not found." };
        }

        var ok = await _otpService.VerifyOtpAsync(user.PhoneNumber ?? string.Empty, code);
        if (!ok)
        {
            return new LoginResultDto { Success = false, ErrorMessage = "Invalid OTP." };
        }

        // mark phone as confirmed
        user.PhoneNumberConfirmed = true;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
            return new LoginResultDto { Success = false, ErrorMessage = errors };
        }

        return new LoginResultDto { Success = true };
    }

    public async Task<LoginResultDto> VerifyTwoFactorAsync(TwoFactorRequestDto request)
    {
        // Attempt two-factor sign-in using the current two-factor user from the sign-in manager
        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(request.Code, request.RememberMe, request.RememberClient);
        if (result.Succeeded)
        {
            return new LoginResultDto { Success = true };
        }

        if (result.IsLockedOut)
        {
            return new LoginResultDto { Success = false, ErrorMessage = "Account is locked out." };
        }

        return new LoginResultDto { Success = false, ErrorMessage = "Invalid two-factor code." };
    }

    // ...existing code...
}
