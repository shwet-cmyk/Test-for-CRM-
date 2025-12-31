using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace BOSGlobal.Crm.Maui.Services;

/// <summary>
/// Minimal authentication state provider for the MAUI host. It keeps the app running
/// even when no server-side authentication is available by presenting an anonymous user.
/// </summary>
public class AnonymousAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly AuthenticationState AnonymousState = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(AnonymousState);
    }
}
