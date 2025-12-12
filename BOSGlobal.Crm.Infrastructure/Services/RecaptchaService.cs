using BOSGlobal.Crm.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace BOSGlobal.Crm.Infrastructure.Services;

public class RecaptchaService : IRecaptchaService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string? _secret;

    public RecaptchaService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _secret = configuration["Recaptcha:SecretKey"];
    }

    public async Task<bool> VerifyTokenAsync(string? token)
    {
        // If no secret is configured, skip verification (convenience for local/dev)
        if (string.IsNullOrWhiteSpace(_secret))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var client = _httpClientFactory.CreateClient();
        var resp = await client.PostAsJsonAsync("https://www.google.com/recaptcha/api/siteverify", new Dictionary<string, string>
        {
            { "secret", _secret },
            { "response", token }
        });

        if (!resp.IsSuccessStatusCode)
        {
            return false;
        }

        var body = await resp.Content.ReadFromJsonAsync<RecaptchaResponse>();
        return body?.Success ?? false;
    }

    private class RecaptchaResponse
    {
        public bool Success { get; set; }
        public DateTime? Challenge_TS { get; set; }
        public string? Hostname { get; set; }
        public string[]? Error_Codes { get; set; }
    }
}
