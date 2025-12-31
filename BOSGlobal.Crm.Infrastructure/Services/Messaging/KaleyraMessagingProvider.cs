using BOSGlobal.Crm.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace BOSGlobal.Crm.Infrastructure.Services.Messaging;

public class KaleyraMessagingProvider : IMessagingProvider
{
    private readonly ILogger<KaleyraMessagingProvider> _logger;

    public KaleyraMessagingProvider(ILogger<KaleyraMessagingProvider> logger)
    {
        _logger = logger;
    }

    public string ProviderName => "Kaleyra";

    public Task SendEmailAsync(string destination, string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Kaleyra email stub to {Destination} with subject {Subject}.", destination, subject);
        return Task.CompletedTask;
    }

    public Task SendSmsAsync(string destination, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Kaleyra SMS stub to {Destination}.", destination);
        return Task.CompletedTask;
    }

    public Task SendWhatsappAsync(string destination, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Kaleyra WhatsApp stub to {Destination}.", destination);
        return Task.CompletedTask;
    }
}
