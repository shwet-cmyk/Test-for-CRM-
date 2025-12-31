using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using BOSGlobal.Crm.Infrastructure.Identity;

namespace BOSGlobal.Crm.Infrastructure.Services;

/// <summary>
/// Middleware that validates the session id claim on the authenticated principal
/// against the stored SessionId in ApplicationUser. If they differ, the user is signed out.
/// Also updates LastActivityUtc and sliding SessionExpiresUtc.
/// </summary>
public class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;

    public SessionValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var sessionClaim = context.User.FindFirst("sessionId")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    // invalid user - sign out
                    await signInManager.SignOutAsync();
                }
                else
                {
                    if (user.SessionExpiresUtc.HasValue && user.SessionExpiresUtc.Value <= DateTime.UtcNow)
                    {
                        await signInManager.SignOutAsync();
                        await _next(context);
                        return;
                    }

                    // if session ids mismatch -> sign out
                    if (!string.Equals(user.SessionId ?? string.Empty, sessionClaim ?? string.Empty, StringComparison.Ordinal))
                    {
                        await signInManager.SignOutAsync();
                    }
                    else
                    {
                        // update last activity and extend session expiry (sliding)
                        user.LastActivityUtc = DateTime.UtcNow;
                        user.SessionExpiresUtc = DateTime.UtcNow.AddMinutes(30);
                        await userManager.UpdateAsync(user);
                    }
                }
            }
        }

        await _next(context);
    }
}
