using MongoDB.Driver;
using Million.Domain.Entities;
using Million.Infrastructure.Persistence;

namespace Million.Infrastructure.Migrations;

public static class EnsureIndexes
{
    public static async Task RunAsync(MongoContext ctx, CancellationToken ct)
    {
        var coll = ctx.GetCollection<Property>("properties");
        // Text index on Name and AddressProperty
        var textKeys = Builders<Property>.IndexKeys.Text(x => x.Name).Text(x => x.AddressProperty);
        var textModel = new CreateIndexModel<Property>(textKeys, new CreateIndexOptions { Name = "idx_text_name_address" });
        await coll.Indexes.CreateOneAsync(textModel, cancellationToken: ct);

        // Numeric index on PriceProperty
        var priceIdx = Builders<Property>.IndexKeys.Ascending(x => x.PriceProperty);
        var priceModel = new CreateIndexModel<Property>(priceIdx, new CreateIndexOptions { Name = "idx_price" });
        await coll.Indexes.CreateOneAsync(priceModel, cancellationToken: ct);
    }
}

