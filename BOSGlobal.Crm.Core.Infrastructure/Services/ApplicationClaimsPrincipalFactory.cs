using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using BOSGlobal.Crm.Infrastructure.Identity;

namespace BOSGlobal.Crm.Infrastructure.Services;

/// <summary>
/// Include additional claims (session id) in the authentication principal based on ApplicationUser fields.
/// </summary>
public class ApplicationClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
{
    public ApplicationClaimsPrincipalFactory(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        if (!string.IsNullOrEmpty(user.SessionId))
        {
            identity.AddClaim(new Claim("sessionId", user.SessionId));
        }
        return identity;
    }
}
