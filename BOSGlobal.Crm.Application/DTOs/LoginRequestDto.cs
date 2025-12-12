using System.ComponentModel.DataAnnotations;

namespace BOSGlobal.Crm.Application.DTOs;

public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }

    // reCAPTCHA token provided by client (optional in dev)
    public string? RecaptchaToken { get; set; }
    // optional device information and location for audit logging
    public string? DeviceInfo { get; set; }
    public string? Location { get; set; }

    // User category for login (e.g. User, Partner, Employee)
    public string? SelectedRole { get; set; }

    // Optional list of modules the user wants to enter into for this session
    public List<string>? SelectedModules { get; set; }
}
