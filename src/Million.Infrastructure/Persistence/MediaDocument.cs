using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Million.Domain.Entities;

namespace Million.Infrastructure.Persistence;

[BsonIgnoreExtraElements]
public class MediaDocument
{
    [BsonElement("Id")]
    public string Id { get; set; } = string.Empty;

    [BsonElement("Type")]
    [BsonRepresentation(BsonType.Int32)]
    public MediaType Type { get; set; } = MediaType.Image;

    [BsonElement("Url")]
    public string Url { get; set; } = string.Empty;

    [BsonElement("Index")]
    public int Index { get; set; }

    [BsonElement("Enabled")]
    public bool Enabled { get; set; } = true;

    [BsonElement("Featured")]
    public bool Featured { get; set; } = false;

    // Image-specific properties
    [BsonElement("Variants")]
    public MediaVariantsDocument? Variants { get; set; }

    // Video-specific properties (future)
    [BsonElement("Poster")]
    public string? Poster { get; set; }

    [BsonElement("Duration")]
    public int? Duration { get; set; } // in seconds

    public static MediaDocument FromEntity(Media entity)
    {
        return new MediaDocument
        {
            Id = entity.Id,
            Type = entity.Type,
            Url = entity.Url,
            Index = entity.Index,
            Enabled = entity.Enabled,
            Featured = entity.Featured,
            Variants = entity.Variants != null ? MediaVariantsDocument.FromEntity(entity.Variants) : null,
            Poster = entity.Poster,
            Duration = entity.Duration
        };
    }

    public Media ToEntity()
    {
        return new Media
        {
            Id = Id,
            Type = Type,
            Url = Url,
            Index = Index,
            Enabled = Enabled,
            Featured = Featured,
            Variants = Variants?.ToEntity(),
            Poster = Poster,
            Duration = Duration
        };
    }
}

[BsonIgnoreExtraElements]
public class MediaVariantsDocument
{
    [BsonElement("Small")]
    public string? Small { get; set; }

    [BsonElement("Medium")]
    public string? Medium { get; set; }

    [BsonElement("Large")]
    public string? Large { get; set; }

    public static MediaVariantsDocument FromEntity(MediaVariants entity)
    {
        return new MediaVariantsDocument
        {
            Small = entity.Small,
            Medium = entity.Medium,
            Large = entity.Large
        };
    }

    public MediaVariants ToEntity()
    {
        return new MediaVariants
        {
            Small = Small,
            Medium = Medium,
            Large = Large
        };
    }
}
