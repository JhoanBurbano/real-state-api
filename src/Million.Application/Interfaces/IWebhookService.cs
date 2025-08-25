using Million.Application.DTOs;

namespace Million.Application.Interfaces;

public interface IWebhookService
{
    Task<WebhookResponse> ProcessPropertyUpdateAsync(WebhookRequest request, CancellationToken ct = default);

    Task<bool> ValidateWebhookSignatureAsync(string payload, string signature, string secret, CancellationToken ct = default);

    Task<List<string>> GetWebhookEndpointsAsync(string propertyId, CancellationToken ct = default);

    Task<bool> RegisterWebhookEndpointAsync(string propertyId, string endpoint, string secret, CancellationToken ct = default);

    Task<bool> UnregisterWebhookEndpointAsync(string propertyId, string endpoint, CancellationToken ct = default);

    Task<List<WebhookRequest>> GetWebhookHistoryAsync(string propertyId, int limit = 50, CancellationToken ct = default);

    Task<bool> RetryFailedWebhookAsync(string webhookId, CancellationToken ct = default);
}
