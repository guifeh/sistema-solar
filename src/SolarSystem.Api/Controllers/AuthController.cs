using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SolarSystem.Application.Common;
using SolarSystem.Application.Common.Interfaces;
using SolarSystem.Domain.Identity;
using SolarSystem.Infrastructure.Services;
using System.Security.Claims;

namespace SolarSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AuthController(
        IJwtService jwtService,
        IConfiguration configuration,
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IPasswordHasher passwordHasher)
    {
        _jwtService = jwtService;
        _configuration = configuration;
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _passwordHasher = passwordHasher;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new { error = "E-mail e senha são obrigatórios." });

        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
            return Unauthorized(new { error = "Credenciais inválidas." });

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            return Unauthorized(new { error = "Credenciais inválidas." });

        var tokens = _jwtService.GenerateTokens(user.Id, user.TenantId, user.Role);

        return Ok(new
        {
            accessToken = tokens.AccessToken,
            expiresAt = tokens.ExpiresAt,
            refreshToken = tokens.RefreshToken,
            user = new {
                id = user.Id,
                email = user.Email,
                name = user.Name,
                role = user.Role,
                tenantId = user.TenantId,
                tenantName = "Sistema Solar" // Optional: fetch actual tenant name if needed
            }
        });
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CompanyName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Todos os campos são obrigatórios." });

        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            return BadRequest(new { error = "E-mail já cadastrado." });

        var tenant = SolarSystem.Domain.Identity.Tenant.Create(request.CompanyName);
        await _tenantRepository.CreateAsync(tenant);

        var passwordHash = _passwordHasher.HashPassword(request.Password);
        var user = SolarSystem.Domain.Identity.User.Create(tenant.Id, request.Email, request.Name, passwordHash, "admin");
        await _userRepository.CreateAsync(user);

        var tokens = _jwtService.GenerateTokens(user.Id, user.TenantId, user.Role);

        return Ok(new
        {
            accessToken = tokens.AccessToken,
            expiresAt = tokens.ExpiresAt,
            refreshToken = tokens.RefreshToken,
            user = new {
                id = user.Id,
                email = user.Email,
                name = user.Name,
                role = user.Role,
                tenantId = user.TenantId,
                tenantName = tenant.Name
            }
        });
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
