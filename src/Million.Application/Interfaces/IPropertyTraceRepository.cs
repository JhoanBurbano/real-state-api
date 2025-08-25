using Million.Application.DTOs;
using Million.Domain.Entities;

namespace Million.Application.Interfaces;

public interface IPropertyTraceRepository
{
    Task<List<PropertyTraceDto>> GetPropertyTracesAsync(string propertyId, CancellationToken ct = default);

    Task<List<PropertyTraceDto>> GetUserTracesAsync(string userId, CancellationToken ct = default);

    Task<PropertyTraceDto?> GetTraceByIdAsync(string traceId, CancellationToken ct = default);

    Task<PropertyTraceDto> CreateTraceAsync(PropertyTrace trace, CancellationToken ct = default);

    Task<List<PropertyTraceDto>> GetRecentTracesAsync(int limit = 50, CancellationToken ct = default);

    Task<List<PropertyTraceDto>> GetTracesByActionAsync(string propertyId, string action, CancellationToken ct = default);

    Task<List<PropertyTraceDto>> GetTracesByDateRangeAsync(string propertyId, DateTime from, DateTime to, CancellationToken ct = default);

    Task<int> GetTraceCountAsync(string propertyId, CancellationToken ct = default);

    Task<bool> DeleteTraceAsync(string traceId, CancellationToken ct = default);
}
