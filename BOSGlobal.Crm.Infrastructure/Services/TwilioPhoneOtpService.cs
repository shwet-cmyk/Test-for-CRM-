using BOSGlobal.Crm.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Verify.V2.Service;

namespace BOSGlobal.Crm.Infrastructure.Services;

/// <summary>
/// Twilio-backed implementation of <see cref="IPhoneOtpService"/> using Twilio Verify service.
/// Configuration keys (use environment variables in production):
///   Twilio:AccountSid
///   Twilio:AuthToken
///   Twilio:VerifyServiceSid
/// This implementation delegates verification to Twilio Verify. No secrets are committed here.
/// </summary>
public class TwilioPhoneOtpService : IPhoneOtpService
{
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _serviceSid;

    public TwilioPhoneOtpService(IConfiguration config)
    {
        _accountSid = config["Twilio:AccountSid"] ?? string.Empty;
        _authToken = config["Twilio:AuthToken"] ?? string.Empty;
        _serviceSid = config["Twilio:VerifyServiceSid"] ?? string.Empty;
        if (!string.IsNullOrEmpty(_accountSid) && !string.IsNullOrEmpty(_authToken))
        {
            TwilioClient.Init(_accountSid, _authToken);
        }
    }

    public async Task<string> SendOtpAsync(string phoneNumber)
    {
        if (string.IsNullOrEmpty(_serviceSid))
        {
            throw new InvalidOperationException("Twilio Verify Service SID not configured.");
        }

        var verification = await VerificationResource.CreateAsync(to: phoneNumber, channel: "sms", pathServiceSid: _serviceSid);

        // Return the verification SID for correlation; in production you typically don't return the code.
        return verification?.Sid ?? string.Empty;
    }

    public async Task<bool> VerifyOtpAsync(string phoneNumber, string code)
    {
        if (string.IsNullOrEmpty(_serviceSid))
        {
            throw new InvalidOperationException("Twilio Verify Service SID not configured.");
        }

        var check = await VerificationCheckResource.CreateAsync(to: phoneNumber, code: code, pathServiceSid: _serviceSid);
        return string.Equals(check.Status, "approved", StringComparison.OrdinalIgnoreCase);
    }
}
