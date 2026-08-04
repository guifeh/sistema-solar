using FluentValidation;
using MediatR;
using SolarSystem.Application.Common;
using SolarSystem.Application.Common.Interfaces;
using SolarSystem.Domain.Identity;

namespace SolarSystem.Application.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<Result<AuthDto>>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("E-mail é obrigatório.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Senha é obrigatória.");
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthDto>>
{
    private const string InvalidCredentials = "Credenciais inválidas.";

    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<Result<AuthDto>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);

        // Mesma mensagem para usuario inexistente, inativo e senha errada: nao entrega
        // ao atacante a informacao de quais e-mails existem na base.
        if (user is null || !user.IsActive || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            return Result.Failure<AuthDto>(InvalidCredentials);

        var tenant = await _tenantRepository.GetByIdAsync(user.TenantId, ct);
        if (tenant is null || !tenant.IsActive)
            return Result.Failure<AuthDto>(InvalidCredentials);

        var tokens = _jwtService.GenerateTokens(user.Id, user.TenantId, user.Role);
        await _refreshTokenRepository.CreateAsync(
            RefreshToken.Create(user.Id, tokens.RefreshToken, tokens.RefreshExpiresAt), ct);

        return Result.Success(new AuthDto(
            tokens.AccessToken,
            tokens.ExpiresAt,
            tokens.RefreshToken,
            new AuthUserDto(user.Id, user.Email, user.Name, user.Role, tenant.Id, tenant.Name)));
    }
}
