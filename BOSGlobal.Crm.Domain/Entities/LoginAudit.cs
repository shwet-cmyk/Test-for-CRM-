using System.ComponentModel.DataAnnotations;
using System;

namespace BOSGlobal.Crm.Domain.Entities;

public class LoginAudit
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    [MaxLength(256)]
    public string? Device { get; set; }
    [MaxLength(256)]
    public string? Location { get; set; }
    public bool Success { get; set; }
    [MaxLength(256)]
    public string? FailureReason { get; set; }
}
