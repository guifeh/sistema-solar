using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SolarSystem.Application.Common.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SolarSystem.Infrastructure.Services;

public class JwtService : IJwtService
{
    public const int AccessTokenMinutes = 15;
    public const int RefreshTokenDays = 30;

    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public TokenResult GenerateTokens(Guid userId, Guid tenantId, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("user_id", userId.ToString()),
            new Claim("tenant_id", tenantId.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var expiresAt = DateTime.UtcNow.AddMinutes(AccessTokenMinutes);
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        // Guid nao e criptograficamente aleatorio; refresh token e credencial de longa
        // duracao, entao vem do RNG seguro.
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        return new TokenResult(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt,
            refreshToken,
            DateTime.UtcNow.AddDays(RefreshTokenDays));
    }
}
