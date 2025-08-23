using MongoDB.Bson;
using MongoDB.Driver;
using Million.Application.DTOs;
using Million.Application.Interfaces;
using Million.Domain.Entities;
using Million.Infrastructure.Persistence;

namespace Million.Infrastructure.Repositories;

public class PropertyRepository : IPropertyRepository
{
    private readonly IMongoCollection<PropertyDocument> _collection;
    private readonly int _featuredMediaLimit = 12; // FEATURED_MEDIA_LIMIT
    private readonly int _mediaLibraryLimit = 60; // MEDIA_LIBRARY_LIMIT

    public PropertyRepository(MongoContext context)
    {
        _collection = context.GetCollection<PropertyDocument>("properties");
    }

    public async Task<(IReadOnlyList<PropertyListDto> Items, long Total)> FindAsync(PropertyListQuery query, CancellationToken cancellationToken)
    {
        var filters = new List<FilterDefinition<PropertyDocument>>();
        var builder = Builders<PropertyDocument>.Filter;

        // Basic filters
        if (query.MinPrice.HasValue)
            filters.Add(builder.Gte(x => x.Price, query.MinPrice.Value));

        if (query.MaxPrice.HasValue)
            filters.Add(builder.Lte(x => x.Price, query.MaxPrice.Value));

        // Advanced filters
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchFilter = builder.Or(
                builder.Regex(x => x.Name, new BsonRegularExpression(query.Search, "i")),
                builder.Regex(x => x.Description, new BsonRegularExpression(query.Search, "i")),
                builder.Regex(x => x.Address, new BsonRegularExpression(query.Search, "i")),
                builder.Regex(x => x.City, new BsonRegularExpression(query.Search, "i")),
                builder.Regex(x => x.Neighborhood, new BsonRegularExpression(query.Search, "i"))
            );
            filters.Add(searchFilter);
        }

        if (!string.IsNullOrWhiteSpace(query.Location))
        {
            var locationFilter = builder.Or(
                builder.Regex(x => x.City, new BsonRegularExpression(query.Location, "i")),
                builder.Regex(x => x.Neighborhood, new BsonRegularExpression(query.Location, "i"))
            );
            filters.Add(locationFilter);
        }

        if (!string.IsNullOrWhiteSpace(query.PropertyType))
            filters.Add(builder.Regex(x => x.PropertyType, new BsonRegularExpression(query.PropertyType, "i")));

        if (query.Bedrooms.HasValue)
            filters.Add(builder.Eq(x => x.Bedrooms, query.Bedrooms.Value));

        if (query.Bathrooms.HasValue)
            filters.Add(builder.Eq(x => x.Bathrooms, query.Bathrooms.Value));

        if (query.MinSize.HasValue)
            filters.Add(builder.Gte(x => x.Size, query.MinSize.Value));

        if (query.MaxSize.HasValue)
            filters.Add(builder.Lte(x => x.Size, query.MaxSize.Value));

        if (query.HasPool.HasValue)
            filters.Add(builder.Eq(x => x.HasPool, query.HasPool.Value));

        if (query.HasGarden.HasValue)
            filters.Add(builder.Eq(x => x.HasGarden, query.HasPool.Value));

        if (query.HasParking.HasValue)
            filters.Add(builder.Eq(x => x.HasParking, query.HasParking.Value));

        if (query.IsFurnished.HasValue)
            filters.Add(builder.Eq(x => x.IsFurnished, query.IsFurnished.Value));

        if (!string.IsNullOrWhiteSpace(query.IdOwner))
            filters.Add(builder.Eq(x => x.OwnerId, query.IdOwner));

        if (query.AvailableFrom.HasValue)
            filters.Add(builder.Lte(x => x.AvailableFrom, query.AvailableFrom.Value));

        if (query.AvailableTo.HasValue)
            filters.Add(builder.Gte(x => x.AvailableTo, query.AvailableTo.Value));

        // Only show active properties by default
        filters.Add(builder.Eq(x => x.IsActive, true));

        var filter = filters.Count > 0 ? builder.And(filters) : builder.Empty;

