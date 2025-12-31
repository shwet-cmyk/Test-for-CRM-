namespace BOSGlobal.Crm.Infrastructure.Options;

public class SecurityGatewayOptions
{
    public CaptchaOptions Captcha { get; set; } = new();
    public OtpOptions Otp { get; set; } = new();
    public MessagingOptions Messaging { get; set; } = new();

    public class CaptchaOptions
    {
        public string Provider { get; set; } = "GoogleRecaptcha";
        public string SecretKey { get; set; } = string.Empty;
        public decimal MinimumScore { get; set; } = 0.5m;
        public bool Enabled { get; set; } = true;
    }

    public class OtpOptions
    {
        public int DefaultLength { get; set; } = 6;
        public int DefaultExpirySeconds { get; set; } = 300;
        public int CooldownSeconds { get; set; } = 30;
        public int MaxAttempts { get; set; } = 5;
        public int MaxSendsPerHour { get; set; } = 5;
        public int MaxSendsPerDay { get; set; } = 20;
    }

    public class MessagingOptions
    {
        public string DefaultProviderName { get; set; } = "Mock";
        public Dictionary<string, ProviderSettings> Providers { get; set; } = new();
    }

    public class ProviderSettings
    {
        public string? AccountSid { get; set; }
        public string? AuthToken { get; set; }
        public string? FromSms { get; set; }
        public string? FromWhatsapp { get; set; }
        public string? FromEmail { get; set; }
        public string? ApiKey { get; set; }
        public string? SenderId { get; set; }
    }
}
