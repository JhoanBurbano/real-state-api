namespace Million.Infrastructure.Config;

public class MongoOptions
{
    public const string SectionName = "Mongo";
    public string Uri { get; set; } = "mongodb://localhost:27017";
    public string Database { get; set; } = "million";
}

