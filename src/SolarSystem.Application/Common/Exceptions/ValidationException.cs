using FluentValidation.Results;

namespace SolarSystem.Application.Common.Exceptions;

/// <summary>
/// Lancada pelo ValidationBehavior quando um comando/query nao passa nas regras do FluentValidation.
/// O middleware global traduz para HTTP 400 com os erros agrupados por campo.
/// </summary>
public class ValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("Um ou mais campos sao invalidos.")
    {
        Errors = failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}
