using BOSGlobal.Crm.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace BOSGlobal.Crm.Infrastructure.Services.Messaging;

public class MockMessagingProvider : IMessagingProvider
{
    private readonly ILogger<MockMessagingProvider> _logger;

    public MockMessagingProvider(ILogger<MockMessagingProvider> logger)
    {
        _logger = logger;
    }

    public string ProviderName => "Mock";

    public Task SendEmailAsync(string destination, string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock email to {Destination} with subject {Subject}. Body: {Body}", destination, subject, body);
        return Task.CompletedTask;
    }

    public Task SendSmsAsync(string destination, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock SMS to {Destination}. Body: {Body}", destination, body);
        return Task.CompletedTask;
    }

    public Task SendWhatsappAsync(string destination, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock WhatsApp to {Destination}. Body: {Body}", destination, body);
        return Task.CompletedTask;
    }
}
