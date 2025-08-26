using MongoDB.Driver;
using Million.Application.Interfaces;
using Million.Domain.Entities;
using Million.Infrastructure.Persistence;

namespace Million.Infrastructure.Repositories;

public class OwnerSessionRepository : IOwnerSessionRepository
{
    private readonly IMongoCollection<OwnerSessionDocument> _collection;

    public OwnerSessionRepository(MongoContext context)
    {
        _collection = context.Database.GetCollection<OwnerSessionDocument>("owner_sessions");
    }

    public async Task<OwnerSession?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var document = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
        return document?.ToEntity();
    }

    public async Task<OwnerSession?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken ct = default)
    {
        var document = await _collection.Find(x => x.RefreshTokenHash == refreshTokenHash).FirstOrDefaultAsync(ct);
        return document?.ToEntity();
    }

    public async Task<List<OwnerSession>> GetByOwnerIdAsync(string ownerId, CancellationToken ct = default)
    {
        var documents = await _collection.Find(x => x.OwnerId == ownerId)
            .Sort(Builders<OwnerSessionDocument>.Sort.Descending(x => x.IssuedAt))
            .ToListAsync(ct);

        return documents.Select(d => d.ToEntity()).ToList();
    }

    public async Task<OwnerSession> CreateAsync(OwnerSession session, CancellationToken ct = default)
    {
        var document = OwnerSessionDocument.FromEntity(session);
        await _collection.InsertOneAsync(document, cancellationToken: ct);
        return document.ToEntity();
    }

    public async Task<OwnerSession> UpdateAsync(OwnerSession session, CancellationToken ct = default)
    {
        var document = OwnerSessionDocument.FromEntity(session);
        var result = await _collection.ReplaceOneAsync(x => x.Id == session.Id, document, cancellationToken: ct);

        if (result.ModifiedCount == 0)
            throw new InvalidOperationException($"Session with ID '{session.Id}' not found");

        return document.ToEntity();
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var result = await _collection.DeleteOneAsync(x => x.Id == id, cancellationToken: ct);
        return result.DeletedCount > 0;
    }

    public async Task<bool> RevokeAsync(string id, CancellationToken ct = default)
    {
        var update = Builders<OwnerSessionDocument>.Update.Set(x => x.RevokedAt, DateTime.UtcNow);
        var result = await _collection.UpdateOneAsync(x => x.Id == id, update, cancellationToken: ct);
        return result.ModifiedCount > 0;
    }

    public async Task<int> CleanupExpiredSessionsAsync(CancellationToken ct = default)
    {
        var filter = Builders<OwnerSessionDocument>.Filter.Lt(x => x.ExpiresAt, DateTime.UtcNow);
        var result = await _collection.DeleteManyAsync(filter, cancellationToken: ct);
        return (int)result.DeletedCount;
    }
}

