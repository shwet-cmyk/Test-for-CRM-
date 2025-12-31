using System.ComponentModel.DataAnnotations;
using BOSGlobal.Crm.Domain.Enums;

namespace BOSGlobal.Crm.Application.DTOs.Security;

public class OtpSendRequest
{
    public string? UserId { get; set; }
    [Required]
    public string Destination { get; set; } = string.Empty;
    [Required]
    public OtpChannel Channel { get; set; }
    [Required]
    public OtpPurpose Purpose { get; set; }
    public string? RemoteIp { get; set; }
}
