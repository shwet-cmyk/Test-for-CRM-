namespace BOSGlobal.Crm.Application.DTOs;

public class TwoFactorRequestDto
{
    public string Code { get; set; } = string.Empty;

    public bool RememberMe { get; set; }

    // Remember this client so 2FA isn't required on this browser/device
    public bool RememberClient { get; set; }
}
