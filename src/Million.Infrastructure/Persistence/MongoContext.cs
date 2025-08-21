using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Million.Infrastructure.Config;

namespace Million.Infrastructure.Persistence;

public class MongoContext
{
    private readonly IMongoDatabase _database;

    public MongoContext(IOptions<MongoOptions> options)
    {
        var client = new MongoClient(options.Value.Uri);
        _database = client.GetDatabase(options.Value.Database);
    }

    public IMongoCollection<T> GetCollection<T>(string name) => _database.GetCollection<T>(name);
}

