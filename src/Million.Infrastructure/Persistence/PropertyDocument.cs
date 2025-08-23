using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Million.Domain.Entities;

namespace Million.Infrastructure.Persistence;

public class PropertyDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("ownerId")]
    public string OwnerId { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("address")]
    public string Address { get; set; } = string.Empty;

    [BsonElement("price")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Price { get; set; }

    [BsonElement("codeInternal")]
    public string CodeInternal { get; set; } = string.Empty;

    [BsonElement("year")]
    public int Year { get; set; }

    [BsonElement("status")]
    [BsonRepresentation(BsonType.Int32)]
    public PropertyStatus Status { get; set; }

    // New media system
    [BsonElement("cover")]
    public Cover Cover { get; set; } = new();

    [BsonElement("media")]
    public List<Media> Media { get; set; } = new();

    // Legacy fields for backward compatibility
    [BsonElement("coverImage")]
    public string CoverImage { get; set; } = string.Empty;

    [BsonElement("images")]
    public List<LegacyImage> Images { get; set; } = new();

    // Property details
    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("city")]
    public string City { get; set; } = string.Empty;

    [BsonElement("neighborhood")]
    public string Neighborhood { get; set; } = string.Empty;

    [BsonElement("propertyType")]
    public string PropertyType { get; set; } = string.Empty;

    [BsonElement("size")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Size { get; set; }

    [BsonElement("bedrooms")]
    public int Bedrooms { get; set; }

    [BsonElement("bathrooms")]
    public int Bathrooms { get; set; }

    [BsonElement("hasPool")]
    public bool HasPool { get; set; }

    [BsonElement("hasGarden")]
    public bool HasGarden { get; set; }

    [BsonElement("hasParking")]
    public bool HasParking { get; set; }

    [BsonElement("isFurnished")]
    public bool IsFurnished { get; set; }

    [BsonElement("availableFrom")]
    public DateTime? AvailableFrom { get; set; }

    [BsonElement("availableTo")]
    public DateTime? AvailableTo { get; set; }

    // Sales history
    [BsonElement("traces")]
    public List<PropertyTrace> Traces { get; set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    [BsonElement("isActive")]
    public bool IsActive { get; set; }

    public static PropertyDocument FromEntity(Property entity)
    {
        return new PropertyDocument
        {
            Id = entity.Id,
            OwnerId = entity.OwnerId,
            Name = entity.Name,
            Address = entity.Address,
            Price = entity.Price,
            CodeInternal = entity.CodeInternal,
            Year = entity.Year,
            Status = entity.Status,
            Cover = entity.Cover,
            Media = entity.Media,
            CoverImage = entity.CoverImage,
            Images = entity.Images,
            Description = entity.Description,
            City = entity.City,
            Neighborhood = entity.Neighborhood,
            PropertyType = entity.PropertyType,
            Size = entity.Size,
            Bedrooms = entity.Bedrooms,
            Bathrooms = entity.Bathrooms,
            HasPool = entity.HasPool,
            HasGarden = entity.HasGarden,
            HasParking = entity.HasParking,
            IsFurnished = entity.IsFurnished,
            AvailableFrom = entity.AvailableFrom,
            AvailableTo = entity.AvailableTo,
            Traces = entity.Traces,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsActive = entity.IsActive
        };
    }

    public Property ToEntity()
    {
        return new Property
        {
            Id = Id,
            OwnerId = OwnerId,
            Name = Name,
            Address = Address,
            Price = Price,
            CodeInternal = CodeInternal,
            Year = Year,
            Status = Status,
            Cover = Cover,
            Media = Media,
            CoverImage = CoverImage,
            Images = Images,
            Description = Description,
            City = City,
            Neighborhood = Neighborhood,
            PropertyType = PropertyType,
            Size = Size,
            Bedrooms = Bedrooms,
            Bathrooms = Bathrooms,
            HasPool = HasPool,
            HasGarden = HasGarden,
            HasParking = HasParking,
            IsFurnished = IsFurnished,
            AvailableFrom = AvailableFrom,
            AvailableTo = AvailableTo,
            Traces = Traces,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt,
            IsActive = IsActive
        };
    }
}
