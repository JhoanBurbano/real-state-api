using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Million.Domain.Entities;

namespace Million.Infrastructure.Persistence;

[BsonIgnoreExtraElements]
public class CoverDocument
{
    [BsonElement("Type")]
    [BsonRepresentation(BsonType.Int32)]
    public MediaType Type { get; set; } = MediaType.Image;

    [BsonElement("Url")]
    public string Url { get; set; } = string.Empty;

    [BsonElement("Index")]
    public int Index { get; set; } = 0;

    [BsonElement("Poster")]
    public string? Poster { get; set; } // For video covers

    public static CoverDocument FromEntity(Cover entity)
    {
        return new CoverDocument
        {
            Type = entity.Type,
            Url = entity.Url,
            Index = entity.Index,
            Poster = entity.Poster
        };
    }

    public Cover ToEntity()
    {
        return new Cover
        {
            Type = Type,
            Url = Url,
            Index = Index,
            Poster = Poster
        };
    }
}
