using BOSGlobal.Crm.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace BOSGlobal.Crm.Infrastructure.Services.Messaging;

public class TwilioMessagingProvider : IMessagingProvider
{
    private readonly ILogger<TwilioMessagingProvider> _logger;

    public TwilioMessagingProvider(ILogger<TwilioMessagingProvider> logger)
    {
        _logger = logger;
    }

    public string ProviderName => "Twilio";

    public Task SendEmailAsync(string destination, string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Twilio email stub to {Destination} with subject {Subject}.", destination, subject);
        return Task.CompletedTask;
    }

    public Task SendSmsAsync(string destination, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Twilio SMS stub to {Destination}.", destination);
        return Task.CompletedTask;
    }

    public Task SendWhatsappAsync(string destination, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Twilio WhatsApp stub to {Destination}.", destination);
        return Task.CompletedTask;
    }
}
