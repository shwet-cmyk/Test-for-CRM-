namespace BOSGlobal.Crm.Application.Interfaces;

public interface IPhoneOtpService
{
    // Generate and send an OTP to the provided phone number. Returns the generated OTP in development mode.
    Task<string> SendOtpAsync(string phoneNumber);

    // Verify the OTP for the phone number. Returns true when valid.
    Task<bool> VerifyOtpAsync(string phoneNumber, string code);
}
