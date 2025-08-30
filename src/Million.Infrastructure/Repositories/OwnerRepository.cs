using MongoDB.Driver;
using Million.Application.Interfaces;
using Million.Domain.Entities;
using Million.Infrastructure.Persistence;

namespace Million.Infrastructure.Repositories;

public class OwnerRepository : IOwnerRepository
{
    private readonly IMongoCollection<OwnerDocument> _collection;

    public OwnerRepository(MongoContext context)
    {
        _collection = context.Database.GetCollection<OwnerDocument>("owners");
    }

    public async Task<Owner?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var document = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
        return document?.ToEntity();
    }

    public async Task<Owner?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var document = await _collection.Find(x => x.Email == email.ToLowerInvariant()).FirstOrDefaultAsync(ct);
        return document?.ToEntity();
    }

    public async Task<Owner?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        // Get all owners and filter by slug in memory (MongoDB doesn't support complex string operations)
        var allOwners = await _collection.Find(_ => true).ToListAsync(ct);

        var owner = allOwners.FirstOrDefault(o =>
        {
            var normalizedName = o.FullName.ToLower().Replace(" ", "-");
            var normalizedSlug = slug.ToLower();
            return normalizedName == normalizedSlug;
        });

        return owner?.ToEntity();
    }

    public async Task<List<Owner>> GetAllAsync(CancellationToken ct = default)
    {
        var documents = await _collection.Find(_ => true).ToListAsync(ct);
        return documents.Select(d => d.ToEntity()).ToList();
    }

    public async Task<List<Owner>> FindAsync(string? query = null, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var filter = Builders<OwnerDocument>.Filter.Empty;

        if (!string.IsNullOrWhiteSpace(query))
        {
            var regex = new MongoDB.Bson.BsonRegularExpression(query, "i");
            filter = Builders<OwnerDocument>.Filter.Or(
                Builders<OwnerDocument>.Filter.Regex(x => x.FullName, regex),
                Builders<OwnerDocument>.Filter.Regex(x => x.Email, regex)
            );
        }

        var documents = await _collection.Find(filter)
            .Sort(Builders<OwnerDocument>.Sort.Ascending(x => x.FullName))
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        return documents.Select(d => d.ToEntity()).ToList();
    }

    public async Task<Owner> CreateAsync(Owner owner, CancellationToken ct = default)
    {
        var document = OwnerDocument.FromEntity(owner);
        await _collection.InsertOneAsync(document, cancellationToken: ct);
        return document.ToEntity();
    }

    public async Task<Owner> UpdateAsync(Owner owner, CancellationToken ct = default)
    {
        var document = OwnerDocument.FromEntity(owner);
        var result = await _collection.ReplaceOneAsync(x => x.Id == owner.Id, document, cancellationToken: ct);

        if (result.ModifiedCount == 0)
            throw new InvalidOperationException($"Owner with ID '{owner.Id}' not found");

        return document.ToEntity();
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var result = await _collection.DeleteOneAsync(x => x.Id == id, cancellationToken: ct);
        return result.DeletedCount > 0;
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        var count = await _collection.CountDocumentsAsync(x => x.Email == email.ToLowerInvariant(), cancellationToken: ct);
        return count > 0;
    }
}

