namespace BOSGlobal.Crm.Application.DTOs.Security;

public class OtpVerifyResponse
{
    public bool Success { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? VerifiedUtc { get; set; }
    public string? FailureReason { get; set; }
}
