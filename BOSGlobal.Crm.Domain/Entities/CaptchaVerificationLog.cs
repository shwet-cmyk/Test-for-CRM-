using System.ComponentModel.DataAnnotations;

namespace BOSGlobal.Crm.Domain.Entities;

public class CaptchaVerificationLog
{
    public Guid Id { get; set; }
    public string? UserId { get; set; }
    [MaxLength(256)]
    public string TokenHash { get; set; } = string.Empty;
    [MaxLength(128)]
    public string? Action { get; set; }
    public decimal? Score { get; set; }
    public bool Success { get; set; }
    public DateTime CreatedUtc { get; set; }
    [MaxLength(64)]
    public string? RemoteIp { get; set; }
    [MaxLength(512)]
    public string? UserAgent { get; set; }
    public string? ProviderResponseJson { get; set; }
}
