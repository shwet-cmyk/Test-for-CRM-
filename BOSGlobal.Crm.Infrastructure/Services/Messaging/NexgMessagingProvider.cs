using BOSGlobal.Crm.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace BOSGlobal.Crm.Infrastructure.Services.Messaging;

public class NexgMessagingProvider : IMessagingProvider
{
    private readonly ILogger<NexgMessagingProvider> _logger;

    public NexgMessagingProvider(ILogger<NexgMessagingProvider> logger)
    {
        _logger = logger;
    }

    public string ProviderName => "Nexg";

    public Task SendEmailAsync(string destination, string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Nexg email stub to {Destination} with subject {Subject}.", destination, subject);
        return Task.CompletedTask;
    }

    public Task SendSmsAsync(string destination, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Nexg SMS stub to {Destination}.", destination);
        return Task.CompletedTask;
    }

    public Task SendWhatsappAsync(string destination, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Nexg WhatsApp stub to {Destination}.", destination);
        return Task.CompletedTask;
    }
}
