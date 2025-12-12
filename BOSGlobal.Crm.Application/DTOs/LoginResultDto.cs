namespace BOSGlobal.Crm.Application.DTOs;

public class LoginResultDto
{
    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    // When true, the client should prompt the user for a 2FA code and call VerifyTwoFactorAsync
    public bool RequiresTwoFactor { get; set; }

    // Optional user id for follow-up steps
    public string? UserId { get; set; }
}
