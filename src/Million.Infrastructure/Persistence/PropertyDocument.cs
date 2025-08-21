using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Million.Infrastructure.Persistence;

public class PropertyDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string IdOwner { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string AddressProperty { get; set; } = string.Empty;

    public decimal PriceProperty { get; set; }

    public string Image { get; set; } = string.Empty;

    public static PropertyDocument FromEntity(Million.Domain.Entities.Property entity)
    {
        return new PropertyDocument
        {
            Id = entity.Id,
            IdOwner = entity.IdOwner,
            Name = entity.Name,
            AddressProperty = entity.AddressProperty,
            PriceProperty = entity.PriceProperty,
            Image = entity.Image
        };
    }

    public Million.Domain.Entities.Property ToEntity()
    {
        return new Million.Domain.Entities.Property
        {
            Id = Id,
            IdOwner = IdOwner,
            Name = Name,
            AddressProperty = AddressProperty,
            PriceProperty = PriceProperty,
            Image = Image
        };
    }
}
