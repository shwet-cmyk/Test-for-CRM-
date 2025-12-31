using BOSGlobal.Crm.Application.Interfaces;
using BOSGlobal.Crm.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BOSGlobal.Crm.Infrastructure.Services.Messaging;

public interface IProviderRouter
{
    IMessagingProvider Resolve(string? providerName = null);
}

public class ProviderRouter : IProviderRouter
{
    private readonly IDictionary<string, IMessagingProvider> _providers;
    private readonly SecurityGatewayOptions _options;
    private readonly ILogger<ProviderRouter> _logger;

    public ProviderRouter(IEnumerable<IMessagingProvider> providers, IOptions<SecurityGatewayOptions> options, ILogger<ProviderRouter> logger)
    {
        _providers = providers.ToDictionary(p => p.ProviderName, StringComparer.OrdinalIgnoreCase);
        _options = options.Value;
        _logger = logger;
    }

    public IMessagingProvider Resolve(string? providerName = null)
    {
        var nameToUse = string.IsNullOrWhiteSpace(providerName) ? _options.Messaging.DefaultProviderName : providerName;
        if (_providers.TryGetValue(nameToUse, out var provider))
        {
            return provider;
        }

        _logger.LogWarning("Requested messaging provider {ProviderName} not registered. Falling back to default provider {DefaultProvider}.", nameToUse, _options.Messaging.DefaultProviderName);
        if (_providers.TryGetValue(_options.Messaging.DefaultProviderName, out var defaultProvider))
        {
            return defaultProvider;
        }

        throw new InvalidOperationException("No messaging providers are registered.");
    }
}
