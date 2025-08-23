using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Million.Application.DTOs.Auth;
using Million.Application.Interfaces;
using Million.Domain.Entities;
using Million.Domain.Exceptions;

namespace Million.Application.Services;

public class AuthService : IAuthService
{
    private readonly IOwnerRepository _ownerRepository;
    private readonly IOwnerSessionRepository _sessionRepository;
    private readonly IJwtService _jwtService;
    private readonly IPasswordService _passwordService;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    private readonly int _lockoutAttempts;
    private readonly int _lockoutWindowMinutes;
    private readonly int _refreshTokenTtlDays;

    public AuthService(
        IOwnerRepository ownerRepository,
        IOwnerSessionRepository sessionRepository,
        IJwtService jwtService,
        IPasswordService passwordService,
        IMemoryCache cache,
        IConfiguration configuration)
    {
        _ownerRepository = ownerRepository;
        _sessionRepository = sessionRepository;
        _jwtService = jwtService;
        _passwordService = passwordService;
        _cache = cache;
        _configuration = configuration;
        _lockoutAttempts = int.TryParse(configuration["AUTH_LOCKOUT_ATTEMPTS"], out var attempts) ? attempts : 5;
        _lockoutWindowMinutes = int.TryParse(configuration["AUTH_LOCKOUT_WINDOW_MIN"], out var window) ? window : 15;
        _refreshTokenTtlDays = int.TryParse(configuration["AUTH_REFRESH_TTL_DAYS"], out var ttl) ? ttl : 14;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, string? ip = null, string? userAgent = null, CancellationToken ct = default)
    {
        var owner = await _ownerRepository.GetByEmailAsync(request.Email.ToLowerInvariant(), ct);
        if (owner == null)
        {
            await IncrementFailedAttempts(request.Email, ip);
            throw new InvalidCredentialsException();
        }

        if (!owner.IsActive)
        {
            throw new AccountInactiveException();
        }

        if (IsAccountLocked(request.Email, ip))
        {
            throw new AccountLockedException();
        }

        if (!_passwordService.VerifyPassword(request.Password, owner.PasswordHash))
        {
            await IncrementFailedAttempts(request.Email, ip);
            throw new InvalidCredentialsException();
        }

        // Reset failed attempts on successful login
        await ResetFailedAttempts(request.Email, ip);

        var accessToken = _jwtService.GenerateAccessToken(owner);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenHash = _jwtService.HashRefreshToken(refreshToken);

        var session = OwnerSession.Create(owner.Id, refreshTokenHash, TimeSpan.FromDays(_refreshTokenTtlDays), ip, userAgent);
        await _sessionRepository.CreateAsync(session, ct);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["AUTH_ACCESS_TTL_MIN"] ?? "10"))
        };
    }

    public async Task<LoginResponse> RefreshAsync(RefreshRequest request, string? ip = null, string? userAgent = null, CancellationToken ct = default)
    {
        var refreshTokenHash = _jwtService.HashRefreshToken(request.RefreshToken);
        var session = await _sessionRepository.GetByRefreshTokenHashAsync(refreshTokenHash, ct);

        if (session == null || !session.IsActive)
        {
            throw new RefreshTokenRevokedException();
        }

        var owner = await _ownerRepository.GetByIdAsync(session.OwnerId, ct);
        if (owner == null || !owner.IsActive)
        {
            throw new AccountInactiveException();
        }

        // Rotate refresh token
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var newRefreshTokenHash = _jwtService.HashRefreshToken(newRefreshToken);

        session.Rotate(newRefreshTokenHash);
        await _sessionRepository.UpdateAsync(session, ct);

        var accessToken = _jwtService.GenerateAccessToken(owner);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["AUTH_ACCESS_TTL_MIN"] ?? "10"))
        };
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var refreshTokenHash = _jwtService.HashRefreshToken(refreshToken);
        var session = await _sessionRepository.GetByRefreshTokenHashAsync(refreshTokenHash, ct);

        if (session != null)
        {
            session.Revoke();
            await _sessionRepository.UpdateAsync(session, ct);
        }
    }

    public async Task<Owner> CreateOwnerAsync(CreateOwnerRequest request, CancellationToken ct = default)
    {
        if (await _ownerRepository.ExistsByEmailAsync(request.Email.ToLowerInvariant(), ct))
        {
            throw new DuplicateEmailException(request.Email);
        }

        var owner = new Owner
        {
            FullName = request.FullName,
            Email = request.Email.ToLowerInvariant(),
            PhoneE164 = request.PhoneE164,
            PhotoUrl = request.PhotoUrl,
            Role = request.Role,
            PasswordHash = _passwordService.HashPassword(request.TemporaryPassword)
        };

        return await _ownerRepository.CreateAsync(owner, ct);
    }

    public async Task<Owner> UpdateOwnerAsync(string id, UpdateOwnerRequest request, CancellationToken ct = default)
    {
        var owner = await _ownerRepository.GetByIdAsync(id, ct);
        if (owner == null)
        {
            throw new OwnerNotFoundException(id);
        }

        if (request.FullName != null)
            owner.FullName = request.FullName;
        if (request.PhoneE164 != null)
            owner.PhoneE164 = request.PhoneE164;
        if (request.PhotoUrl != null)
            owner.PhotoUrl = request.PhotoUrl;
        if (request.Role.HasValue)
            owner.Role = request.Role.Value;
        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
                owner.Activate();
            else
                owner.Deactivate();
        }

        return await _ownerRepository.UpdateAsync(owner, ct);
    }

    public async Task<Owner?> GetOwnerByIdAsync(string id, CancellationToken ct = default)
    {
        return await _ownerRepository.GetByIdAsync(id, ct);
    }

    public async Task<Owner?> GetOwnerByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _ownerRepository.GetByEmailAsync(email, ct);
    }

    public async Task<List<Owner>> GetOwnersAsync(string? query = null, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        return await _ownerRepository.FindAsync(query, page, pageSize, ct);
    }

    public async Task<List<OwnerSession>> GetOwnerSessionsAsync(string ownerId, CancellationToken ct = default)
    {
        return await _sessionRepository.GetByOwnerIdAsync(ownerId, ct);
    }

    public async Task RevokeSessionAsync(string sessionId, CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, ct);
        if (session != null)
        {
            session.Revoke();
            await _sessionRepository.UpdateAsync(session, ct);
        }
    }

    public async Task<bool> ValidateAccessTokenAsync(string token, CancellationToken ct = default)
    {
        var principal = _jwtService.ValidateAccessToken(token);
        if (principal == null) return false;

        var ownerId = principal.FindFirst("ownerId")?.Value;
        if (string.IsNullOrEmpty(ownerId)) return false;

        var owner = await _ownerRepository.GetByIdAsync(ownerId, ct);
        return owner?.IsActive == true;
    }

    public async Task<Owner?> GetOwnerFromTokenAsync(string token, CancellationToken ct = default)
    {
        var ownerId = _jwtService.GetOwnerIdFromToken(token);
        if (string.IsNullOrEmpty(ownerId)) return null;

        return await _ownerRepository.GetByIdAsync(ownerId, ct);
    }

    private Task IncrementFailedAttempts(string email, string? ip)
    {
        var key = $"failed_attempts_{email}_{ip}";
        var attempts = _cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_lockoutWindowMinutes);
            return 0;
        });

        _cache.Set(key, attempts + 1, TimeSpan.FromMinutes(_lockoutWindowMinutes));
        return Task.CompletedTask;
    }

    private Task ResetFailedAttempts(string email, string? ip)
    {
        var key = $"failed_attempts_{email}_{ip}";
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    private bool IsAccountLocked(string email, string? ip)
    {
        var key = $"failed_attempts_{email}_{ip}";
        var attempts = _cache.Get<int>(key);
        return attempts >= _lockoutAttempts;
    }
}
