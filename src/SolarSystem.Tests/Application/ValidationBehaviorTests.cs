using FluentAssertions;
using FluentValidation;
using MediatR;
using SolarSystem.Application.Common;
using SolarSystem.Application.Common.Behaviors;
using ValidationException = SolarSystem.Application.Common.Exceptions.ValidationException;

namespace SolarSystem.Tests.Application;

/// <summary>
/// Regressao: a versao anterior tentava montar Result.Failure por reflection e batia em
/// AmbiguousMatchException, transformando toda falha de validacao em HTTP 500.
/// </summary>
public class ValidationBehaviorTests
{
    private const string NomeObrigatorio = "Nome é obrigatório.";

    public record ComandoComValor(string Nome) : IRequest<Result<string>>;
    public record ComandoSemValor(string Nome) : IRequest<Result>;

    private class ValidadorComValor : AbstractValidator<ComandoComValor>
    {
        public ValidadorComValor() => RuleFor(x => x.Nome).NotEmpty().WithMessage(NomeObrigatorio);
    }

    private class ValidadorSemValor : AbstractValidator<ComandoSemValor>
    {
        public ValidadorSemValor() => RuleFor(x => x.Nome).NotEmpty().WithMessage(NomeObrigatorio);
    }

    [Fact]
    public async Task Lanca_ValidationException_para_resposta_generica()
    {
        var behavior = new ValidationBehavior<ComandoComValor, Result<string>>(new[] { new ValidadorComValor() });

        var act = async () => await behavior.Handle(
            new ComandoComValor(""),
            _ => Task.FromResult(Result.Success("nunca chega aqui")),
            CancellationToken.None);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().ContainKey(nameof(ComandoComValor.Nome));
    }

    [Fact]
    public async Task Lanca_ValidationException_tambem_para_Result_nao_generico()
    {
        var behavior = new ValidationBehavior<ComandoSemValor, Result>(new[] { new ValidadorSemValor() });

        var act = async () => await behavior.Handle(
            new ComandoSemValor(""),
            _ => Task.FromResult(Result.Success()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Agrupa_os_erros_por_campo()
    {
        var behavior = new ValidationBehavior<ComandoComValor, Result<string>>(new[] { new ValidadorComValor() });

        var act = async () => await behavior.Handle(
            new ComandoComValor(""),
            _ => Task.FromResult(Result.Success("x")),
            CancellationToken.None);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors[nameof(ComandoComValor.Nome)].Should().Contain(NomeObrigatorio);
    }

    [Fact]
    public async Task Deixa_passar_quando_o_comando_e_valido()
    {
        var behavior = new ValidationBehavior<ComandoComValor, Result<string>>(new[] { new ValidadorComValor() });

        var resultado = await behavior.Handle(
            new ComandoComValor("Guilherme"),
            _ => Task.FromResult(Result.Success("chegou no handler")),
            CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().Be("chegou no handler");
    }
}
