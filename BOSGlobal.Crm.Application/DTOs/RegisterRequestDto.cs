using System.ComponentModel.DataAnnotations;

namespace BOSGlobal.Crm.Application.DTOs;

public class RegisterRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string? ConfirmPassword { get; set; }

    // Optional phone number for OTP verification
    public string? PhoneNumber { get; set; }

    // Optional ERP identifier to link to external ERP system
    public string? ErpId { get; set; }

    // Optional GST number
    public string? GstNumber { get; set; }

    // reCAPTCHA token provided by client (optional in dev)
    public string? RecaptchaToken { get; set; }
}
