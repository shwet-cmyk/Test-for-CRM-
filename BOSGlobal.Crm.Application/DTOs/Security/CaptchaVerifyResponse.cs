namespace BOSGlobal.Crm.Application.DTOs.Security;

public class CaptchaVerifyResponse
{
    public bool Success { get; set; }
    public decimal? Score { get; set; }
    public string? Reason { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}
