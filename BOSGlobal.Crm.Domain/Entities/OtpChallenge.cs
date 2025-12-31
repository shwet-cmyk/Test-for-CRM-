using System.ComponentModel.DataAnnotations;
using BOSGlobal.Crm.Domain.Enums;

namespace BOSGlobal.Crm.Domain.Entities;

public class OtpChallenge
{
    public Guid Id { get; set; }
    public string? UserId { get; set; }
    [MaxLength(256)]
    public string Destination { get; set; } = string.Empty;
    public OtpChannel Channel { get; set; }
    public OtpPurpose Purpose { get; set; }
    [MaxLength(256)]
    public string CodeHash { get; set; } = string.Empty;
    [MaxLength(128)]
    public string Salt { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
    public DateTime ExpiresUtc { get; set; }
    public DateTime? VerifiedUtc { get; set; }
    public int AttemptCount { get; set; }
    public int MaxAttempts { get; set; }
    public DateTime LastSentUtc { get; set; }
    public int SendCount { get; set; }
    public OtpStatus Status { get; set; }
    [MaxLength(64)]
    public string CorrelationId { get; set; } = string.Empty;
    [MaxLength(128)]
    public string? ProviderName { get; set; }
    public string? MetaJson { get; set; }
}
