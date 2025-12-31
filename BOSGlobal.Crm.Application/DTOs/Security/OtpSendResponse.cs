namespace BOSGlobal.Crm.Application.DTOs.Security;

public class OtpSendResponse
{
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime ExpiresUtc { get; set; }
    public int CooldownSeconds { get; set; }
    public string? Status { get; set; }
}