        // Use aggregation pipeline for optimized listing
        var pipeline = new List<BsonDocument>
        {
            new BsonDocument("$match", filter.ToBsonDocument()),
            new BsonDocument("$project", new BsonDocument
            {
                { "name", 1 },
                { "address", 1 },
                { "price", 1 },
                { "year", 1 },
                { "codeInternal", 1 },
                { "ownerId", 1 },
                { "status", 1 },
                { "coverUrl", new BsonDocument("$ifNull", new BsonArray { "$cover.url", "$cover.poster" }) },
                { "totalImages", new BsonDocument("$size", new BsonDocument("$filter", new BsonDocument
                {
                    { "input", "$media" },
                    { "as", "m" },
                    { "cond", new BsonDocument("$eq", new BsonArray { "$$m.type", 0 }) } // MediaType.Image = 0
                })) },
                { "totalVideos", new BsonDocument("$size", new BsonDocument("$filter", new BsonDocument
                {
                    { "input", "$media" },
                    { "as", "m" },
                    { "cond", new BsonDocument("$eq", new BsonArray { "$$m.type", 1 }) } // MediaType.Video = 1
                })) }
            }),
            new BsonDocument("$addFields", new BsonDocument
            {
                { "hasMoreMedia", new BsonDocument("$gt", new BsonArray { "$totalImages", _featuredMediaLimit }) }
            }),
            new BsonDocument("$sort", ParseSortBson(query.Sort)),
            new BsonDocument("$facet", new BsonDocument
            {
                { "items", new BsonArray
                {
                    new BsonDocument("$skip", (query.Page - 1) * query.PageSize),
                    new BsonDocument("$limit", query.PageSize)
                }},
                { "total", new BsonArray { new BsonDocument("$count", "count") } }
            })
        };

        var result = await _collection.Aggregate<BsonDocument>(pipeline).ToListAsync(cancellationToken);

        var items = result.FirstOrDefault()?.GetValue("items").AsBsonArray
            .Select(item => new PropertyListDto
            {
                Id = item["_id"].AsString,
                Name = item["name"].AsString,
                Address = item["address"].AsString,
                Price = item["price"].AsDecimal,
                Year = item["year"].AsInt32,
                CodeInternal = item["codeInternal"].AsString,
                OwnerId = item["ownerId"].AsString,
                Status = item["status"].AsInt32.ToString(),
                CoverUrl = item["coverUrl"].AsString,
                HasMoreMedia = item["hasMoreMedia"].AsBoolean,
                TotalImages = item["totalImages"].AsInt32,
                TotalVideos = item["totalVideos"].AsInt32
            }).ToList() ?? new List<PropertyListDto>();

        var total = result.FirstOrDefault()?.GetValue("total").AsBsonArray.FirstOrDefault()?.AsBsonDocument.GetValue("count").AsInt64 ?? 0;

