using Million.Application.DTOs.Auth;
using Million.Application.DTOs;
using Million.Domain.Entities;

namespace Million.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, string? ip = null, string? userAgent = null, CancellationToken ct = default);
    Task<LoginResponse> RefreshAsync(RefreshRequest request, string? ip = null, string? userAgent = null, CancellationToken ct = default);
    Task LogoutAsync(string refreshToken, CancellationToken ct = default);
    Task<Owner> CreateOwnerAsync(CreateOwnerRequest request, CancellationToken ct = default);
    Task<Owner> UpdateOwnerAsync(string id, UpdateOwnerRequest request, CancellationToken ct = default);
    Task<Owner> UpdateOwnerProfileAsync(string id, UpdateOwnerProfileRequest request, CancellationToken ct = default);
    Task<Owner?> GetOwnerByIdAsync(string id, CancellationToken ct = default);
    Task<Owner?> GetOwnerByEmailAsync(string email, CancellationToken ct = default);
    Task<List<Owner>> GetOwnersAsync(string? query = null, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<List<OwnerSession>> GetOwnerSessionsAsync(string ownerId, CancellationToken ct = default);
    Task RevokeSessionAsync(string sessionId, CancellationToken ct = default);
    Task<bool> ValidateAccessTokenAsync(string token, CancellationToken ct = default);
    Task<Owner?> GetOwnerFromTokenAsync(string token, CancellationToken ct = default);
}

