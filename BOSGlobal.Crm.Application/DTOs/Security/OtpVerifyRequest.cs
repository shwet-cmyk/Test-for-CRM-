using System.ComponentModel.DataAnnotations;
using BOSGlobal.Crm.Domain.Enums;

namespace BOSGlobal.Crm.Application.DTOs.Security;

public class OtpVerifyRequest
{
    [Required]
    public string CorrelationId { get; set; } = string.Empty;
    [Required]
    public string Destination { get; set; } = string.Empty;
    [Required]
    public OtpChannel Channel { get; set; }
    [Required]
    public OtpPurpose Purpose { get; set; }
    [Required]
    public string Code { get; set; } = string.Empty;
    public string? RemoteIp { get; set; }
}
