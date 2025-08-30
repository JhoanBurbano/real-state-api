namespace Million.Domain.Entities;

public class OwnerSession
{
    public string Id { get; set; } = string.Empty;

    public string OwnerId { get; set; } = string.Empty;

    public string RefreshTokenHash { get; set; } = string.Empty;

    public string? Ip { get; set; }

    public string? UserAgent { get; set; }

    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public DateTime? RotatedAt { get; set; }

    // Business logic methods
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    public bool IsRevoked => RevokedAt.HasValue;

    public bool IsActive => !IsExpired && !IsRevoked;

    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
    }

    public void Rotate(string newRefreshTokenHash)
    {
        RefreshTokenHash = newRefreshTokenHash;
        RotatedAt = DateTime.UtcNow;
    }

    public static OwnerSession Create(string ownerId, string refreshTokenHash, TimeSpan ttl, string? ip = null, string? userAgent = null)
    {
        return new OwnerSession
        {
            Id = Guid.NewGuid().ToString(),
            OwnerId = ownerId,
            RefreshTokenHash = refreshTokenHash,
            Ip = ip,
            UserAgent = userAgent,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(ttl)
        };
    }
}

