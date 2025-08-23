using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Million.Domain.Entities;

namespace Million.Application.Services;

public interface IJwtService
{
    string GenerateAccessToken(Owner owner);
    string GenerateRefreshToken();
    string HashRefreshToken(string refreshToken);
    ClaimsPrincipal? ValidateAccessToken(string token);
    string? GetOwnerIdFromToken(string token);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly string _privateKey;
    private readonly string _publicKey;
    private readonly int _accessTokenTtlMinutes;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
        _issuer = _configuration["AUTH_JWT_ISSUER"] ?? "million-api";
        _audience = _configuration["AUTH_JWT_AUDIENCE"] ?? "million-app";
        _privateKey = _configuration["AUTH_JWT_PRIVATE_KEY"] ?? "";
        _publicKey = _configuration["AUTH_JWT_PUBLIC_KEY"] ?? "";
        _accessTokenTtlMinutes = int.TryParse(_configuration["AUTH_ACCESS_TTL_MIN"], out var ttl) ? ttl : 10;
    }

    public string GenerateAccessToken(Owner owner)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var privateKeyBytes = Convert.FromBase64String(_privateKey);

        var rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(privateKeyBytes, out _);

        var key = new RsaSecurityKey(rsa);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, owner.Id),
            new Claim(ClaimTypes.Email, owner.Email),
            new Claim(ClaimTypes.Name, owner.FullName),
            new Claim(ClaimTypes.Role, owner.Role.ToString()),
            new Claim("ownerId", owner.Id)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_accessTokenTtlMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = credentials
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public string HashRefreshToken(string refreshToken)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToBase64String(hashBytes);
    }

    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var publicKeyBytes = Convert.FromBase64String(_publicKey);

            var rsa = RSA.Create();
            rsa.ImportRSAPublicKey(publicKeyBytes, out _);

            var key = new RsaSecurityKey(rsa);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    public string? GetOwnerIdFromToken(string token)
    {
        var principal = ValidateAccessToken(token);
        return principal?.FindFirst("ownerId")?.Value;
    }
}

