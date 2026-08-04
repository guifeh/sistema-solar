using FluentValidation;
using MediatR;
using SolarSystem.Application.Common;
using SolarSystem.Application.Common.Interfaces;
using SolarSystem.Domain.Identity;

namespace SolarSystem.Application.Auth.Commands;

public record RegisterCommand(
    string CompanyName,
    string Email,
    string Password,
    string Name
) : IRequest<Result<AuthDto>>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Nome da empresa é obrigatório.")
            .MaximumLength(200).WithMessage("Nome da empresa deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.")
            .MaximumLength(255).WithMessage("E-mail deve ter no máximo 255 caracteres.");

        RuleFor(x => x.Password).ApplyPasswordPolicy();
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthDto>> Handle(RegisterCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _userRepository.GetByEmailAsync(email, ct) is not null)
            return Result.Failure<AuthDto>("E-mail já cadastrado.");

        var tenant = Tenant.Create(request.CompanyName.Trim());
        var user = User.Create(
            tenant.Id,
            email,
            request.Name.Trim(),
            _passwordHasher.HashPassword(request.Password),
            "admin");

        var tokens = _jwtService.GenerateTokens(user.Id, user.TenantId, user.Role);

        // Tenant + usuario admin + refresh token nascem juntos ou nao nascem.
        await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            await _tenantRepository.CreateAsync(tenant, token);
            await _userRepository.CreateAsync(user, token);
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
