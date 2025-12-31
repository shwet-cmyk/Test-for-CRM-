namespace BOSGlobal.Crm.Application.Interfaces;

public interface IRecaptchaService
{
    /// <summary>
    /// Verify the provided reCAPTCHA token with the provider (Google). Returns true when verification succeeds or is skipped in dev when not configured.
    /// </summary>
    Task<bool> VerifyTokenAsync(string? token);
}
