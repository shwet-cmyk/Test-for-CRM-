using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using BOSGlobal.Crm.Application.DTOs.Security;
using BOSGlobal.Crm.Application.Exceptions;
using BOSGlobal.Crm.Application.Interfaces;
using BOSGlobal.Crm.Domain.Entities;
using BOSGlobal.Crm.Domain.Enums;
using BOSGlobal.Crm.Infrastructure.Identity;
using BOSGlobal.Crm.Infrastructure.Options;
using BOSGlobal.Crm.Infrastructure.Services.Messaging;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BOSGlobal.Crm.Infrastructure.Services;

public class SecurityGatewayService : ISecurityGatewayService
{
    private readonly CrmDbContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SecurityGatewayOptions _options;
    private readonly IProviderRouter _providerRouter;
    private readonly ILogger<SecurityGatewayService> _logger;

    public SecurityGatewayService(
        CrmDbContext dbContext,
        IHttpClientFactory httpClientFactory,
        IOptions<SecurityGatewayOptions> options,
        IProviderRouter providerRouter,
        ILogger<SecurityGatewayService> logger)
    {
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _providerRouter = providerRouter;
        _logger = logger;
    }

    public async Task<CaptchaVerifyResponse> VerifyCaptchaAsync(CaptchaVerifyRequest request, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString("N");
        var log = new CaptchaVerificationLog
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            TokenHash = HashValue(request.Token),
            Action = request.Action,
            CreatedUtc = DateTime.UtcNow,
            RemoteIp = request.RemoteIp,
            UserAgent = request.UserAgent
        };

        var responsePayload = new CaptchaVerifyResponse
        {
            CorrelationId = correlationId
        };

        if (!_options.Captcha.Enabled || string.IsNullOrWhiteSpace(_options.Captcha.SecretKey))
        {
            log.Success = true;
            log.ProviderResponseJson = "{\"skipped\":true}";
            responsePayload.Success = true;
            responsePayload.Reason = "Captcha disabled";
            await PersistCaptchaLogAsync(log, cancellationToken);
            return responsePayload;
        }

        var verifyResult = await VerifyCaptchaWithProviderAsync(request.Token, request.RemoteIp, cancellationToken);
        log.Score = verifyResult.Score;
        log.Success = verifyResult.Success;
        log.ProviderResponseJson = verifyResult.RawJson;

        responsePayload.Success = verifyResult.Success;
        responsePayload.Score = verifyResult.Score;
        responsePayload.Reason = verifyResult.Reason;

