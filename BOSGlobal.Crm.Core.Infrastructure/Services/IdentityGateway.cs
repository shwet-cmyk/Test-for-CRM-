using BOSGlobal.Crm.Application.DTOs;
using BOSGlobal.Crm.Application.Interfaces;
using BOSGlobal.Crm.Domain.Entities;
using BOSGlobal.Crm.Domain.Enums;
using BOSGlobal.Crm.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Security.Claims;

namespace BOSGlobal.Crm.Infrastructure.Services;

public class IdentityGateway : IIdentityGateway
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPhoneOtpService _otpService;
    private readonly ILoginAuditRepository _auditRepository;
    private readonly CrmDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IdentityGateway(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IPhoneOtpService otpService,
        ILoginAuditRepository auditRepository,
        CrmDbContext dbContext,
        IHttpContextAccessor httpContextAccessor)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _otpService = otpService;
        _auditRepository = auditRepository;
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<LoginResultDto> LoginAsync(LoginRequestDto request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email)
                       ?? await _userManager.FindByNameAsync(request.Email);
            if (user is null)
            {
                await LogAuditSafe(request.Email, false, "User not found");
                return new LoginResultDto { Success = false, ErrorMessage = "Invalid login attempt." };
            }

            // realm selection
            var realmText = request.SelectedRealm ?? request.SelectedRole;
            if (!Enum.TryParse<Realm>(realmText, true, out var realm))
            {
                return new LoginResultDto { Success = false, ErrorMessage = "Select a valid realm (User, Partner, Employee)." };
            }

            if (!user.IsActive)
            {
                await LogAuditSafe(user.Id, false, "Inactive user");
                return new LoginResultDto { Success = false, ErrorMessage = "Your account is inactive. Contact your administrator." };
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                await LogAuditSafe(user.Id, false, "Locked out");
                return new LoginResultDto { Success = false, ErrorMessage = "Your account is locked. Please try again later." };
            }

            // active session guard: invalidate previous and proceed
            if (!string.IsNullOrEmpty(user.SessionId) && user.SessionExpiresUtc.HasValue && user.SessionExpiresUtc.Value > DateTime.UtcNow)
            {
                user.SessionId = null;
                user.SessionExpiresUtc = null;
                await _userManager.UpdateAsync(user);
                await LogAuditSafe(user.Id, true, "Previous session invalidated");
            }

            var passwordResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (passwordResult.RequiresTwoFactor)
            {
                return new LoginResultDto { Success = false, RequiresTwoFactor = true, UserId = user.Id };
            }
            if (passwordResult.IsLockedOut)
            {
                await LogAuditSafe(user.Id, false, "Locked after failures");
                return new LoginResultDto { Success = false, ErrorMessage = "Your account is locked. Please try again later." };
            }
            if (!passwordResult.Succeeded)
            {
                await LogAuditSafe(user.Id, false, "Invalid password");
                return new LoginResultDto { Success = false, ErrorMessage = "Invalid login attempt." };
            }

            var userTenants = await _dbContext.UserTenants!
                .Include(ut => ut.Tenant)
                .Where(ut => ut.UserId == user.Id && ut.Realm == realm)
                .ToListAsync();

            if (!userTenants.Any())
            {
                return new LoginResultDto
                {
                    Success = false,
                    ErrorMessage = "No tenants are linked to this user for the selected realm.",
                    Tenants = Enumerable.Empty<TenantSummaryDto>()
                };
            }

            if (!request.SelectedTenantId.HasValue && userTenants.Count > 1)
            {
                return new LoginResultDto
                {
                    Success = false,
                    ErrorCode = "TenantSelectionRequired",
                    ErrorMessage = "Select a tenant to continue.",
                    Tenants = userTenants.Select(t => new TenantSummaryDto { Id = t.TenantId, Name = t.Tenant!.Name, Realm = realm.ToString(), IsAdmin = t.IsAdmin })
                };
            }

            var tenantId = request.SelectedTenantId ?? userTenants.First().TenantId;
            var selectedTenant = userTenants.FirstOrDefault(t => t.TenantId == tenantId);
            if (selectedTenant is null)
            {
                return new LoginResultDto
                {
                    Success = false,
                    ErrorCode = "TenantSelectionRequired",
                    ErrorMessage = "Select a tenant to continue.",
                    Tenants = userTenants.Select(t => new TenantSummaryDto { Id = t.TenantId, Name = t.Tenant!.Name, Realm = realm.ToString(), IsAdmin = t.IsAdmin })
                };
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles is null || !roles.Any())
            {
                await LogAuditSafe(user.Id, false, "No roles assigned");
                return new LoginResultDto { Success = false, ErrorMessage = "No roles are assigned to this account. Contact your administrator." };
            }

            // create new session id
            var sessionId = Guid.NewGuid().ToString("N");
            user.SessionId = sessionId;
            user.SessionExpiresUtc = DateTime.UtcNow.AddMinutes(10);
            user.LastActivityUtc = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var additionalClaims = new List<Claim>
            {
                new("sessionId", sessionId),
                new("tenantId", tenantId.ToString()),
                new("realm", realm.ToString())
            };

            var principal = await _signInManager.CreateUserPrincipalAsync(user);
            ((ClaimsIdentity)principal.Identity!).AddClaims(additionalClaims);
            await _signInManager.Context.SignInAsync(IdentityConstants.ApplicationScheme, principal, new AuthenticationProperties { IsPersistent = request.RememberMe });

            await LogAuditSafe(user.Id, true, null, request.DeviceInfo, request.Location);

            var modules = await GetEntitledModulesAsync(user.Id, tenantId);

            return new LoginResultDto
            {
                Success = true,
                Roles = roles,
                RedirectUrl = "/apps",
                Tenants = userTenants.Select(t => new TenantSummaryDto { Id = t.TenantId, Name = t.Tenant!.Name, Realm = realm.ToString(), IsAdmin = t.IsAdmin }),
                Modules = modules
            };
        }
        catch
        {
            return new LoginResultDto { Success = false, ErrorMessage = "Unable to sign in right now. Please try again." };
        }
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task TerminateActiveSessionAsync(string userIdOrEmail)
    {
        ApplicationUser? user = null;
        if (!string.IsNullOrWhiteSpace(userIdOrEmail))
        {
            user = await _userManager.FindByIdAsync(userIdOrEmail) ?? await _userManager.FindByEmailAsync(userIdOrEmail);
        }

        if (user is null) return;

        user.SessionId = null;
        user.SessionExpiresUtc = null;
        user.LastActivityUtc = null;
        await _userManager.UpdateAsync(user);
    }

    public async Task<LoginResultDto> RegisterAsync(RegisterRequestDto request)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
        {
            return new LoginResultDto { Success = false, ErrorMessage = "A user with this email already exists." };
        }

        if (!Enum.TryParse<Realm>(request.Realm, true, out var realm) || realm == Realm.Employee)
        {
            return new LoginResultDto { Success = false, ErrorMessage = "Realm must be User or Partner for self signup." };
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

        // bootstrap tenant
        var tenantName = string.IsNullOrWhiteSpace(request.TenantName) ? $"{request.Email.Split('@').First()} Tenant" : request.TenantName;
        var tenant = new Tenant { Name = tenantName };
        _dbContext.Tenants!.Add(tenant);
        await _dbContext.SaveChangesAsync();

        // ensure requested modules exist
        var requestedModuleKeys = request.SelectedModuleKeys?.Any() == true ? request.SelectedModuleKeys : new List<string> { "CRM" };
        var modules = await _dbContext.Modules!.Where(m => requestedModuleKeys!.Contains(m.Key)).ToListAsync();
        var missingKeys = requestedModuleKeys!.Except(modules.Select(m => m.Key), StringComparer.OrdinalIgnoreCase);
        foreach (var key in missingKeys)
        {
            var module = new Module { Key = key, Name = key };
            _dbContext.Modules!.Add(module);
            modules.Add(module);
        }
        await _dbContext.SaveChangesAsync();

        foreach (var module in modules)
        {
            _dbContext.TenantSubscriptions!.Add(new TenantSubscription { TenantId = tenant.Id, ModuleId = module.Id, IsActive = true });
            _dbContext.UserEntitlements!.Add(new UserEntitlement { TenantId = tenant.Id, ModuleId = module.Id, UserId = user.Id, GrantedByUserId = user.Id });
        }

        _dbContext.UserTenants!.Add(new UserTenant { TenantId = tenant.Id, UserId = user.Id, Realm = realm, IsAdmin = true });
        await _dbContext.SaveChangesAsync();

        await IssueSignInAsync(user, realm, tenant.Id, isPersistent: false);

        return new LoginResultDto
        {
            Success = true,
            Tenants = new[] { new TenantSummaryDto { Id = tenant.Id, Name = tenant.Name, Realm = realm.ToString(), IsAdmin = true } },
            Modules = await GetEntitledModulesAsync(user.Id, tenant.Id),
            RedirectUrl = "/apps"
        };
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

    public async Task<IEnumerable<TenantSummaryDto>> GetTenantsForCurrentUserAsync()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Enumerable.Empty<TenantSummaryDto>();
        }

        var userTenants = await _dbContext.UserTenants!
            .Include(ut => ut.Tenant)
            .Where(ut => ut.UserId == userId)
            .ToListAsync();

        return userTenants.Select(t => new TenantSummaryDto { Id = t.TenantId, Name = t.Tenant!.Name, Realm = t.Realm.ToString(), IsAdmin = t.IsAdmin });
    }

    public async Task<LoginResultDto> SwitchTenantAsync(Guid tenantId)
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return new LoginResultDto { Success = false, ErrorMessage = "Not authenticated." };
        }

        var mapping = await _dbContext.UserTenants!
            .Include(ut => ut.Tenant)
            .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TenantId == tenantId);
        if (mapping is null)
        {
            return new LoginResultDto { Success = false, ErrorMessage = "Tenant access not found." };
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return new LoginResultDto { Success = false, ErrorMessage = "User not found." };
        }

        await IssueSignInAsync(user, mapping.Realm, tenantId, isPersistent: true);

        return new LoginResultDto
        {
            Success = true,
            Tenants = await GetTenantsForCurrentUserAsync(),
            Modules = await GetEntitledModulesAsync(userId, tenantId),
            RedirectUrl = "/apps"
        };
    }

    private async Task IssueSignInAsync(ApplicationUser user, Realm realm, Guid tenantId, bool isPersistent)
    {
        var sessionId = Guid.NewGuid().ToString("N");
        user.SessionId = sessionId;
        user.SessionExpiresUtc = DateTime.UtcNow.AddMinutes(10);
        user.LastActivityUtc = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var additionalClaims = new List<Claim>
        {
            new("sessionId", sessionId),
            new("tenantId", tenantId.ToString()),
            new("realm", realm.ToString())
        };

        var principal = await _signInManager.CreateUserPrincipalAsync(user);
        ((ClaimsIdentity)principal.Identity!).AddClaims(additionalClaims);
        await _signInManager.Context.SignInAsync(IdentityConstants.ApplicationScheme, principal, new AuthenticationProperties { IsPersistent = isPersistent });
    }

    private async Task<IEnumerable<ModuleDto>> GetEntitledModulesAsync(string userId, Guid tenantId)
    {
        var modules = await _dbContext.UserEntitlements!
            .Include(ue => ue.Module)
            .Where(ue => ue.UserId == userId && ue.TenantId == tenantId)
            .Select(ue => ue.Module!)
            .Distinct()
            .ToListAsync();

        return modules.Select(m => new ModuleDto { Id = m.Id, Key = m.Key, Name = m.Name, Description = m.Description });
    }

    private async Task LogAuditSafe(string userId, bool success, string? reason, string? device = null, string? location = null)
    {
        try
        {
            await _auditRepository.LogAsync(new LoginAudit { UserId = userId, Success = success, FailureReason = reason, Device = device, Location = location });
        }
        catch
        {
            // ignore audit errors
        }
    }
}