        return (items, total);
    }

    public async Task<Property?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var filter = Builders<PropertyDocument>.Filter.Eq(x => x.Id, id);
        var document = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        return document?.ToEntity();
    }

    public async Task<Property> CreateAsync(Property property, CancellationToken cancellationToken)
    {
        var document = PropertyDocument.FromEntity(property);
        await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
        return document.ToEntity();
    }

    public async Task<Property> UpdateAsync(string id, Property property, CancellationToken cancellationToken)
    {
        var document = PropertyDocument.FromEntity(property);
        var filter = Builders<PropertyDocument>.Filter.Eq(x => x.Id, id);

        var update = Builders<PropertyDocument>.Update
            .Set(x => x.Name, document.Name)
            .Set(x => x.Description, document.Description)
            .Set(x => x.Address, document.Address)
            .Set(x => x.City, document.City)
            .Set(x => x.Neighborhood, document.Neighborhood)
            .Set(x => x.PropertyType, document.PropertyType)
            .Set(x => x.Price, document.Price)
            .Set(x => x.Size, document.Size)
            .Set(x => x.Bedrooms, document.Bedrooms)
            .Set(x => x.Bathrooms, document.Bathrooms)
            .Set(x => x.HasPool, document.HasPool)
            .Set(x => x.HasGarden, document.HasGarden)
            .Set(x => x.HasParking, document.HasParking)
            .Set(x => x.IsFurnished, document.IsFurnished)
            .Set(x => x.AvailableFrom, document.AvailableFrom)
            .Set(x => x.AvailableTo, document.AvailableTo)
            .Set(x => x.UpdatedAt, document.UpdatedAt);

        var options = new FindOneAndUpdateOptions<PropertyDocument> { ReturnDocument = ReturnDocument.After };
        var updatedDocument = await _collection.FindOneAndUpdateAsync(filter, update, options, cancellationToken);
        return updatedDocument?.ToEntity() ?? throw new InvalidOperationException($"Failed to update property {id}");
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var filter = Builders<PropertyDocument>.Filter.Eq(x => x.Id, id);
        var result = await _collection.DeleteOneAsync(filter, cancellationToken);
        return result.DeletedCount > 0;
    }

    public async Task<bool> ActivateAsync(string id, CancellationToken cancellationToken)
    {
        var filter = Builders<PropertyDocument>.Filter.Eq(x => x.Id, id);
        var update = Builders<PropertyDocument>.Update
            .Set(x => x.IsActive, true)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeactivateAsync(string id, CancellationToken cancellationToken)
    {
        var filter = Builders<PropertyDocument>.Filter.Eq(x => x.Id, id);
        var update = Builders<PropertyDocument>.Update
            .Set(x => x.IsActive, false)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }

    public async Task<List<Media>> GetMediaAsync(string propertyId, MediaQueryDto query, CancellationToken cancellationToken)
    {
        var filter = Builders<PropertyDocument>.Filter.Eq(x => x.Id, propertyId);
        var document = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);

        if (document == null) return new List<Media>();

        var property = document.ToEntity();
        var mediaQuery = property.Media.Where(m => m.Enabled);

        if (!string.IsNullOrEmpty(query.Type))
        {
            var mediaType = query.Type.ToLowerInvariant() == "video" ? MediaType.Video : MediaType.Image;
            mediaQuery = mediaQuery.Where(m => m.Type == mediaType);
        }

        if (query.Featured.HasValue)
        {
            mediaQuery = mediaQuery.Where(m => m.Featured == query.Featured.Value);
        }

        return mediaQuery
            .OrderBy(m => m.Index)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();
    }

    public async Task<bool> UpdateMediaAsync(string propertyId, MediaPatchDto mediaPatch, CancellationToken cancellationToken)
    {
        var filter = Builders<PropertyDocument>.Filter.Eq(x => x.Id, propertyId);
        var updates = new List<UpdateDefinition<PropertyDocument>>();

        if (mediaPatch.Index.HasValue)
            updates.Add(Builders<PropertyDocument>.Update.Set("media.$[elem].index", mediaPatch.Index.Value));
        if (mediaPatch.Featured.HasValue)
            updates.Add(Builders<PropertyDocument>.Update.Set("media.$[elem].featured", mediaPatch.Featured.Value));
        if (mediaPatch.Enabled.HasValue)
            updates.Add(Builders<PropertyDocument>.Update.Set("media.$[elem].enabled", mediaPatch.Enabled.Value));
        if (!string.IsNullOrEmpty(mediaPatch.Url))
            updates.Add(Builders<PropertyDocument>.Update.Set("media.$[elem].url", mediaPatch.Url));
        if (!string.IsNullOrEmpty(mediaPatch.Poster))
            updates.Add(Builders<PropertyDocument>.Update.Set("media.$[elem].poster", mediaPatch.Poster));
        if (mediaPatch.Variants != null)
            updates.Add(Builders<PropertyDocument>.Update.Set("media.$[elem].variants", mediaPatch.Variants));

        updates.Add(Builders<PropertyDocument>.Update.Set(x => x.UpdatedAt, DateTime.UtcNow));

        var combinedUpdate = Builders<PropertyDocument>.Update.Combine(updates);

        var arrayFilters = new List<ArrayFilterDefinition>
        {
            new BsonDocumentArrayFilterDefinition<BsonValue>(new BsonDocument("elem._id", mediaPatch.MediaId))
        };

        var options = new UpdateOptions { ArrayFilters = arrayFilters };
        var result = await _collection.UpdateOneAsync(filter, combinedUpdate, options, cancellationToken);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> AddTraceAsync(string propertyId, PropertyTrace trace, CancellationToken cancellationToken)
    {
        var filter = Builders<PropertyDocument>.Filter.Eq(x => x.Id, propertyId);
        var update = Builders<PropertyDocument>.Update
            .Push(x => x.Traces, trace)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }

    private static BsonDocument ParseSortBson(string? sort)
    {
        return (sort?.ToLowerInvariant()) switch
        {
            "price" => new BsonDocument("price", 1),
            "-price" or null or "" => new BsonDocument("price", -1),
            "name" => new BsonDocument("name", 1),
            "-name" => new BsonDocument("name", -1),
            "date" => new BsonDocument("createdAt", 1),
            "-date" => new BsonDocument("createdAt", -1),
            "size" => new BsonDocument("size", 1),
            "-size" => new BsonDocument("size", -1),
            "bedrooms" => new BsonDocument("bedrooms", 1),
            "-bedrooms" => new BsonDocument("bedrooms", -1),
            "bathrooms" => new BsonDocument("bathrooms", 1),
            "-bathrooms" => new BsonDocument("bathrooms", -1),
            _ => new BsonDocument("price", -1)
        };
    }
}

