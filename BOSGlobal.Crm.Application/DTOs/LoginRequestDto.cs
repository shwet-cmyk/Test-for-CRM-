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
}
