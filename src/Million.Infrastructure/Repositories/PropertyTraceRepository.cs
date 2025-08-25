using Million.Application.DTOs;
using Million.Application.Interfaces;
using Million.Domain.Entities;
using Million.Infrastructure.Persistence;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Million.Infrastructure.Repositories;

public class PropertyTraceRepository : IPropertyTraceRepository
{
    private readonly IMongoCollection<PropertyTrace> _collection;

    public PropertyTraceRepository(MongoContext context)
    {
        _collection = context.Database.GetCollection<PropertyTrace>("property_traces");
    }

    public async Task<List<PropertyTraceDto>> GetPropertyTracesAsync(string propertyId, CancellationToken ct = default)
    {
        var filter = Builders<PropertyTrace>.Filter.Eq(x => x.PropertyId, propertyId);
        var traces = await _collection.Find(filter)
            .SortByDescending(x => x.Timestamp)
            .ToListAsync(ct);

        return traces.Select(MapToDto).ToList();
    }

    public async Task<List<PropertyTraceDto>> GetUserTracesAsync(string userId, CancellationToken ct = default)
    {
        var filter = Builders<PropertyTrace>.Filter.Eq(x => x.UserId, userId);
        var traces = await _collection.Find(filter)
            .SortByDescending(x => x.Timestamp)
            .ToListAsync(ct);

        return traces.Select(MapToDto).ToList();
    }

    public async Task<PropertyTraceDto?> GetTraceByIdAsync(string traceId, CancellationToken ct = default)
    {
        var filter = Builders<PropertyTrace>.Filter.Eq(x => x.Id, traceId);
        var trace = await _collection.Find(filter).FirstOrDefaultAsync(ct);

        return trace != null ? MapToDto(trace) : null;
    }

    public async Task<PropertyTraceDto> CreateTraceAsync(PropertyTrace trace, CancellationToken ct = default)
    {
        await _collection.InsertOneAsync(trace, cancellationToken: ct);
        return MapToDto(trace);
    }

    public async Task<List<PropertyTraceDto>> GetRecentTracesAsync(int limit = 50, CancellationToken ct = default)
    {
        var traces = await _collection.Find(FilterDefinition<PropertyTrace>.Empty)
            .SortByDescending(x => x.Timestamp)
            .Limit(limit)
            .ToListAsync(ct);

        return traces.Select(MapToDto).ToList();
    }

    public async Task<List<PropertyTraceDto>> GetTracesByActionAsync(string propertyId, string action, CancellationToken ct = default)
    {
        var filter = Builders<PropertyTrace>.Filter.And(
            Builders<PropertyTrace>.Filter.Eq(x => x.PropertyId, propertyId),
            Builders<PropertyTrace>.Filter.Eq(x => x.Action.ToString(), action)
        );

        var traces = await _collection.Find(filter)
            .SortByDescending(x => x.Timestamp)
            .ToListAsync(ct);

        return traces.Select(MapToDto).ToList();
    }

    public async Task<List<PropertyTraceDto>> GetTracesByDateRangeAsync(string propertyId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        var filter = Builders<PropertyTrace>.Filter.And(
            Builders<PropertyTrace>.Filter.Eq(x => x.PropertyId, propertyId),
            Builders<PropertyTrace>.Filter.Gte(x => x.Timestamp, from),
            Builders<PropertyTrace>.Filter.Lte(x => x.Timestamp, to)
        );

        var traces = await _collection.Find(filter)
            .SortByDescending(x => x.Timestamp)
            .ToListAsync(ct);

        return traces.Select(MapToDto).ToList();
    }

    public async Task<int> GetTraceCountAsync(string propertyId, CancellationToken ct = default)
    {
        var filter = Builders<PropertyTrace>.Filter.Eq(x => x.PropertyId, propertyId);
        return (int)await _collection.CountDocumentsAsync(filter, cancellationToken: ct);
    }

    public async Task<bool> DeleteTraceAsync(string traceId, CancellationToken ct = default)
    {
        var filter = Builders<PropertyTrace>.Filter.Eq(x => x.Id, traceId);
        var result = await _collection.DeleteOneAsync(filter, cancellationToken: ct);
        return result.DeletedCount > 0;
    }

    private static PropertyTraceDto MapToDto(PropertyTrace trace)
    {
        return new PropertyTraceDto
        {
            Id = trace.Id,
            PropertyId = trace.PropertyId,
            Action = trace.Action.ToString(),
            PreviousValue = trace.PreviousValue,
            NewValue = trace.NewValue,
            Timestamp = trace.Timestamp,
            UserId = trace.UserId,
            Notes = trace.Notes,
            PropertyName = trace.PropertyName,
            Price = trace.Price,
            Status = trace.Status
        };
    }
}
