using FluentValidation;

namespace SolarSystem.Application.Auth;

public static class PasswordPolicy
{
    public const int MinLength = 8;
    public const int MaxLength = 128;

    /// <summary>
    /// Regra unica de senha, compartilhada por registro e futuras trocas de senha.
    /// </summary>
    public static IRuleBuilderOptions<T, string> ApplyPasswordPolicy<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(MinLength).WithMessage($"Senha deve ter no mínimo {MinLength} caracteres.")
            .MaximumLength(MaxLength).WithMessage($"Senha deve ter no máximo {MaxLength} caracteres.")
            .Matches("[A-Za-z]").WithMessage("Senha deve conter ao menos uma letra.")
            .Matches("[0-9]").WithMessage("Senha deve conter ao menos um número.");
    }
}
