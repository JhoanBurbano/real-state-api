using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Million.Application.DTOs;
using Million.Application.Interfaces;
using Million.Domain.Entities;
using Million.Infrastructure.Persistence;

namespace Million.Infrastructure.Repositories;

public class PropertyRepository : IPropertyRepository
{
    private readonly IMongoCollection<Property> _collection;

    public PropertyRepository(MongoContext context)
    {
        _collection = context.GetCollection<Property>("properties");
    }

    public async Task<(IReadOnlyList<Property> Items, long Total)> FindAsync(PropertyListQuery query, CancellationToken cancellationToken)
    {
        var filters = new List<FilterDefinition<Property>>();
        var builder = Builders<Property>.Filter;

        bool hasName = !string.IsNullOrWhiteSpace(query.Name);
        bool hasAddress = !string.IsNullOrWhiteSpace(query.Address);
        if (hasName && hasAddress)
        {
            filters.Add(builder.Text((query.Name + " " + query.Address).Trim()));
        }
        else if (hasName)
        {
            filters.Add(builder.Regex(x => x.Name, new BsonRegularExpression(query.Name!, "i")));
        }
        else if (hasAddress)
        {
            filters.Add(builder.Regex(x => x.AddressProperty, new BsonRegularExpression(query.Address!, "i")));
        }

        if (query.MinPrice.HasValue)
            filters.Add(builder.Gte(x => x.PriceProperty, query.MinPrice.Value));
        if (query.MaxPrice.HasValue)
            filters.Add(builder.Lte(x => x.PriceProperty, query.MaxPrice.Value));

        var filter = filters.Count > 0 ? builder.And(filters) : builder.Empty;

        var find = _collection.Find(filter).Project<Property>(Builders<Property>.Projection.Exclude("_v"));

        var sort = ParseSort(query.Sort);
        find = find.Sort(sort);

        int skip = (query.Page - 1) * query.PageSize;
        var totalTask = _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var itemsTask = find.Skip(skip).Limit(query.PageSize).ToListAsync(cancellationToken);
        await Task.WhenAll(totalTask, itemsTask);
        return (itemsTask.Result, totalTask.Result);
    }

    public Task<Property?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var filter = Builders<Property>.Filter.Eq(x => x.Id, id);
        return _collection.Find(filter).FirstOrDefaultAsync(cancellationToken)!;
    }

    private static SortDefinition<Property> ParseSort(string? sort)
    {
        var builder = Builders<Property>.Sort;
        return (sort?.ToLowerInvariant()) switch
        {
            "price" => builder.Ascending(x => x.PriceProperty),
            "-price" or null or "" => builder.Descending(x => x.PriceProperty),
            "name" => builder.Ascending(x => x.Name),
            "-name" => builder.Descending(x => x.Name),
            _ => builder.Descending(x => x.PriceProperty)
        };
    }
}

