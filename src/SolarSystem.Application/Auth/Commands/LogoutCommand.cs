using MediatR;
using SolarSystem.Application.Common;
using SolarSystem.Application.Common.Interfaces;

namespace SolarSystem.Application.Auth.Commands;

public record LogoutCommand(string RefreshToken) : IRequest<Result>;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ICurrentUserService _currentUser;

    public LogoutCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        ICurrentUserService currentUser)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            var stored = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, ct);

            // So revoga se o token pertence a quem esta chamando: senao qualquer usuario
            // autenticado poderia derrubar a sessao de outro chutando tokens.
            if (stored is not null && stored.UserId == _currentUser.UserId)
                await _refreshTokenRepository.RevokeAsync(request.RefreshToken, ct);
        }

        // Logout e idempotente: token ja revogado, inexistente ou ausente responde sucesso.
        return Result.Success();
    }
}
