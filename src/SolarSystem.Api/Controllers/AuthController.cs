using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SolarSystem.Application.Common;
using SolarSystem.Infrastructure.Services;

namespace SolarSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;

    public AuthController(IJwtService jwtService, IConfiguration configuration)
    {
        _jwtService = jwtService;
        _configuration = configuration;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new { error = "E-mail e senha são obrigatórios." });

        // TODO: Validar usuário no banco (Tenant/User entities)
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var role = "admin";

        var tokens = _jwtService.GenerateTokens(userId, tenantId, role);

        return Ok(new
        {
            accessToken = tokens.AccessToken,
            expiresAt = tokens.ExpiresAt,
            refreshToken = tokens.RefreshToken
        });
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // TODO: Implementar criação de tenant + usuário admin no banco
        return Ok(new { message = "Registro realizado com sucesso. Implementação completa pendente." });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var principal = _jwtService.ValidateToken(request.AccessToken);
        if (principal == null)
            return BadRequest(new { error = "Token inválido." });

        var userId = Guid.Parse(principal.FindFirst("user_id")!.Value);
        var tenantId = Guid.Parse(principal.FindFirst("tenant_id")!.Value);
        var role = principal.FindFirst(ClaimTypes.Role)!.Value;

        var tokens = _jwtService.GenerateTokens(userId, tenantId, role);

        return Ok(new
        {
            accessToken = tokens.AccessToken,
            expiresAt = tokens.ExpiresAt,
            refreshToken = tokens.RefreshToken
        });
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string CompanyName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class RefreshRequest
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
