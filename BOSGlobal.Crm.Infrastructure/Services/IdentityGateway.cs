using Microsoft.AspNetCore.Identity;
using BOSGlobal.Crm.Application.DTOs;
using BOSGlobal.Crm.Application.Interfaces;
using BOSGlobal.Crm.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using QRCoder;
using System.IO;
using System.Text;


namespace BOSGlobal.Crm.Infrastructure.Services;

public class IdentityGateway : IIdentityGateway
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly BOSGlobal.Crm.Application.Interfaces.IPhoneOtpService _otpService;
    private readonly BOSGlobal.Crm.Application.Interfaces.ILoginAuditRepository _auditRepository;
        

    public IdentityGateway(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, BOSGlobal.Crm.Application.Interfaces.IPhoneOtpService otpService, BOSGlobal.Crm.Application.Interfaces.ILoginAuditRepository auditRepository)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _otpService = otpService;
        _auditRepository = auditRepository;
    }

    public async Task<LoginResultDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            // log failed attempt
            try
            {
                await _auditRepository.LogAsync(new BOSGlobal.Crm.Domain.Entities.LoginAudit { UserId = request.Email, Success = false, FailureReason = "User not found" });
            }
            catch { }
            return new LoginResultDto { Success = false, ErrorMessage = "Invalid login attempt." };
        }

        // If the user currently has an active session (and it's not expired), instruct client to terminate the other session
        if (!string.IsNullOrEmpty(user.SessionId) && user.SessionExpiresUtc.HasValue && user.SessionExpiresUtc.Value > DateTime.UtcNow)
        {
            return new LoginResultDto { Success = false, ErrorCode = "ActiveSessionExists", ErrorMessage = "A session for this user is already active from another device. Terminate it to continue or wait until it expires." };
        }

        var result = await _signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            // create a new session id and persist to user before issuing final cookie
            var sessionId = Guid.NewGuid().ToString("N");
            user.SessionId = sessionId;
            user.SessionExpiresUtc = DateTime.UtcNow.AddMinutes(10); // session TTL
            user.LastActivityUtc = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Re-issue sign-in so the updated claims (sessionId) are baked into the cookie
            await _signInManager.SignInAsync(user, request.RememberMe);

            // record login audit
            try
            {
                await _auditRepository.LogAsync(new BOSGlobal.Crm.Domain.Entities.LoginAudit { UserId = user.Id, Success = true, Device = request.DeviceInfo, Location = request.Location });
            }
            catch { }

            var roles = await _userManager.GetRolesAsync(user);
            var redirect = GetRedirectForRoles(roles);
            return new LoginResultDto { Success = true, Roles = roles, RedirectUrl = redirect };
        }
        if (result.RequiresTwoFactor)
        {
            return new LoginResultDto { Success = false, RequiresTwoFactor = true, UserId = user.Id };
        }

        // log failed attempt
        try
        {
            await _auditRepository.LogAsync(new BOSGlobal.Crm.Domain.Entities.LoginAudit { UserId = user.Id, Success = false, FailureReason = "Invalid password" });
        }
        catch { }

        return new LoginResultDto { Success = false, ErrorMessage = "Invalid login attempt." };
    }

    private string GetRedirectForRoles(IList<string> roles)
    {
        // simple precedence mapping
        if (roles.Contains("Admin", StringComparer.OrdinalIgnoreCase)) return "/dashboards/admin";
        if (roles.Contains("Manager", StringComparer.OrdinalIgnoreCase)) return "/dashboards/manager";
        if (roles.Contains("SalesExecutive", StringComparer.OrdinalIgnoreCase) || roles.Contains("Sales", StringComparer.OrdinalIgnoreCase)) return "/dashboards/sales";
        if (roles.Contains("Telecaller", StringComparer.OrdinalIgnoreCase)) return "/dashboards/telecaller";
        if (roles.Contains("Marketing", StringComparer.OrdinalIgnoreCase)) return "/dashboards/marketing";
        return "/";
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task TerminateActiveSessionAsync(string userIdOrEmail)
    {
        ApplicationUser? user = null;
        // try by id first
        if (!string.IsNullOrWhiteSpace(userIdOrEmail))
        {
            user = await _userManager.FindByIdAsync(userIdOrEmail);
            if (user is null)
            {
                user = await _userManager.FindByEmailAsync(userIdOrEmail);
            }
        }

        if (user is null) return;

        user.SessionId = null;
        user.SessionExpiresUtc = null;
        user.LastActivityUtc = null;
        await _userManager.UpdateAsync(user);
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

    public async Task<string> GenerateAuthenticatorQrAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) throw new InvalidOperationException("User not found.");

        // Ensure an authenticator key exists
        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            key = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        // Build otpauth URI following the standard used by authenticator apps
        var issuer = Uri.EscapeDataString("BOSGlobal.Crm");
        var label = Uri.EscapeDataString(user.Email ?? user.UserName ?? "user");
        var otpauth = $"otpauth://totp/{issuer}:{label}?secret={key}&issuer={issuer}&digits=6";

        // Generate QR code PNG and return as data URI
        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(otpauth, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);
        var qrBytes = qrCode.GetGraphic(20);
        var base64 = Convert.ToBase64String(qrBytes);
        return $"data:image/png;base64,{base64}";
    }

    public async Task<bool> EnableAuthenticatorAsync(string userId, string verificationCode)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return false;

        // Verify the provided code using the authenticator token provider
        var valid = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);
        if (!valid) return false;

        user.TwoFactorEnabled = true;
        var res = await _userManager.UpdateAsync(user);
        return res.Succeeded;
    }

    public async Task<IEnumerable<string>> GenerateRecoveryCodesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) throw new InvalidOperationException("User not found.");

        var codes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        return codes;
    }

    // ...existing code...
}
