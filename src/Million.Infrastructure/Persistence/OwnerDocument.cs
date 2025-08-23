using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Million.Domain.Entities;

namespace Million.Infrastructure.Persistence;

public class OwnerDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("fullName")]
    public string FullName { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("phoneE164")]
    public string? PhoneE164 { get; set; }

    [BsonElement("photoUrl")]
    public string? PhotoUrl { get; set; }

    [BsonElement("role")]
    [BsonRepresentation(BsonType.Int32)]
    public OwnerRole Role { get; set; }

    [BsonElement("isActive")]
    public bool IsActive { get; set; }

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    public static OwnerDocument FromEntity(Owner entity)
    {
        return new OwnerDocument
        {
            Id = entity.Id,
            FullName = entity.FullName,
            Email = entity.Email,
            PhoneE164 = entity.PhoneE164,
            PhotoUrl = entity.PhotoUrl,
            Role = entity.Role,
            IsActive = entity.IsActive,
            PasswordHash = entity.PasswordHash,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public Owner ToEntity()
    {
        return new Owner
        {
            Id = Id,
            FullName = FullName,
            Email = Email,
            PhoneE164 = PhoneE164,
            PhotoUrl = PhotoUrl,
            Role = Role,
            IsActive = IsActive,
            PasswordHash = PasswordHash,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt
        };
    }
}