        await PersistCaptchaLogAsync(log, cancellationToken);
        return responsePayload;
    }

    public async Task<OtpSendResponse> SendOtpAsync(OtpSendRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        await EnforceSendRateLimitsAsync(request.Destination, request.Channel, request.Purpose, now, cancellationToken);

        var code = GenerateOtp(_options.Otp.DefaultLength);
        var salt = GenerateSalt();
        var hash = HashCode(code, salt);
        var correlationId = Guid.NewGuid().ToString("N");
        var expires = now.AddSeconds(_options.Otp.DefaultExpirySeconds);

        var challenge = new OtpChallenge
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Destination = request.Destination,
            Channel = request.Channel,
            Purpose = request.Purpose,
            CodeHash = hash,
            Salt = Convert.ToBase64String(salt),
            CreatedUtc = now,
            ExpiresUtc = expires,
            AttemptCount = 0,
            MaxAttempts = _options.Otp.MaxAttempts,
            LastSentUtc = now,
            SendCount = 1,
            Status = OtpStatus.Pending,
            CorrelationId = correlationId,
            ProviderName = _options.Messaging.DefaultProviderName
        };

        _dbContext.OtpChallenges!.Add(challenge);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var provider = _providerRouter.Resolve(challenge.ProviderName);
        try
        {
            await DispatchOtpAsync(provider, challenge.Channel, challenge.Destination, code, cancellationToken);
            await LogOtpAuditAsync(challenge, "Send", true, null, request.RemoteIp, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP for destination {Destination}", challenge.Destination);
            await LogOtpAuditAsync(challenge, "Send", false, ex.Message, request.RemoteIp, cancellationToken);
            throw new SecurityGatewayException("OTP delivery failed.", 500);
        }

        return new OtpSendResponse
        {
            CorrelationId = correlationId,
            ExpiresUtc = expires,
            CooldownSeconds = _options.Otp.CooldownSeconds,
            Status = challenge.Status.ToString()
        };
    }

    public async Task<OtpSendResponse> ResendOtpAsync(OtpResendRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var challenge = await _dbContext.OtpChallenges!.FirstOrDefaultAsync(c => c.CorrelationId == request.CorrelationId, cancellationToken)
            ?? throw new SecurityGatewayException("OTP challenge not found.", 404);

        await EnforceSendRateLimitsAsync(challenge.Destination, challenge.Channel, challenge.Purpose, now, cancellationToken);

        if (challenge.Status == OtpStatus.Verified)
        {
            throw new SecurityGatewayException("OTP already verified.", 400);
        }

        if (challenge.Status == OtpStatus.Locked)
        {
            throw new SecurityGatewayException("OTP locked due to too many attempts.", 400);
        }

        if (now < challenge.LastSentUtc.AddSeconds(_options.Otp.CooldownSeconds))
        {
            throw new SecurityGatewayException("OTP resend cooldown active.", 429);
        }

        var code = GenerateOtp(_options.Otp.DefaultLength);
        var salt = GenerateSalt();
        challenge.CodeHash = HashCode(code, salt);
        challenge.Salt = Convert.ToBase64String(salt);
        challenge.LastSentUtc = now;
        challenge.SendCount += 1;
        challenge.ExpiresUtc = now.AddSeconds(_options.Otp.DefaultExpirySeconds);
        challenge.Status = OtpStatus.Pending;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var provider = _providerRouter.Resolve(challenge.ProviderName);
        try
        {
            await DispatchOtpAsync(provider, challenge.Channel, challenge.Destination, code, cancellationToken);
            await LogOtpAuditAsync(challenge, "Resend", true, null, request.RemoteIp, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resend OTP for destination {Destination}", challenge.Destination);
            await LogOtpAuditAsync(challenge, "Resend", false, ex.Message, request.RemoteIp, cancellationToken);
            throw new SecurityGatewayException("OTP delivery failed.", 500);
        }

        return new OtpSendResponse
        {
            CorrelationId = challenge.CorrelationId,
            ExpiresUtc = challenge.ExpiresUtc,
            CooldownSeconds = _options.Otp.CooldownSeconds,
            Status = challenge.Status.ToString()
        };
    }

    public async Task<OtpVerifyResponse> VerifyOtpAsync(OtpVerifyRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var challenge = await _dbContext.OtpChallenges!.FirstOrDefaultAsync(c =>
            c.CorrelationId == request.CorrelationId &&
            c.Destination == request.Destination &&
            c.Channel == request.Channel &&
            c.Purpose == request.Purpose, cancellationToken);

        if (challenge == null)
        {
            return new OtpVerifyResponse { Success = false, Status = "NotFound", FailureReason = "OTP challenge not found." };
        }

        if (challenge.Status == OtpStatus.Verified)
        {
            return new OtpVerifyResponse { Success = true, Status = challenge.Status.ToString(), VerifiedUtc = challenge.VerifiedUtc };
        }

        if (challenge.ExpiresUtc <= now)
        {
            challenge.Status = OtpStatus.Expired;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await LogOtpAuditAsync(challenge, "Verify", false, "OTP expired", request.RemoteIp, cancellationToken);
            return new OtpVerifyResponse { Success = false, Status = challenge.Status.ToString(), FailureReason = "OTP expired." };
        }

        if (challenge.AttemptCount >= challenge.MaxAttempts)
        {
            challenge.Status = OtpStatus.Locked;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await LogOtpAuditAsync(challenge, "Verify", false, "Maximum attempts exceeded", request.RemoteIp, cancellationToken);
            return new OtpVerifyResponse { Success = false, Status = challenge.Status.ToString(), FailureReason = "Maximum attempts exceeded." };
        }

        var salt = Convert.FromBase64String(challenge.Salt);
        var incomingHash = HashCode(request.Code, salt);
        if (FixedTimeEquals(challenge.CodeHash, incomingHash))
        {
            challenge.Status = OtpStatus.Verified;
            challenge.VerifiedUtc = now;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await LogOtpAuditAsync(challenge, "Verify", true, null, request.RemoteIp, cancellationToken);

            return new OtpVerifyResponse
            {
                Success = true,
                Status = challenge.Status.ToString(),
                VerifiedUtc = challenge.VerifiedUtc
            };
        }

        challenge.AttemptCount += 1;
        if (challenge.AttemptCount >= challenge.MaxAttempts)
        {
            challenge.Status = OtpStatus.Locked;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await LogOtpAuditAsync(challenge, "Verify", false, "Invalid code", request.RemoteIp, cancellationToken);

        return new OtpVerifyResponse
        {
            Success = false,
            Status = challenge.Status.ToString(),
            FailureReason = challenge.Status == OtpStatus.Locked ? "Maximum attempts exceeded." : "Invalid code."
        };
    }

    private async Task PersistCaptchaLogAsync(CaptchaVerificationLog log, CancellationToken cancellationToken)
    {
        _dbContext.CaptchaVerificationLogs!.Add(log);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<(bool Success, decimal? Score, string? Reason, string RawJson)> VerifyCaptchaWithProviderAsync(string token, string? remoteIp, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return (false, null, "Missing token", "{}");
        }

        var client = _httpClientFactory.CreateClient(nameof(SecurityGatewayService));
        var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "secret", _options.Captcha.SecretKey },
                { "response", token },
                { "remoteip", remoteIp ?? string.Empty }
            }), cancellationToken);

        var rawJson = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return (false, null, "Captcha provider error", rawJson);
        }

        var payload = await response.Content.ReadFromJsonAsync<RecaptchaResponse>(cancellationToken: cancellationToken);
        if (payload == null)
        {
            return (false, null, "Captcha provider error", rawJson);
        }

        var success = payload.Success && (payload.Score ?? 0) >= _options.Captcha.MinimumScore;
        var reason = success ? null : "Captcha validation failed";
        return (success, (decimal?)payload.Score, reason, rawJson);
    }

    private async Task DispatchOtpAsync(IMessagingProvider provider, OtpChannel channel, string destination, string code, CancellationToken cancellationToken)
    {
        var body = $"Your verification code is {code}. It will expire in {_options.Otp.DefaultExpirySeconds / 60} minutes.";
        switch (channel)
        {
            case OtpChannel.Email:
                await provider.SendEmailAsync(destination, "Your verification code", body, cancellationToken);
                break;
            case OtpChannel.Sms:
                await provider.SendSmsAsync(destination, body, cancellationToken);
                break;
            case OtpChannel.WhatsApp:
                await provider.SendWhatsappAsync(destination, body, cancellationToken);
                break;
            default:
                throw new InvalidOperationException("Unsupported OTP channel.");
        }
    }

    private async Task EnforceSendRateLimitsAsync(string destination, OtpChannel channel, OtpPurpose purpose, DateTime now, CancellationToken cancellationToken)
    {
        var hourAgo = now.AddHours(-1);
        var dayAgo = now.AddDays(-1);
        var sendEvents = new[] { "Send", "Resend" };

        var hourCount = await _dbContext.OtpAudits!.CountAsync(a =>
            a.Destination == destination &&
            a.Channel == channel &&
            a.Purpose == purpose &&
            sendEvents.Contains(a.EventType) &&
            a.Success &&
            a.TimestampUtc >= hourAgo, cancellationToken);

        if (hourCount >= _options.Otp.MaxSendsPerHour)
        {
            throw new SecurityGatewayException("Hourly OTP send limit reached.", 429);
        }

        var dayCount = await _dbContext.OtpAudits!.CountAsync(a =>
            a.Destination == destination &&
            a.Channel == channel &&
            a.Purpose == purpose &&
            sendEvents.Contains(a.EventType) &&
            a.Success &&
            a.TimestampUtc >= dayAgo, cancellationToken);

        if (dayCount >= _options.Otp.MaxSendsPerDay)
        {
            throw new SecurityGatewayException("Daily OTP send limit reached.", 429);
        }

        var lastSend = await _dbContext.OtpAudits!
            .Where(a => a.Destination == destination && a.Channel == channel && a.Purpose == purpose && sendEvents.Contains(a.EventType))
            .OrderByDescending(a => a.TimestampUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastSend != null && now < lastSend.TimestampUtc.AddSeconds(_options.Otp.CooldownSeconds))
        {
            throw new SecurityGatewayException("OTP send cooldown active.", 429);
        }
    }

    private async Task LogOtpAuditAsync(OtpChallenge challenge, string eventType, bool success, string? reason, string? remoteIp, CancellationToken cancellationToken)
    {
        var audit = new OtpAudit
        {
            OtpChallengeId = challenge.Id,
            EventType = eventType,
            TimestampUtc = DateTime.UtcNow,
            Destination = challenge.Destination,
            Channel = challenge.Channel,
            Purpose = challenge.Purpose,
            Success = success,
            FailureReason = reason,
            ProviderName = challenge.ProviderName,
            UserId = challenge.UserId,
            CorrelationId = challenge.CorrelationId,
            RemoteIp = remoteIp
        };

        _dbContext.OtpAudits!.Add(audit);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string HashCode(string code, byte[] salt)
    {
        var hashed = KeyDerivation.Pbkdf2(
            password: code,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 32);
        return Convert.ToBase64String(hashed);
    }

    private static byte[] GenerateSalt()
    {
        var salt = new byte[16];
        RandomNumberGenerator.Fill(salt);
        return salt;
    }

    private static string GenerateOtp(int length)
    {
        var max = (int)Math.Pow(10, length);
        var number = RandomNumberGenerator.GetInt32(0, max);
        return number.ToString($"D{length}");
    }

    private static bool FixedTimeEquals(string a, string b)
    {
        var aBytes = Encoding.UTF8.GetBytes(a);
        var bBytes = Encoding.UTF8.GetBytes(b);
        return CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
    }

    private static string HashValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
        return Convert.ToBase64String(bytes);
    }

    private class RecaptchaResponse
    {
        public bool Success { get; set; }
        public double? Score { get; set; }
        public string? Action { get; set; }
        public string[]? ErrorCodes { get; set; }
    }
}
