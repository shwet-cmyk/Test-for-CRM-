using System.ComponentModel.DataAnnotations;

namespace BOSGlobal.Crm.Application.DTOs.Security;

public class CaptchaVerifyRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;
    public string? Action { get; set; }
    public string? UserId { get; set; }
    public string? RemoteIp { get; set; }
    public string? UserAgent { get; set; }
}
