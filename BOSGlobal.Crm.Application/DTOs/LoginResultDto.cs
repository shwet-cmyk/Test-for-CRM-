namespace BOSGlobal.Crm.Application.DTOs;

public class LoginResultDto
{
    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    // When true, the client should prompt the user for a 2FA code and call VerifyTwoFactorAsync
    public bool RequiresTwoFactor { get; set; }

    // Optional user id for follow-up steps
    public string? UserId { get; set; }
    // Roles assigned to the user
    public IEnumerable<string>? Roles { get; set; }

    // Optional redirect URL for role-specific dashboard
    public string? RedirectUrl { get; set; }
    // Optional machine-readable error code to guide client behavior
    public string? ErrorCode { get; set; }
}
