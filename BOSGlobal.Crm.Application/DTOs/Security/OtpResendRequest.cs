using System.ComponentModel.DataAnnotations;

namespace BOSGlobal.Crm.Application.DTOs.Security;

public class OtpResendRequest
{
    [Required]
    public string CorrelationId { get; set; } = string.Empty;
    public string? RemoteIp { get; set; }
}
