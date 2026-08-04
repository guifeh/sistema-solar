using FluentValidation;
using MediatR;
using SolarSystem.Application.Common;
using SolarSystem.Application.Common.Interfaces;
using SolarSystem.Domain.Identity;

namespace SolarSystem.Application.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthDto>>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh token é obrigatório.");
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthDto>>
{
    private const string InvalidToken = "Refresh token inválido ou expirado.";

    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IJwtService jwtService,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthDto>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var stored = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, ct);
        if (stored is null || stored.IsRevoked || stored.ExpiresAt <= DateTime.UtcNow)
            return Result.Failure<AuthDto>(InvalidToken);

        var user = await _userRepository.GetByIdAsync(stored.UserId, ct);
        if (user is null || !user.IsActive)
            return Result.Failure<AuthDto>(InvalidToken);

        var tenant = await _tenantRepository.GetByIdAsync(user.TenantId, ct);
        if (tenant is null || !tenant.IsActive)
            return Result.Failure<AuthDto>(InvalidToken);

        var tokens = _jwtService.GenerateTokens(user.Id, user.TenantId, user.Role);

        // Rotacao: o token usado morre no mesmo instante em que o substituto nasce,
        // entao um refresh token vazado so serve enquanto nao for usado.
        await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            await _refreshTokenRepository.RevokeAsync(request.RefreshToken, token);
            await _refreshTokenRepository.CreateAsync(
                RefreshToken.Create(user.Id, tokens.RefreshToken, tokens.RefreshExpiresAt), token);
        }, ct);

        return Result.Success(new AuthDto(
            tokens.AccessToken,
            tokens.ExpiresAt,
            tokens.RefreshToken,
            new AuthUserDto(user.Id, user.Email, user.Name, user.Role, tenant.Id, tenant.Name)));
    }
}
