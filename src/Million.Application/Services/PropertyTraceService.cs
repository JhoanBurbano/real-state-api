using Million.Application.DTOs;
using Million.Application.Interfaces;
using Million.Domain.Entities;

namespace Million.Application.Services;

public class PropertyTraceService : IPropertyTraceService
{
    private readonly IPropertyTraceRepository _traceRepository;

    public PropertyTraceService(IPropertyTraceRepository traceRepository)
    {
        _traceRepository = traceRepository;
    }

    public async Task<List<PropertyTraceDto>> GetPropertyTimelineAsync(string propertyId, CancellationToken ct = default)
    {
        var traces = await _traceRepository.GetPropertyTracesAsync(propertyId, ct);
        return traces.OrderByDescending(t => t.Timestamp).ToList();
    }

    public async Task<PropertyTraceDto> LogPropertyCreationAsync(string propertyId, string propertyName, string? userId = null, CancellationToken ct = default)
    {
        var trace = PropertyTrace.Create(
            propertyId,
            TraceAction.Created,
            null,
            propertyName,
            userId,
            "Property created"
        );

        return await _traceRepository.CreateTraceAsync(trace, ct);
    }

    public async Task<PropertyTraceDto> LogPropertyUpdateAsync(string propertyId, string field, string? previousValue, string? newValue, string? userId = null, CancellationToken ct = default)
    {
        var trace = PropertyTrace.Create(
            propertyId,
            TraceAction.Updated,
            previousValue,
            newValue,
            userId,
            $"Field '{field}' updated"
        );

        return await _traceRepository.CreateTraceAsync(trace, ct);
    }

    public async Task<PropertyTraceDto> LogPriceChangeAsync(string propertyId, decimal previousPrice, decimal newPrice, string? userId = null, string? notes = null, CancellationToken ct = default)
    {
        var trace = PropertyTrace.CreatePriceChange(propertyId, previousPrice, newPrice, userId, notes);
        return await _traceRepository.CreateTraceAsync(trace, ct);
    }

    public async Task<PropertyTraceDto> LogStatusChangeAsync(string propertyId, string previousStatus, string newStatus, string? userId = null, string? notes = null, CancellationToken ct = default)
    {
        var trace = PropertyTrace.CreateStatusChange(propertyId, previousStatus, newStatus, userId, notes);
        return await _traceRepository.CreateTraceAsync(trace, ct);
    }

    public async Task<PropertyTraceDto> LogMediaUpdateAsync(string propertyId, string action, string? userId = null, string? notes = null, CancellationToken ct = default)
    {
        var trace = PropertyTrace.CreateMediaUpdate(propertyId, action, userId, notes);
        return await _traceRepository.CreateTraceAsync(trace, ct);
    }

    public async Task<PropertyTraceDto> LogPropertySaleAsync(string propertyId, decimal salePrice, string? userId = null, string? notes = null, CancellationToken ct = default)
    {
        var trace = PropertyTrace.Create(
            propertyId,
            TraceAction.Sold,
            null,
            salePrice.ToString("C"),
            userId,
            notes ?? "Property sold"
        );

        return await _traceRepository.CreateTraceAsync(trace, ct);
    }

    public async Task<PropertyTraceDto> LogPropertyRentalAsync(string propertyId, decimal rentalPrice, string? userId = null, string? notes = null, CancellationToken ct = default)
    {
        var trace = PropertyTrace.Create(
            propertyId,
            TraceAction.Rented,
            null,
            rentalPrice.ToString("C"),
            userId,
            notes ?? "Property rented"
        );

        return await _traceRepository.CreateTraceAsync(trace, ct);
    }

    public async Task<List<PropertyTraceDto>> GetRecentActivityAsync(int limit = 20, CancellationToken ct = default)
    {
        var traces = await _traceRepository.GetRecentTracesAsync(limit, ct);
        return traces.OrderByDescending(t => t.Timestamp).ToList();
    }

    public async Task<List<PropertyTraceDto>> GetActivityByUserAsync(string userId, int limit = 50, CancellationToken ct = default)
    {
        var traces = await _traceRepository.GetUserTracesAsync(userId, ct);
        return traces.OrderByDescending(t => t.Timestamp).Take(limit).ToList();
    }
}
