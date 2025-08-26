using Million.Application.DTOs;
using Million.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Million.Application.Services;

public class WebhookService : IWebhookService
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IPropertyTraceService _traceService;

    public WebhookService(IPropertyRepository propertyRepository, IPropertyTraceService traceService)
    {
        _propertyRepository = propertyRepository;
        _traceService = traceService;
    }

    public async Task<WebhookResponse> ProcessPropertyUpdateAsync(WebhookRequest request, CancellationToken ct = default)
    {
        try
        {
            // Validate webhook request
            if (string.IsNullOrEmpty(request.PropertyId) || string.IsNullOrEmpty(request.Event))
            {
                return new WebhookResponse
                {
                    Success = false,
                    Message = "Invalid webhook request: missing required fields",
                    Error = "Missing PropertyId or Event",
                    ProcessedAt = DateTime.UtcNow
                };
            }

            // Verify property exists
            var property = await _propertyRepository.GetByIdAsync(request.PropertyId, ct);
            if (property == null)
            {
                return new WebhookResponse
                {
                    Success = false,
                    Message = "Property not found",
                    Error = $"Property with ID {request.PropertyId} not found",
                    ProcessedAt = DateTime.UtcNow
                };
            }

            // Process the webhook based on event type
            switch (request.Event.ToLowerInvariant())
            {
                case "property_created":
                    await ProcessPropertyCreated(request, property, ct);
                    break;
                case "property_updated":
                    await ProcessPropertyUpdated(request, property, ct);
                    break;
                case "property_sold":
                    await ProcessPropertySold(request, property, ct);
                    break;
                case "property_rented":
                    await ProcessPropertyRented(request, property, ct);
                    break;
                case "price_changed":
                    await ProcessPriceChanged(request, property, ct);
                    break;
                case "status_changed":
                    await ProcessStatusChanged(request, property, ct);
                    break;
                case "media_updated":
                    await ProcessMediaUpdated(request, property, ct);
                    break;
                default:
                    return new WebhookResponse
                    {
                        Success = false,
                        Message = "Unknown event type",
                        Error = $"Event type '{request.Event}' is not supported",
                        ProcessedAt = DateTime.UtcNow
                    };
            }

            return new WebhookResponse
            {
                Success = true,
                Message = $"Webhook processed successfully for event: {request.Event}",
                ProcessedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            return new WebhookResponse
            {
                Success = false,
                Message = "Webhook processing failed",
                Error = ex.Message,
                ProcessedAt = DateTime.UtcNow
            };
        }
    }

    public async Task<bool> ValidateWebhookSignatureAsync(string payload, string signature, string secret, CancellationToken ct = default)
    {
        try
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var computedSignature = Convert.ToBase64String(computedHash);

            return signature.Equals(computedSignature, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<string>> GetWebhookEndpointsAsync(string propertyId, CancellationToken ct = default)
    {
        // This would typically query a webhook registry
        // For now, return empty list
        return new List<string>();
    }

    public async Task<bool> RegisterWebhookEndpointAsync(string propertyId, string endpoint, string secret, CancellationToken ct = default)
    {
        // This would typically register the webhook endpoint
        // For now, return true
        return true;
    }

    public async Task<bool> UnregisterWebhookEndpointAsync(string propertyId, string endpoint, CancellationToken ct = default)
    {
        // This would typically unregister the webhook endpoint
        // For now, return true
        return true;
    }

    public async Task<List<WebhookRequest>> GetWebhookHistoryAsync(string propertyId, int limit = 50, CancellationToken ct = default)
    {
        // This would typically query webhook history
        // For now, return empty list
        return new List<WebhookRequest>();
    }

    public async Task<bool> RetryFailedWebhookAsync(string webhookId, CancellationToken ct = default)
    {
        // This would typically retry a failed webhook
        // For now, return true
        return true;
    }

    private async Task ProcessPropertyCreated(WebhookRequest request, Domain.Entities.Property property, CancellationToken ct)
    {
        // Log property creation
        await _traceService.LogPropertyCreationAsync(
            property.Id,
            property.Name,
            request.OwnerId,
            ct
        );
    }

    private async Task ProcessPropertyUpdated(WebhookRequest request, Domain.Entities.Property property, CancellationToken ct)
    {
        if (request.Changes?.Field != null)
        {
            await _traceService.LogPropertyUpdateAsync(
                property.Id,
                request.Changes.Field,
                request.Changes.PreviousValue,
                request.Changes.NewValue,
                request.OwnerId,
                ct
            );
        }
    }

    private async Task ProcessPropertySold(WebhookRequest request, Domain.Entities.Property property, CancellationToken ct)
    {
        if (decimal.TryParse(request.Changes?.NewValue?.Replace("$", "").Replace(",", ""), out var salePrice))
        {
            await _traceService.LogPropertySaleAsync(
                property.Id,
                salePrice,
                request.OwnerId,
                request.Changes?.Notes,
                ct
            );
        }
    }

    private async Task ProcessPropertyRented(WebhookRequest request, Domain.Entities.Property property, CancellationToken ct)
    {
        if (decimal.TryParse(request.Changes?.NewValue?.Replace("$", "").Replace(",", ""), out var rentalPrice))
        {
            await _traceService.LogPropertyRentalAsync(
                property.Id,
                rentalPrice,
                request.OwnerId,
                request.Changes?.Notes,
                ct
            );
        }
    }

    private async Task ProcessPriceChanged(WebhookRequest request, Domain.Entities.Property property, CancellationToken ct)
    {
        if (decimal.TryParse(request.Changes?.PreviousValue?.Replace("$", "").Replace(",", ""), out var previousPrice) &&
            decimal.TryParse(request.Changes?.NewValue?.Replace("$", "").Replace(",", ""), out var newPrice))
        {
            await _traceService.LogPriceChangeAsync(
                property.Id,
                previousPrice,
                newPrice,
                request.OwnerId,
                request.Changes?.Notes,
                ct
            );
        }
    }

    private async Task ProcessStatusChanged(WebhookRequest request, Domain.Entities.Property property, CancellationToken ct)
    {
        if (request.Changes?.PreviousValue != null && request.Changes?.NewValue != null)
        {
            await _traceService.LogStatusChangeAsync(
                property.Id,
                request.Changes.PreviousValue,
                request.Changes.NewValue,
                request.OwnerId,
                request.Changes?.Notes,
                ct
            );
        }
    }

    private async Task ProcessMediaUpdated(WebhookRequest request, Domain.Entities.Property property, CancellationToken ct)
    {
        await _traceService.LogMediaUpdateAsync(
            property.Id,
            "media",
            request.OwnerId,
            request.Changes?.Notes,
            ct
        );
    }
}
