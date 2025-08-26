using Million.Application.DTOs;
using Million.Domain.Entities;

namespace Million.Application.Interfaces;

public interface IPropertyTraceService
{
    Task<List<PropertyTraceDto>> GetPropertyTimelineAsync(string propertyId, CancellationToken ct = default);

    Task<PropertyTraceDto> LogPropertyCreationAsync(string propertyId, string propertyName, string? userId = null, CancellationToken ct = default);

    Task<PropertyTraceDto> LogPropertyUpdateAsync(string propertyId, string field, string? previousValue, string? newValue, string? userId = null, CancellationToken ct = default);

    Task<PropertyTraceDto> LogPriceChangeAsync(string propertyId, decimal previousPrice, decimal newPrice, string? userId = null, string? notes = null, CancellationToken ct = default);

    Task<PropertyTraceDto> LogStatusChangeAsync(string propertyId, string previousStatus, string newStatus, string? userId = null, string? notes = null, CancellationToken ct = default);

    Task<PropertyTraceDto> LogMediaUpdateAsync(string propertyId, string action, string? userId = null, string? notes = null, CancellationToken ct = default);

    Task<PropertyTraceDto> LogPropertySaleAsync(string propertyId, decimal salePrice, string? userId = null, string? notes = null, CancellationToken ct = default);

    Task<PropertyTraceDto> LogPropertyRentalAsync(string propertyId, decimal rentalPrice, string? userId = null, string? notes = null, CancellationToken ct = default);

    Task<List<PropertyTraceDto>> GetRecentActivityAsync(int limit = 20, CancellationToken ct = default);

    Task<List<PropertyTraceDto>> GetActivityByUserAsync(string userId, int limit = 50, CancellationToken ct = default);
}
