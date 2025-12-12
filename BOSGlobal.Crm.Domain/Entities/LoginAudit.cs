using System;

namespace BOSGlobal.Crm.Domain.Entities;

public class LoginAudit
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Device { get; set; }
    public string? Location { get; set; }
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
}
