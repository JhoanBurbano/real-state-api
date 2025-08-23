using MongoDB.Driver;
using Million.Infrastructure.Persistence;

namespace Million.Infrastructure.Migrations;

public static class EnsureIndexes
{
    public static async Task RunAsync(MongoContext context, CancellationToken cancellationToken = default)
    {
        await EnsurePropertyIndexesAsync(context, cancellationToken);
        await EnsureOwnerIndexesAsync(context, cancellationToken);
        await EnsureOwnerSessionIndexesAsync(context, cancellationToken);
    }

    private static async Task EnsurePropertyIndexesAsync(MongoContext context, CancellationToken cancellationToken)
    {
        var collection = context.Database.GetCollection<PropertyDocument>("properties");

        // Text search index
        var textIndexModel = new CreateIndexModel<PropertyDocument>(
            Builders<PropertyDocument>.IndexKeys.Text(x => x.Name).Text(x => x.Address),
            new CreateIndexOptions { Name = "text_search" }
        );

        // Price index for range queries
        var priceIndexModel = new CreateIndexModel<PropertyDocument>(
            Builders<PropertyDocument>.IndexKeys.Ascending(x => x.Price),
            new CreateIndexOptions { Name = "price_asc" }
        );

        // Owner ID index
        var ownerIndexModel = new CreateIndexModel<PropertyDocument>(
            Builders<PropertyDocument>.IndexKeys.Ascending(x => x.OwnerId),
            new CreateIndexOptions { Name = "ownerId_asc" }
        );

        // Code internal unique index
        var codeInternalIndexModel = new CreateIndexModel<PropertyDocument>(
            Builders<PropertyDocument>.IndexKeys.Ascending(x => x.CodeInternal),
            new CreateIndexOptions { Name = "codeInternal_unique", Unique = true }
        );

        // Status index
        var statusIndexModel = new CreateIndexModel<PropertyDocument>(
            Builders<PropertyDocument>.IndexKeys.Ascending(x => x.Status),
            new CreateIndexOptions { Name = "status_asc" }
        );

        // Featured media sparse index
        var featuredMediaIndexModel = new CreateIndexModel<PropertyDocument>(
            Builders<PropertyDocument>.IndexKeys.Ascending("media.featured"),
            new CreateIndexOptions { Name = "media_featured", Sparse = true }
        );

        // Active properties index
        var activeIndexModel = new CreateIndexModel<PropertyDocument>(
            Builders<PropertyDocument>.IndexKeys.Ascending(x => x.IsActive),
            new CreateIndexOptions { Name = "isActive_asc" }
        );

        var indexes = new[]
        {
            textIndexModel,
            priceIndexModel,
            ownerIndexModel,
            codeInternalIndexModel,
            statusIndexModel,
            featuredMediaIndexModel,
            activeIndexModel
        };

        await collection.Indexes.CreateManyAsync(indexes, cancellationToken: cancellationToken);
    }

    private static async Task EnsureOwnerIndexesAsync(MongoContext context, CancellationToken cancellationToken)
    {
        var collection = context.Database.GetCollection<OwnerDocument>("owners");

        // Email unique index
        var emailIndexModel = new CreateIndexModel<OwnerDocument>(
            Builders<OwnerDocument>.IndexKeys.Ascending(x => x.Email),
            new CreateIndexOptions { Name = "email_unique", Unique = true }
        );

        // Role index
        var roleIndexModel = new CreateIndexModel<OwnerDocument>(
            Builders<OwnerDocument>.IndexKeys.Ascending(x => x.Role),
            new CreateIndexOptions { Name = "role_asc" }
        );

        // Active owners index
        var activeIndexModel = new CreateIndexModel<OwnerDocument>(
            Builders<OwnerDocument>.IndexKeys.Ascending(x => x.IsActive),
            new CreateIndexOptions { Name = "isActive_asc" }
        );

        // Full name index for search
        var fullNameIndexModel = new CreateIndexModel<OwnerDocument>(
            Builders<OwnerDocument>.IndexKeys.Ascending(x => x.FullName),
            new CreateIndexOptions { Name = "fullName_asc" }
        );

        var indexes = new[]
        {
            emailIndexModel,
            roleIndexModel,
            activeIndexModel,
            fullNameIndexModel
        };

        await collection.Indexes.CreateManyAsync(indexes, cancellationToken: cancellationToken);
    }

    private static async Task EnsureOwnerSessionIndexesAsync(MongoContext context, CancellationToken cancellationToken)
    {
        var collection = context.Database.GetCollection<OwnerSessionDocument>("owner_sessions");

        // Owner ID index
        var ownerIdIndexModel = new CreateIndexModel<OwnerSessionDocument>(
            Builders<OwnerSessionDocument>.IndexKeys.Ascending(x => x.OwnerId),
            new CreateIndexOptions { Name = "ownerId_asc" }
        );

        // Expires at index with TTL
        var expiresAtIndexModel = new CreateIndexModel<OwnerSessionDocument>(
            Builders<OwnerSessionDocument>.IndexKeys.Ascending(x => x.ExpiresAt),
            new CreateIndexOptions
            {
                Name = "expiresAt_ttl",
                ExpireAfter = TimeSpan.Zero // TTL will be managed by application logic
            }
        );

        // Refresh token hash index
        var refreshTokenIndexModel = new CreateIndexModel<OwnerSessionDocument>(
            Builders<OwnerSessionDocument>.IndexKeys.Ascending(x => x.RefreshTokenHash),
            new CreateIndexOptions { Name = "refreshTokenHash_asc" }
        );

        // Issued at index for sorting
        var issuedAtIndexModel = new CreateIndexModel<OwnerSessionDocument>(
            Builders<OwnerSessionDocument>.IndexKeys.Descending(x => x.IssuedAt),
            new CreateIndexOptions { Name = "issuedAt_desc" }
        );

        var indexes = new[]
        {
            ownerIdIndexModel,
            expiresAtIndexModel,
            refreshTokenIndexModel,
            issuedAtIndexModel
        };

        await collection.Indexes.CreateManyAsync(indexes, cancellationToken: cancellationToken);
    }
}

