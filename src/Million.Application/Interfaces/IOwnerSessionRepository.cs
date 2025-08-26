using Million.Domain.Entities;

namespace Million.Application.Interfaces;

public interface IOwnerSessionRepository
{
    Task<OwnerSession?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<OwnerSession?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken ct = default);
    Task<List<OwnerSession>> GetByOwnerIdAsync(string ownerId, CancellationToken ct = default);
    Task<OwnerSession> CreateAsync(OwnerSession session, CancellationToken ct = default);
    Task<OwnerSession> UpdateAsync(OwnerSession session, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
    Task<bool> RevokeAsync(string id, CancellationToken ct = default);
    Task<int> CleanupExpiredSessionsAsync(CancellationToken ct = default);
}

