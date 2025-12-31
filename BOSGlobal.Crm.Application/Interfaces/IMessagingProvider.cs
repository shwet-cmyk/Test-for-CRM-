namespace BOSGlobal.Crm.Application.Interfaces;

public interface IMessagingProvider
{
    string ProviderName { get; }
    Task SendEmailAsync(string destination, string subject, string body, CancellationToken cancellationToken = default);
    Task SendSmsAsync(string destination, string body, CancellationToken cancellationToken = default);
    Task SendWhatsappAsync(string destination, string body, CancellationToken cancellationToken = default);
}
