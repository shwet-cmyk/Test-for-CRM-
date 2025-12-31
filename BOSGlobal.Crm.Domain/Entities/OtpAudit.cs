using System.ComponentModel.DataAnnotations;
using BOSGlobal.Crm.Domain.Enums;

namespace BOSGlobal.Crm.Domain.Entities;

public class OtpAudit
{
    public long Id { get; set; }
    public Guid OtpChallengeId { get; set; }
    [MaxLength(64)]
    public string EventType { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; }
    [MaxLength(256)]
    public string Destination { get; set; } = string.Empty;
    public OtpChannel Channel { get; set; }
    public OtpPurpose Purpose { get; set; }
    public bool Success { get; set; }
    [MaxLength(256)]
    public string? FailureReason { get; set; }
    [MaxLength(128)]
    public string? ProviderName { get; set; }
    [MaxLength(450)]
    public string? UserId { get; set; }
    [MaxLength(64)]
    public string? CorrelationId { get; set; }
    [MaxLength(64)]
    public string? RemoteIp { get; set; }
}
