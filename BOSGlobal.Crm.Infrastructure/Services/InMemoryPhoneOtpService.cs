using BOSGlobal.Crm.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace BOSGlobal.Crm.Infrastructure.Services;

public class InMemoryPhoneOtpService : IPhoneOtpService
{
    private readonly IMemoryCache _cache;

    public InMemoryPhoneOtpService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<string> SendOtpAsync(string phoneNumber)
    {
        // generate 6-digit code
        var rng = new Random();
        var code = rng.Next(100000, 999999).ToString();

        // store with 5 minute expiration
        _cache.Set(GetCacheKey(phoneNumber), code, TimeSpan.FromMinutes(5));

        // In production, hook an SMS provider here. For now, return the code so callers (tests/dev) can display it.
        Console.WriteLine($"[OTP] Sending OTP {code} to {phoneNumber}");
        return Task.FromResult(code);
    }

    public Task<bool> VerifyOtpAsync(string phoneNumber, string code)
    {
        if (_cache.TryGetValue(GetCacheKey(phoneNumber), out string? stored) && stored == code)
        {
            _cache.Remove(GetCacheKey(phoneNumber));
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    private static string GetCacheKey(string phone) => $"otp:{phone}";
}
