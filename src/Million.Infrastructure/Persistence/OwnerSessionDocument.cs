using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Million.Domain.Entities;

namespace Million.Infrastructure.Persistence;

public class OwnerSessionDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("ownerId")]
    public string OwnerId { get; set; } = string.Empty;

    [BsonElement("refreshTokenHash")]
    public string RefreshTokenHash { get; set; } = string.Empty;

    [BsonElement("ip")]
    public string? Ip { get; set; }

    [BsonElement("userAgent")]
    public string? UserAgent { get; set; }

    [BsonElement("issuedAt")]
    public DateTime IssuedAt { get; set; }

    [BsonElement("expiresAt")]
    public DateTime ExpiresAt { get; set; }

    [BsonElement("revokedAt")]
    public DateTime? RevokedAt { get; set; }

    [BsonElement("rotatedAt")]
    public DateTime? RotatedAt { get; set; }

    public static OwnerSessionDocument FromEntity(OwnerSession entity)
    {
        return new OwnerSessionDocument
        {
            Id = entity.Id,
            OwnerId = entity.OwnerId,
            RefreshTokenHash = entity.RefreshTokenHash,
            Ip = entity.Ip,
            UserAgent = entity.UserAgent,
            IssuedAt = entity.IssuedAt,
            ExpiresAt = entity.ExpiresAt,
            RevokedAt = entity.RevokedAt,
            RotatedAt = entity.RotatedAt
        };
    }

    public OwnerSession ToEntity()
    {
        return new OwnerSession
        {
            Id = Id,
            OwnerId = OwnerId,
            RefreshTokenHash = RefreshTokenHash,
            Ip = Ip,
            UserAgent = UserAgent,
            IssuedAt = IssuedAt,
            ExpiresAt = ExpiresAt,
            RevokedAt = RevokedAt,
            RotatedAt = RotatedAt
        };
    }
}

